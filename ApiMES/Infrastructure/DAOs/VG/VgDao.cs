using ApiMES.Application.DTOs.VG;
using ApiMES.Infrastructure.Database;
using Dapper;
using System.Data;

namespace ApiMES.Infrastructure.DAOs.VG
{
    public class VgDao(DbConnectionFactory dbFactory, ILogger<VgDao> logger)
    {
        private readonly DbConnectionFactory _dbFactory = dbFactory;
        private readonly ILogger<VgDao> _logger = logger;

        public async Task<IEnumerable<dynamic>> SaveAsync(InOutLogDto log)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");

                return await connection.QueryAsync(
                    "VG_SaveInOutLog",
                    new
                    {
                        log.ID,
                        log.ApplicationID,
                        log.Date,
                        log.Detail,
                        log.Action
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving InOutLog");
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> GetDataDepartmentAsync(string employeeId)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");

                return await connection.QueryAsync(
                    "GetDataDepartment",
                    new { employeeId },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department data for EmployeeID: {EmployeeID}", employeeId);
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> ChangeStatus(string ApplicationID, int Status)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");

                return await connection.QueryAsync(
                    "VG_ChangeStatus",
                    new { ApplicationID, Status },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ChangeStatus for ApplicationID: {ApplicationID}", ApplicationID);
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> GetApproverList(string Type, string DepartmentID, string ApproverID)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");
                return await connection.QueryAsync(
                    "GetApproverList",
                    new { Type, DepartmentID, ApproverID },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approver list");
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> DeleteApplication(string ApplicationID)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");
                return await connection.QueryAsync(
                    "VG_DeleteApplication",
                    new { ApplicationID },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approver list");
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> MailNotice(string ApplicationID, string Status, string UserList, string DenyReason)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");
                return await connection.QueryAsync(
                    "VG_@CancelApplicationNotice",
                    new { ApplicationID, Status, UserList, DenyReason },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approver list");
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> GetVG_Detail(string ApplicationID)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");
                return await connection.QueryAsync(
                    "GetVG_Detail",
                    new { ApplicationID },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approver list");
                throw;
            }
        }
        
        public async Task<IEnumerable<dynamic>> Search(string ApplicationID, string ApplicantID, string TimeStart, string TimeEnd, string CompanyName, int GuestTypeID, string Status)
        {
            try
            {
                using var connection = _dbFactory.CreateConnection("SpecialGuest");
                return await connection.QueryAsync(
                    "SearchApplications",
                    new { ApplicationID, ApplicantID, TimeStart, TimeEnd, CompanyName, GuestTypeID, Status },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approver list");
                throw;
            }
        }
    }
}
