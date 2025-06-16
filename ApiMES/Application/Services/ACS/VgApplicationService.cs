using ApiMES.Application.DTOs.VG;
using ApiMES.Infrastructure.DAOs.VG;

namespace ApiMES.Application.Services.ACS
{
    public class VgApplicationService(VgDao vgDao)
    {
        private readonly VgDao _vgDao = vgDao;

        public async Task<IEnumerable<dynamic>> SaveInOutLogAsync(InOutLogDto log)
        {
            return await _vgDao.SaveAsync(log);
        }

        public async Task<IEnumerable<dynamic>> GetDataDepartmentAsync(string EmployeeID)
        {
            return await _vgDao.GetDataDepartmentAsync(EmployeeID);
        }

        public async Task<IEnumerable<dynamic>> ChangeStatus(string ApplicationID, int Status)
        {
            return await _vgDao.ChangeStatus(ApplicationID, Status);
        }

        public async Task<IEnumerable<dynamic>> GetApproverList(string Type, string DepartmentID, string ApproverID)
        {
            return await _vgDao.GetApproverList(Type, DepartmentID, ApproverID);
        }

        public async Task<IEnumerable<dynamic>> DeleteApplication(string ApplicationID)
        {
            return await _vgDao.DeleteApplication(ApplicationID);
        }

        public async Task<IEnumerable<dynamic>> MailNotice(string ApplicationID, string Status, string UserList, string DenyReason)
        {
            return await _vgDao.MailNotice(ApplicationID, Status, UserList, DenyReason);
        }

        public async Task<IEnumerable<dynamic>> GetVG_Detail(string ApplicationID)
        {
            return await _vgDao.GetVG_Detail(ApplicationID);
        }

        public async Task<IEnumerable<dynamic>> Search(string ApplicationID, string ApplicantID, string TimeStart, string TimeEnd, string CompanyName, int GuestTypeID, string Status)
        {
            return await _vgDao.Search(ApplicationID, ApplicantID, TimeStart, TimeEnd, CompanyName, GuestTypeID, Status);
        }
    }
}
