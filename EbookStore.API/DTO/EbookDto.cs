namespace EbookStore.API.DTO
{
    public class EbookDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? FileType { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryId { get; set; }

        public IFormFile? File { get; set; }
    }
}
