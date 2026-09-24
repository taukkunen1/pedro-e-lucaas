using System;
using System.Collections.Generic;

namespace ConquerSite.Models.DTOs
{
    public class UpdatePostDTO
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Category { get; set; } = "Update";
        public string Version { get; set; } = "";
        public DateTime PublishedAt { get; set; }
        public bool Featured { get; set; }
        public List<string> Changes { get; set; } = new();
    }

    public class UpdatesPageDTO
    {
        public List<UpdatePostDTO> Posts { get; set; } = new();
    }
}
