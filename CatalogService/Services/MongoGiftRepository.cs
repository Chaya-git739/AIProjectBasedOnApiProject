using CatalogService.Data;
using CatalogService.Models;
using MongoDB.Driver;

namespace CatalogService.Services
{
    public class MongoGiftRepository : IGiftRepository
    {
        private readonly IMongoCollection<GiftModel> _gifts;
        private readonly IMongoCollection<CategoryModel> _categories;
        private readonly IMongoCollection<DonorModel> _donors;

        public MongoGiftRepository(IMongoDatabase database)
        {
            _gifts = database.GetCollection<GiftModel>(MongoCollections.Gifts);
            _categories = database.GetCollection<CategoryModel>(MongoCollections.Categories);
            _donors = database.GetCollection<DonorModel>(MongoCollections.Donors);
        }

        public async Task<List<GiftModel>> GetAllAsync()
        {
            var gifts = await _gifts.Find(g => !g.IsDeleted).SortBy(g => g.Id).ToListAsync();
            await HydrateReferencesAsync(gifts);
            return gifts;
        }

        public async Task<GiftModel?> GetByIdAsync(int id)
        {
            var gift = await _gifts.Find(g => g.Id == id && !g.IsDeleted).FirstOrDefaultAsync();
            if (gift != null)
            {
                await HydrateReferencesAsync(new List<GiftModel> { gift });
            }
            return gift;
        }

        public async Task<GiftModel> AddAsync(GiftModel gift)
        {
            gift.Id = await GetNextIdAsync();
            await _gifts.InsertOneAsync(gift);
            return gift;
        }

        public async Task<GiftModel> UpdateAsync(GiftModel gift)
        {
            await _gifts.ReplaceOneAsync(g => g.Id == gift.Id, gift);
            return gift;
        }

        public async Task DeleteAsync(GiftModel gift)
        {
            gift.IsDeleted = true;
            await _gifts.ReplaceOneAsync(g => g.Id == gift.Id, gift);
        }

        private async Task HydrateReferencesAsync(List<GiftModel> gifts)
        {
            var categoryIds = gifts.Select(g => g.CategoryId).Distinct().ToList();
            var donorIds = gifts.Select(g => g.DonorId).Distinct().ToList();

            var categories = await _categories.Find(c => categoryIds.Contains(c.Id) && !c.IsDeleted).ToListAsync();
            var donors = await _donors.Find(d => donorIds.Contains(d.Id) && !d.IsDeleted).ToListAsync();

            foreach (var gift in gifts)
            {
                gift.Category = categories.FirstOrDefault(c => c.Id == gift.CategoryId);
                gift.Donor = donors.FirstOrDefault(d => d.Id == gift.DonorId);
            }
        }

        private async Task<int> GetNextIdAsync()
        {
            var maxId = await _gifts.Find(FilterDefinition<GiftModel>.Empty)
                .SortByDescending(g => g.Id)
                .Project(g => g.Id)
                .FirstOrDefaultAsync();
            return maxId + 1;
        }
    }
}