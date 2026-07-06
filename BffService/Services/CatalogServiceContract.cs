namespace BffService.Services;

public class CatalogServiceContract
{
    public class GiftDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
    }
}
