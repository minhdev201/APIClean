namespace ApiMES.Application.DTOs.Users
{
    public class UpdatePassDTO
    {
        public string? Username { get; set; }
        public string? OldP { get; set; }
        public string? NewPassword { get; set; }
    }
}
