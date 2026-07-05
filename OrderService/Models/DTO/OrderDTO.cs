using System.ComponentModel.DataAnnotations;

namespace OrderService.Models.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsDraft { get; set; }
        public double TotalAmount { get; set; }

        [Required(ErrorMessage = "יש לספק פריטי הזמנה")]
        [MinLength(1, ErrorMessage = "יש להוסיף לפחות פריט אחד להזמנה")]
        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }

    public class OrderItemDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "GiftId חייב להיות גדול מאפס")]
        public int GiftId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity חייב להיות גדול מאפס")]
        public int Quantity { get; set; } = 1;
    }
}
