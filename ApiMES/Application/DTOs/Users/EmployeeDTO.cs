namespace ApiMES.Application.DTOs.Users
{
    public class EmployeeDTO
    {
        public required string EmployeeID { get; set; }
        public required string Sex { get; set; }
        public required string Name { get; set; }
        public string? DepartmentID { get; set; }
        public string? Specification { get; set; }
        public string? PositionName { get; set; }
        public string? Email { get; set; }
        public string? Company { get; set; }
        public string? NickName { get; set; }
    }
}
