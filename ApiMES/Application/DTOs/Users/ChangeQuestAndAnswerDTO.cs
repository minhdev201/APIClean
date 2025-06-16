namespace ApiMES.Application.DTOs.Users
{
    public class ChangeQuestAndAnswerDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string NewPasswordQuestion { get; set; }
    }
}
