using ApiMES.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace ApiMES.Domain.Entities.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string? MobileAlias { get; set; }

        public int IsAnonymous { get; set; }

        public DateTime LastActivityDate { get; set; }

        public required string Password { get; set; }

        public int PasswordFormat { get; set; }

        public string? PasswordSalt { get; set; }

        public string? MobilePIN { get; set; }

        public string? PasswordQuestion { get; set; }

        public string? PasswordAnswer { get; set; }

        public int IsApproved { get; set; }

        public int IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        public int FailedPasswordAnswerAttemptCount { get; set; }

        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        public string? Comment { get; set; }

        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
