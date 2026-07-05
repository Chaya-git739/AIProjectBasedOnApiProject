namespace CatalogService.Models.DTO
{
    public class GiftDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal TicketPrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
    }
}
