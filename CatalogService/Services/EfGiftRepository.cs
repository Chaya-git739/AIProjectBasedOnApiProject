using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public class EfGiftRepository : IGiftRepository
    {
        private readonly CatalogDbContext _context;

        public EfGiftRepository(CatalogDbContext context) => _context = context;

        public async Task<List<GiftModel>> GetAllAsync() =>
            await _context.Gifts
                .AsNoTracking()
                .Include(g => g.Category)
                .Include(g => g.Donor)
                .OrderBy(g => g.Id)
                .ToListAsync();

        public async Task<GiftModel?> GetByIdAsync(int id) =>
            await _context.Gifts.FindAsync(id);

        public async Task<GiftModel> AddAsync(GiftModel gift)
        {
            _context.Gifts.Add(gift);
            await _context.SaveChangesAsync();
            return gift;
        }

        public async Task<GiftModel> UpdateAsync(GiftModel gift)
        {
            _context.Gifts.Update(gift);
            await _context.SaveChangesAsync();
            return gift;
        }

        public async Task DeleteAsync(GiftModel gift)
        {
            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();
        }
    }
}
