using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace CatalogService.Models
{
    public class DonorModel
    {
        [BsonId]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        public List<GiftModel>? Gifts { get; set; }
        public bool IsDeleted { get; set; }
    }
}
