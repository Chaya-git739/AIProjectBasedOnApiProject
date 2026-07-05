using AutoMapper;
using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> AddAsync(CategoryDto category);
        Task<CategoryDto?> UpdateAsync(int id, CategoryDto category);
        Task<bool> DeleteAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly CatalogDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(CatalogDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> AddAsync(CategoryDto category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new ArgumentException("Category name is required.", nameof(category));
            }

            var entity = _mapper.Map<CategoryModel>(category);
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task<CategoryDto?> UpdateAsync(int id, CategoryDto category)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return null;
            existing.Name = category.Name;
            await _context.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return false;
            _context.Categories.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
