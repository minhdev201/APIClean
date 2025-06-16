namespace ApiMES.Application.DTOs.Logs
{
    public class ProcessDTO
    {
        public required string KeyName { get; set; }
        public required string Month { get; set; }
        public required List<ProcessLog> Logs { get; set; }
    }
}
