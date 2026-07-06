namespace OrderService.Models.DTO
{
    public class OrderDetailsSourceDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }
}