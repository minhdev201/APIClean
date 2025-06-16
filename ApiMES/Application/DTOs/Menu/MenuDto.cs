namespace ApiMES.Application.DTOs.Menu
{
    public class MenuDto
    {
        public required string id { get; set; }
        public string? title { get; set; }
        public string? type { get; set; }
        public string? icon { get; set; }
        public string? img { get; set; }
        public string? url { get; set; }
        public string? classes { get; set; }
        public bool? target { get; set; }
        public bool? breadcrumbs { get; set; }
        public List<string>? permissions { get; set; }
        public List<MenuDto> children { get; set; } = new List<MenuDto>();
    }
}
