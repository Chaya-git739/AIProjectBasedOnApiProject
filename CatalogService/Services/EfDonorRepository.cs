using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public class EfDonorRepository : IDonorRepository
    {
        private readonly CatalogDbContext _context;

        public EfDonorRepository(CatalogDbContext context) => _context = context;

        public async Task<List<DonorModel>> GetAllAsync() =>
            await _context.Donors.AsNoTracking().OrderBy(d => d.Id).ToListAsync();

        public async Task<DonorModel?> GetByEmailAsync(string email) =>
            await _context.Donors.FirstOrDefaultAsync(d => d.Email.ToLower() == email.ToLower());

        public async Task<DonorModel?> GetByIdAsync(int id) =>
            await _context.Donors.FindAsync(id);

        public async Task<DonorModel> AddAsync(DonorModel donor)
        {
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();
            return donor;
        }

        public async Task<DonorModel> UpdateAsync(DonorModel donor)
        {
            _context.Donors.Update(donor);
            await _context.SaveChangesAsync();
            return donor;
        }

        public async Task DeleteAsync(DonorModel donor)
        {
            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();
        }
    }
}
