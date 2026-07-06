using CatalogService.Data;
using CatalogService.Models;
using MongoDB.Driver;

namespace CatalogService.Services
{
    public class MongoCategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<CategoryModel> _categories;

        public MongoCategoryRepository(IMongoDatabase database)
        {
            _categories = database.GetCollection<CategoryModel>(MongoCollections.Categories);
        }

        public async Task<List<CategoryModel>> GetAllAsync() =>
            await _categories.Find(c => !c.IsDeleted).SortBy(c => c.Id).ToListAsync();

        public async Task<CategoryModel?> GetByNameAsync(string name) =>
            await _categories.Find(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted).FirstOrDefaultAsync();

        public async Task<CategoryModel?> GetByIdAsync(int id) =>
            await _categories.Find(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync();

        public async Task<CategoryModel> AddAsync(CategoryModel category)
        {
            category.Id = await GetNextIdAsync();
            await _categories.InsertOneAsync(category);
            return category;
        }

        public async Task<CategoryModel> UpdateAsync(CategoryModel category)
        {
            await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
            return category;
        }

        public async Task DeleteAsync(CategoryModel category)
        {
            category.IsDeleted = true;
            await _categories.ReplaceOneAsync(c => c.Id == category.Id, category);
        }

        private async Task<int> GetNextIdAsync()
        {
            var maxId = await _categories.Find(FilterDefinition<CategoryModel>.Empty)
                .SortByDescending(c => c.Id)
                .Project(c => c.Id)
                .FirstOrDefaultAsync();
            return maxId + 1;
        }
    }
}