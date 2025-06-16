using ApiMES.Shared.Results;
using Azure.Identity;

namespace ApiMES.Application.DTOs.Users
{
    public class ValidateUserDTO : Result
    {
        public string? Username { get; set; }
        public string? Nickname { get; set; }
        public string? Email { get; set; }
    }
}
