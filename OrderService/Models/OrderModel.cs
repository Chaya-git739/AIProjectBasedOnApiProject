namespace OrderService.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsDraft { get; set; }
        public List<OrderTicketModel> OrderItems { get; set; } = new();
        public DateTime OrderDate { get; set; }
        public double TotalAmount { get; set; }
    }
}
