using AutoMapper;
using CatalogService.Models;
using CatalogService.Models.DTO;

namespace CatalogService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> AddAsync(CategoryDto category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name is required.", nameof(category));

            var entity = _mapper.Map<CategoryModel>(category);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<CategoryDto>(created);
        }

        public async Task<CategoryDto?> UpdateAsync(int id, CategoryDto category)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = category.Name;
            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<CategoryDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(existing);
            return true;
        }
    }
}
