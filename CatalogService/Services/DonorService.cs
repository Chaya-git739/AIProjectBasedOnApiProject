using AutoMapper;
using CatalogService.Data;
using CatalogService.Models;
using CatalogService.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Services
{
    public interface IDonorService
    {
        Task<List<DonorDto>> GetAllAsync();
        Task<DonorDto> AddAsync(DonorDto donor);
        Task<DonorDto?> UpdateAsync(int id, DonorDto donor);
        Task<bool> DeleteAsync(int id);
    }

    public class DonorService : IDonorService
    {
        private readonly CatalogDbContext _context;
        private readonly IMapper _mapper;

        public DonorService(CatalogDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<DonorDto>> GetAllAsync()
        {
            var donors = await _context.Donors.AsNoTracking().OrderBy(d => d.Id).ToListAsync();
            return _mapper.Map<List<DonorDto>>(donors);
        }

        public async Task<DonorDto> AddAsync(DonorDto donor)
        {
            if (string.IsNullOrWhiteSpace(donor.Name) || string.IsNullOrWhiteSpace(donor.Email))
            {
                throw new ArgumentException("Donor name and email are required.", nameof(donor));
            }

            var entity = _mapper.Map<DonorModel>(donor);
            _context.Donors.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<DonorDto>(entity);
        }

        public async Task<DonorDto?> UpdateAsync(int id, DonorDto donor)
        {
            var existing = await _context.Donors.FindAsync(id);
            if (existing == null) return null;
            existing.Name = donor.Name;
            existing.Email = donor.Email;
            existing.Address = donor.Address;
            await _context.SaveChangesAsync();
            return _mapper.Map<DonorDto>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Donors.FindAsync(id);
            if (existing == null) return false;
            _context.Donors.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
