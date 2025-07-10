namespace EbookStore.API.DTO
{
    public class BlogPostDto
    {
        public int PostId { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }
        public IFormFile? File { get; set; }

        public string? Image { get; set; } // ImageUrl

        public DateTime? CreatedAt { get; set; }
    }
}
