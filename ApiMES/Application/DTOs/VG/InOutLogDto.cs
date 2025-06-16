namespace ApiMES.Application.DTOs.VG
{
    public class InOutLogDto
    {
        public required string ID { get; set; }
        public required string ApplicationID { get; set; }
        public DateTime Date { get; set; }
        public required string Detail { get; set; }
        public required string Action { get; set; }
    }
}
