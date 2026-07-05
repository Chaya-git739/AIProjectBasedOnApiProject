namespace OrderService.Models
{
    public class OrderTicketModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public OrderModel Order { get; set; }
        public int GiftId { get; set; }
        public GiftModel Gift { get; set; }
        public int Quantity { get; set; }
    }
}
