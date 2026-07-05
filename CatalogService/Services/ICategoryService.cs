using CatalogService.Models.DTO;

namespace CatalogService.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> AddAsync(CategoryDto category);
        Task<CategoryDto?> UpdateAsync(int id, CategoryDto category);
        Task<bool> DeleteAsync(int id);
    }
}
