namespace ApiMES.Domain.Entities.Menu
{
    public class MenuItem
    {
        public required string id { get; set; }
        public string? parentId { get; set; }
        public string? title { get; set; }
        public string? url { get; set; }
        public string? icon { get; set; }
        public string? type { get; set; }
        public string? classes { get; set; }
        public string? img { get; set; }
        public bool? target { get; set; }
        public bool? breadcrumbs { get; set; }
        public int sortOrder { get; set; }
        public string? permissions { get; set; }
    }
}
