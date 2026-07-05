namespace OrderService.Models.DTO
{
    public class PurchaserDetailsDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TicketsCount { get; set; }
    }
}
