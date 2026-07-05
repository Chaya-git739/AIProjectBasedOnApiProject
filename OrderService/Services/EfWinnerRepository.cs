using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services
{
    public class EfWinnerRepository : IWinnerRepository
    {
        private readonly OrderDbContext _context;

        public EfWinnerRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<WinnerModel> AddWinnerAsync(WinnerModel winner)
        {
            _context.Winners.Add(winner);
            await _context.SaveChangesAsync();
            return winner;
        }

        public async Task<List<WinnerModel>> GetAllWinnersAsync()
        {
            return await _context.Winners.ToListAsync();
        }

        public async Task<bool> IsGiftAlreadyWonAsync(int giftId)
        {
            return await _context.Winners.AnyAsync(w => w.GiftId == giftId);
        }
    }
}
