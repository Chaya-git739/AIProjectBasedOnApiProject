namespace BffService.Services;

public class OrderServiceContract
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int GiftId { get; set; }
        public int Quantity { get; set; }
    }
}
