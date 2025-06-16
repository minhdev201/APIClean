using ApiMES.Application.DTOs.VG;
using ApiMES.Application.Services.ACS;
using ApiMES.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiMES.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VGController(ILogger<VGController> logger, VgApplicationService vgService) : ControllerBase
    {
        private readonly ILogger<VGController> _logger = logger;
        private readonly VgApplicationService _vgService = vgService;

        [HttpGet("secure")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            return Ok(new { message = "Bạn đã truy cập API bảo vệ thành công!" });
        }

        [HttpPost("SaveInOutLog")]
        public async Task<IActionResult> SaveInOutLog([FromBody] InOutLogDto log)
        {
            var result = await _vgService.SaveInOutLogAsync(log);
            return Ok(result);
        }

        [HttpGet("GetDepartment")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetDataDepartment(string EmployeeID)
        {
            try
            {
                var data = await _vgService.GetDataDepartmentAsync(EmployeeID);
                return Ok(Result<IEnumerable<dynamic>>.Ok(data, "Lấy dữ liệu phòng ban thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get department data for EmployeeID: {EmployeeID}", EmployeeID);
                return StatusCode(500, Result<IEnumerable<dynamic>>.Fail("Internal server error"));
            }
        }

        [HttpPut("ChangeStatus")]
        public async Task<ActionResult<Result>> ChangeStatus(string ApplicationID, int Status)
        {
            try
            {
                var result = await _vgService.ChangeStatus(ApplicationID, Status);
                _logger.LogInformation("ChangeStatus complete for ApplicationID: {ApplicationID}", ApplicationID);
                return Result.Ok("ChangeStatus complete.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ChangeStatus for ApplicationID: {ApplicationID}", ApplicationID);
                return Result.Fail("Internal server error");
            }
        }

        [HttpGet("GetApproverList")]
        public async Task<ActionResult<Result<IEnumerable<dynamic>>>> GetApproverList(string Type, string DepartmentID, string ApproverID)
        {
            try
            {
                var data = await _vgService.GetApproverList(Type, DepartmentID, ApproverID);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get approver list for Type: {Type}, DepartmentID: {DepartmentID}, ApproverID: {ApproverID}", Type, DepartmentID, ApproverID);
                return StatusCode(500, Result<IEnumerable<dynamic>>.Fail("Internal server error"));
            }
        }

        [HttpPut("DeleteApplication")]
        public async Task<ActionResult<Result>> DeleteApplication(string ApplicationID)
        {
            try
            {
                await _vgService.DeleteApplication(ApplicationID);
                return Result.Ok("Delete application complete.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delete application for ApplicationID: {ApplicationID}", ApplicationID);
                return StatusCode(500, Result<IEnumerable<dynamic>>.Fail("Internal server error"));
            }
        }

        [HttpPut("MailNotice")]
        public async Task<ActionResult<Result>> MailNotice(string ApplicationID, string Status, string UserList, string DenyReason)
        {
            try
            {
                await _vgService.MailNotice(ApplicationID, Status, UserList, DenyReason);
                return Result.Ok("MailNotice complete.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error MailNotice for ApplicationID: {ApplicationID}", ApplicationID);
                return StatusCode(500, Result<IEnumerable<dynamic>>.Fail("Internal server error"));
            }
        }

        [HttpGet("GetVG_Detail")]
        public async Task<ActionResult<Result>> GetVG_Detail(string ApplicationID)
        {
            try
            {
                var data = await _vgService.GetVG_Detail(ApplicationID);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delete application for ApplicationID: {ApplicationID}", ApplicationID);
                return StatusCode(500, Result<IEnumerable<dynamic>>.Fail("Internal server error"));
            }
        }

        [HttpGet("Search")]
        public async Task<ActionResult<Result>> Search(
            [FromQuery] string? ApplicationID,
            [FromQuery] string? ApplicantID,
            [FromQuery] string? TimeStart,
            [FromQuery] string? TimeEnd,
            [FromQuery] string? CompanyName,
            [FromQuery] int GuestTypeID,
            [FromQuery] string? Status)
        {
            try
            {
                // Ensure non-null values for nullable parameters
                var applicationID = ApplicationID ?? string.Empty;
                var applicantID = ApplicantID ?? string.Empty;
                var timeStart = TimeStart ?? string.Empty;
                var timeEnd = TimeEnd ?? string.Empty;
                var companyName = CompanyName ?? string.Empty;
                var status = Status ?? string.Empty;

                var data = await _vgService.Search(applicationID, applicantID, timeStart, timeEnd, companyName, GuestTypeID, status);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search with parameters: ApplicationID={ApplicationID}, ApplicantID={ApplicantID}, TimeStart={TimeStart}, TimeEnd={TimeEnd}, CompanyName={CompanyName}, GuestTypeID={GuestTypeID}, Status={Status}",
                    ApplicationID, ApplicantID, TimeStart, TimeEnd, CompanyName, GuestTypeID, Status);
                return StatusCode(500, Result<IEnumerable<dynamic>>.Fail("Internal server error"));
            }
        }
    }
}
