namespace ApiMES.Application.Configurations
{
    public class SmtpSettings
    {
        public required string Host { get; set; }
        public required string Domain { get; set; }
        public int Port { get; set; }
        public required string User { get; set; }
        public required string Pass { get; set; }
        public required string From { get; set; }
        public required string To { get; set; }
        public required string Cc { get; set; }
    }
}
