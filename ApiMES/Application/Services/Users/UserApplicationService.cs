using ApiMES.Application.DTOs.Users;
using ApiMES.Domain.Entities;
using ApiMES.Infrastructure.DAOs;
using ApiMES.Infrastructure.Database;
using ApiMES.Shared.Results;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace ApiMES.Application.Services.Users
{
    public class UserApplicationService(ILogger<UserApplicationService> logger, UserManager<ApplicationUser> userManager, IMemoryCache memoryCache, DbHelper helper, DbConnectionFactory connectionFactory, HrmsDao hrmsDao, ImDao imDao)
    {
        // 1. Fields
        private readonly ILogger<UserApplicationService> _logger = logger;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IMemoryCache _cache = memoryCache;
        private readonly DbHelper _helper = helper;
        private readonly DbConnectionFactory _connectionFactory = connectionFactory;
        private readonly HrmsDao _hrmsDao = hrmsDao;
        private readonly ImDao _imDao = imDao;

        // 3. Public Methods
        public async Task<IEnumerable<object>> GetDepartmentsAsync(string userId, string ctype)
        {
            using var connection = _connectionFactory.CreateConnection("Beling");
            var parameters = new DynamicParameters();
            parameters.Add("UserID", userId);
            parameters.Add("Ctype", ctype);

            var result = await connection.QueryAsync("GetDepartments", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<List<EmployeeDTO>> LoadEmployeesAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(EmployeeCacheKey, out List<EmployeeDTO>? cachedEmployees) && cachedEmployees != null)
            {
                _logger.LogInformation("Đã lấy danh sách nhân viên từ cache, tổng cộng {Count} employee(s).", cachedEmployees.Count);
                return cachedEmployees;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var employees = await _hrmsDao.LoadFromDatabaseAsync();

                _logger.LogInformation("Đã load {Count} employee(s) từ cơ sở dữ liệu.", employees.Count);

                // Cấu hình cache: giữ trong 5 phút
                CacheEmployees(employees);

                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi load employees từ stored procedure.");
                return [];
            }
        }

        public async Task<List<EmployeeDTO>> GetEmployeesByDeptAsync(string id, CancellationToken cancellationToken = default)
        {
            // Tái sử dụng cache nếu có
            var employees = await LoadEmployeesAsync(cancellationToken);

            var filtered = employees
                .Where(e => string.Equals(e.DepartmentID, id, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation("Đã lọc {Count} employee(s) thuộc phòng ban {id}.", filtered.Count, id);

            return filtered;
        }

        public async Task<ValidateUserDTO> ValidateUserAsync(string username, string password)
        {
            var employees = await LoadEmployeesAsync();

            _logger.LogInformation("ValidateUser called for username: {Username}", username);

            var identityUser = await _userManager.FindByNameAsync(username);

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidateUserDTO
                {
                    IsSuccess = false,
                    Message = "Password is required",
                    Errors = ["Password is required"]
                };
            }

            if (identityUser == null)
            {
                _logger.LogWarning("Người dùng '{Username}' không tồn tại trong hệ thống Identity.", username);

                return new ValidateUserDTO
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Errors = ["User not found"]
                };
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, password);
            if (!isPasswordValid)
            {
                return new ValidateUserDTO
                {
                    IsSuccess = false,
                    Message = "Invalid password",
                    Errors = ["Invalid password"]
                };
            }

            var normalizedUsername = _helper.NormalizeUsername(username);
            var employee = employees.FirstOrDefault(e => e.EmployeeID == normalizedUsername);

            if (employee == null)
            {
                _logger.LogWarning("Người dùng xác thực thành công nhưng không có profile trong danh sách employees: {Username}", normalizedUsername);

                return new ValidateUserDTO
                {
                    IsSuccess = false,
                    Message = "Thông tin nhân viên không tồn tại trong hệ thống.",
                    Errors = ["Thông tin nhân viên không tồn tại trong hệ thống."]
                };
            }

            return new ValidateUserDTO
            {
                IsSuccess = true,
                Message = "Authentication successful",
                Errors = [],
                Username = normalizedUsername,
                Nickname = employee?.NickName ?? "",
                Email = employee?.Email ?? ""
            };
        }

        public async Task<EmployeeDTO> GetUserInfo(string id)
        {
            var employees = await LoadEmployeesAsync();
            var normalizedId = id.ToUpper();
            var employee = employees.FirstOrDefault(e => e.EmployeeID == normalizedId) ?? throw new InvalidOperationException($"EmployeeDTO with ID {id} not found.");
            return employee;
        }

        public async Task<List<EmployeeDTO>> RefreshUserAsync()
        {
            try
            {
                _logger.LogInformation("Đang làm mới danh sách nhân viên từ cơ sở dữ liệu...");

                var employeeList = await _hrmsDao.LoadFromDatabaseAsync();

                _logger.LogInformation("Đã làm mới danh sách {EmployeeCount} nhân viên.", employeeList.Count);

                // Ghi đè cache
                CacheEmployees(employeeList);

                return employeeList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi làm mới danh sách nhân viên.");
                throw;
            }
        }

        public async Task<bool> CheckTCodeAsync(string username, string tcode, CancellationToken cancellationToken = default)
        {
            return await _imDao.CheckTCodeAsync(username, tcode, cancellationToken);
        }

        public void ClearEmployeeCache()
        {
            _logger.LogInformation("Đã xóa cache danh sách nhân viên.");
            _cache.Remove(EmployeeCacheKey);
        }

        // 4. Private Methods
        private const string EmployeeCacheKey = "EMPLOYEE_LIST";

        private void CacheEmployees(List<EmployeeDTO> employees)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(EmployeeCacheKey, employees, cacheOptions);
        }

        // 5. Difference
        public async Task<Result> ChangePasswordAsync(UpdatePassDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.OldP) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Result.Fail("Missing required fields.");
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Result.Fail("User not found.");
            }

            user.Password = model.NewPassword;
            var result = await _userManager.ChangePasswordAsync(user, model.OldP, model.NewPassword);

            if (result.Succeeded)
            {
                return Result.Ok("Password reset complete.");
            }

            var errorDescriptions = result.Errors
                .Select(error => $"{error.Code} - {error.Description}")
                .ToList();

            _logger.LogWarning("Password change failed for user {Username}: {Errors}", model.Username, string.Join(", ", errorDescriptions));

            return Result.Fail(errorDescriptions);
        }

        public async Task<Result> ChangeSecurityQuestionAsync(ChangeQuestAndAnswerDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.NewPasswordQuestion))
            {
                return Result.Fail("Missing required fields.");
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Result.Fail("User not found.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                return Result.Fail("Invalid password.");
            }

            user.PasswordQuestion = model.NewPasswordQuestion;

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                return Result.Ok("Nickname updated successfully.");
            }

            foreach (var error in updateResult.Errors)
            {
                _logger.LogError("Update user error: {Code} - {Description}", error.Code, error.Description);
            }

            return Result.Fail("Failed to update security question.");
        }
    }
}