namespace EbookStore.API.DTO
{
    public class EbookResponseDto
    {
        public int EbookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
