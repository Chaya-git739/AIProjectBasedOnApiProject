using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public class EfCategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context;

        public EfCategoryRepository(CatalogDbContext context) => _context = context;

        public async Task<List<CategoryModel>> GetAllAsync() =>
            await _context.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

        public async Task<CategoryModel?> GetByNameAsync(string name) =>
            await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

        public async Task<CategoryModel?> GetByIdAsync(int id) =>
            await _context.Categories.FindAsync(id);

        public async Task<CategoryModel> AddAsync(CategoryModel category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<CategoryModel> UpdateAsync(CategoryModel category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task DeleteAsync(CategoryModel category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
