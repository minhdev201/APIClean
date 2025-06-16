namespace ApiMES.Application.Configurations
{
    public class AppSettings
    {
        public required string MongoConnectionString { get; set; }
        public required string MongoDatabase { get; set; }
        public required string MongoCollection4Form { get; set; }
        public required string AllowedIPList { get; set; }
        public required string BaseAddress { get; set; }
        public required string ContractorFolder { get; set; }
        public required string CourseFolder { get; set; }
        public required string OfficialDispatchFolder { get; set; }
        public required string FileUpload { get; set; }
        public required string LMSApiKey { get; set; }
        public required string LMSAppId { get; set; }
        public required string LMSAPI_url { get; set; }
        public required string ModificationFolder { get; set; }
        public required string AdvanceFile { get; set; }
        public required string BPM { get; set; }
        public required string VIPGuestFolder { get; set; }
        public required string CertificatesFolder { get; set; }
        public required string ScanFolder { get; set; }
        public required string QCFolder { get; set; }
    }
}
