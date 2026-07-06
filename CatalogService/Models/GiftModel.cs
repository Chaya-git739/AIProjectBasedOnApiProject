using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace CatalogService.Models
{
    public class GiftModel
    {
        [BsonId]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? ImageUrl { get; set; }

        [Required]
        public decimal TicketPrice { get; set; }

        public int CategoryId { get; set; }
        public CategoryModel? Category { get; set; }

        public int DonorId { get; set; }
        public DonorModel? Donor { get; set; }

        public bool IsDeleted { get; set; }
    }
}
