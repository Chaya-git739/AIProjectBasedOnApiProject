using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace CatalogService.Models
{
    public class CategoryModel
    {
        [BsonId]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public List<GiftModel> Gifts { get; set; } = new();
        public bool IsDeleted { get; set; }
    }
}
