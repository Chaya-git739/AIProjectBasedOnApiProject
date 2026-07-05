using AutoMapper;
using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public interface IGiftService
    {
        Task<List<GiftDto>> GetAllAsync();
        Task<GiftDto> AddAsync(GiftDto gift);
        Task<GiftDto?> UpdateAsync(int id, GiftDto gift);
        Task<bool> DeleteAsync(int id);
    }

    public class GiftService : IGiftService
    {
        private readonly CatalogDbContext _context;
        private readonly IMapper _mapper;

        public GiftService(CatalogDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<GiftDto>> GetAllAsync()
        {
            var gifts = await _context.Gifts
                .AsNoTracking()
                .Include(g => g.Category)
                .Include(g => g.Donor)
                .OrderBy(g => g.Id)
                .ToListAsync();

            return _mapper.Map<List<GiftDto>>(gifts);
        }

        public async Task<GiftDto> AddAsync(GiftDto gift)
        {
            if (string.IsNullOrWhiteSpace(gift.Name) || string.IsNullOrWhiteSpace(gift.Category) || string.IsNullOrWhiteSpace(gift.DonorName))
            {
                throw new ArgumentException("Gift name, category, and donor name are required.", nameof(gift));
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == gift.Category.ToLower());
            if (category == null)
            {
                category = new CategoryModel { Name = gift.Category };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email.ToLower() == gift.DonorName.ToLower());
            if (donor == null)
            {
                donor = new DonorModel { Name = gift.DonorName, Email = gift.DonorName };
                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
            }

            var entity = _mapper.Map<GiftModel>(gift);
            entity.CategoryId = category.Id;
            entity.DonorId = donor.Id;
            _context.Gifts.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<GiftDto>(entity);
        }

        public async Task<GiftDto?> UpdateAsync(int id, GiftDto gift)
        {
            var existing = await _context.Gifts.FindAsync(id);
            if (existing == null) return null;
            existing.Name = gift.Name;
            existing.Description = gift.Description;
            existing.ImageUrl = gift.ImageUrl;
            existing.TicketPrice = gift.TicketPrice;
            await _context.SaveChangesAsync();
            return _mapper.Map<GiftDto>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Gifts.FindAsync(id);
            if (existing == null) return false;
            _context.Gifts.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
