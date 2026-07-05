namespace CatalogService.Models.DTO
{
    public class DonorDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
