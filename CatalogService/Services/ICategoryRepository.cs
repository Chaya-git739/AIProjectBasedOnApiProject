using CatalogService.Models;

namespace CatalogService.Services
{
    public interface ICategoryRepository
    {
        Task<List<CategoryModel>> GetAllAsync();
        Task<CategoryModel?> GetByNameAsync(string name);
        Task<CategoryModel?> GetByIdAsync(int id);
        Task<CategoryModel> AddAsync(CategoryModel category);
        Task<CategoryModel> UpdateAsync(CategoryModel category);
        Task DeleteAsync(CategoryModel category);
    }
}
