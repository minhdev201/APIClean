using ApiMES.Application.DTOs.Users;
using ApiMES.Application.Services.Auth;
using ApiMES.Application.Services.Files;
using ApiMES.Application.Services.HealthCheck;
using ApiMES.Application.Services.Menu;
using ApiMES.Application.Services.Users;
using ApiMES.Domain.Entities;
using ApiMES.Infrastructure.DAOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace ApiMES.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HSSEController(MongoDao mongoService, ILogger<HSSEController> logger, UserApplicationService userService, UserManager<ApplicationUser> userManager, HealthCheckService healthCheck, FileUploadApplicationService fileUploadService, MenuApplicationService menuService, AuthApplicationService authApplicationService, HrmsDao hrmsDao) : ControllerBase
    {
        private readonly MongoDao _mongoService = mongoService;
        private readonly ILogger<HSSEController> _logger = logger;
        private readonly UserApplicationService _userService = userService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly HealthCheckService _healthCheck = healthCheck;
        private readonly FileUploadApplicationService _fileUploadService = fileUploadService;
        private readonly MenuApplicationService _menuService = menuService;
        private readonly AuthApplicationService _authApplicationService = authApplicationService;
        private readonly HrmsDao _hrmsDao = hrmsDao;

        [HttpPost("ValidateUser")]
        public async Task<IActionResult> ValidateUser([FromBody] UserCredentialsDto credentials)
        {
            var (isSuccess, accessToken, refreshToken, errors) =
                await _authApplicationService.ValidateUserAsync(credentials.Username, credentials.Password);

            if (!isSuccess)
                return Unauthorized(new { message = errors });

            Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(3)
            });

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Missing refresh token" });

            var (isSuccess, accessToken, newRefreshToken, errorMessage) =
                await _authApplicationService.RefreshAccessTokenAsync(refreshToken);

            if (!isSuccess)
                return Unauthorized(new { message = errorMessage });

            Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(1)
            });

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { message = "Token refreshed" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Missing refresh token" });

            var isRevoked = await _authApplicationService.RevokeRefreshTokenAsync(refreshToken);

            // Xóa cookie bất kể token có hợp lệ hay không, để đảm bảo client luôn đăng xuất được
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            if (!isRevoked)
                return BadRequest(new { message = "Invalid refresh token or already revoked." });

            return Ok(new { message = "User logged out successfully." });
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value);
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var permissions = User.FindAll("permission").Select(p => p.Value);
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                id,
                username,
                roles,
                permissions,
            });
        }

        [Authorize]
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var expClaim = User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            var username = User.Identity?.Name;
            var menuTree = await _menuService.GetMenuForUserAsync(username!);
            Console.WriteLine($"Token Expiration: {expClaim}");
            return Ok(menuTree);
        }

        [HttpGet("GetEmployeeInfo/{id}")]
        public async Task<IActionResult> GetEmployeeInfo(string id)
        {
            var result = await _userService.GetUserInfo(id);
            return Ok(result);
        }

        [HttpGet("GetEmployeesByDept/{id}")]
        public async Task<IActionResult> GetDepartments(string id)
        {
            var result = await _userService.GetEmployeesByDeptAsync(id);
            return Ok(result);
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            var result = await _fileUploadService.UploadFileAsync(file);

            if ((bool)result["error"])
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("GetDepartments")]
        public async Task<IActionResult> GetDepartments([FromQuery] string userid, [FromQuery] string ctype)
        {
            var result = await _userService.GetDepartmentsAsync(userid, ctype);
            return Ok(result);
        }

        [HttpPost("Gate/Checker/UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePassDTO model)
        {
            var result = await _userService.ChangePasswordAsync(model);

            if (result.IsSuccess)
                return Ok(result.Message);

            return BadRequest(new
            {
                result.Message,
                result.Errors
            });
        }

        [HttpPost("Gate/Checker/ChangeNickname")]
        public async Task<IActionResult> ChangeSecurityQuestion([FromBody] ChangeQuestAndAnswerDTO model)
        {
            var result = await _userService.ChangeSecurityQuestionAsync(model);

            if (result.IsSuccess)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost("set-default-password")]
        public async Task<IActionResult> SetDefaultPasswordForAllUsers()
        {
            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    if (string.IsNullOrEmpty(user.SecurityStamp))
                    {
                        user.SecurityStamp = Guid.NewGuid().ToString();
                    }

                    var passwordHasher = new PasswordHasher<ApplicationUser>();
                    var hashedPassword = passwordHasher.HashPassword(user, "123456");
                    user.PasswordHash = hashedPassword;
                    user.Password = "123456";
                    var result = await _userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("Lỗi cập nhật mật khẩu cho user {UserName}: {ErrorDescriptions}", user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            return Ok(new { Message = "Đã cập nhật mật khẩu mặc định cho người dùng chưa có PasswordHash." });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTCode(string username, string tcode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(tcode))
            {
                return BadRequest("Username and TCode are required.");
            }

            try
            {
                var hasAccess = await _userService.CheckTCodeAsync(username, tcode, cancellationToken);
                return Ok(new { success = hasAccess });
                //return Ok(hasAccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when checking TCode for user: {Username}", username);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetProcessLogs/{id}/{cId?}")]
        public async Task<IActionResult> GetProcessLogs(string id, string cId = "")
        {
            try
            {
                var logs = await _mongoService.GetProcessLogsAsync(id, cId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get process logs");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("CheckMongoConnection")]
        public async Task<IActionResult> CheckMongoConnection()
        {
            var isConnected = await _mongoService.CheckConnectionAsync();
            if (isConnected)
                return Ok("MongoDB connected successfully");
            else
                return StatusCode(500, "Failed to connect to MongoDB");
        }

        [HttpGet("CheckMongoQuery/{processId}")]
        public async Task<IActionResult> CheckMongoQuery(string processId)
        {
            var (isConnected, message, data) = await _mongoService.CheckMongoAndQueryAsync(processId);

            if (!isConnected)
            {
                return StatusCode(500, new { message, dataCount = 0 });
            }

            return Ok(new
            {
                message,
                dataCount = data.Count,
                data
            });
        }

        [HttpGet("hrms-db-health")]
        public async Task<IActionResult> CheckDatabaseHealth()
        {
            var result = await _healthCheck.TestDatabaseConnectionAsync();
            return result ? Ok("Database is healthy") : StatusCode(503, "Database is unreachable");
        }

        [HttpGet("employees")]
        public async Task<IActionResult> LoadEmployees()
        {
            var employees = await _userService.LoadEmployeesAsync();
            return Ok(employees);
        }
    }
}
