using CatalogService.Data;
using CatalogService.Models;
using MongoDB.Driver;

namespace CatalogService.Services
{
    public class MongoDonorRepository : IDonorRepository
    {
        private readonly IMongoCollection<DonorModel> _donors;

        public MongoDonorRepository(IMongoDatabase database)
        {
            _donors = database.GetCollection<DonorModel>(MongoCollections.Donors);
        }

        public async Task<List<DonorModel>> GetAllAsync() =>
            await _donors.Find(d => !d.IsDeleted).SortBy(d => d.Id).ToListAsync();

        public async Task<DonorModel?> GetByEmailAsync(string email) =>
            await _donors.Find(d => d.Email.ToLower() == email.ToLower() && !d.IsDeleted).FirstOrDefaultAsync();

        public async Task<DonorModel?> GetByIdAsync(int id) =>
            await _donors.Find(d => d.Id == id && !d.IsDeleted).FirstOrDefaultAsync();

        public async Task<DonorModel> AddAsync(DonorModel donor)
        {
            donor.Id = await GetNextIdAsync();
            await _donors.InsertOneAsync(donor);
            return donor;
        }

        public async Task<DonorModel> UpdateAsync(DonorModel donor)
        {
            await _donors.ReplaceOneAsync(d => d.Id == donor.Id, donor);
            return donor;
        }

        public async Task DeleteAsync(DonorModel donor)
        {
            donor.IsDeleted = true;
            await _donors.ReplaceOneAsync(d => d.Id == donor.Id, donor);
        }

        private async Task<int> GetNextIdAsync()
        {
            var maxId = await _donors.Find(FilterDefinition<DonorModel>.Empty)
                .SortByDescending(d => d.Id)
                .Project(d => d.Id)
                .FirstOrDefaultAsync();
            return maxId + 1;
        }
    }
}