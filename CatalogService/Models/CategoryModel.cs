using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public List<GiftModel> Gifts { get; set; } = new();
        public bool IsDeleted { get; set; }
    }
}
