using AutoMapper;
using CatalogService.Models;
using CatalogService.Models.DTO;

namespace CatalogService.Services
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepository _repository;
        private readonly IMapper _mapper;

        public DonorService(IDonorRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<DonorDto>> GetAllAsync()
        {
            var donors = await _repository.GetAllAsync();
            return _mapper.Map<List<DonorDto>>(donors);
        }

        public async Task<DonorDto> AddAsync(DonorDto donor)
        {
            if (string.IsNullOrWhiteSpace(donor.Name) || string.IsNullOrWhiteSpace(donor.Email))
                throw new ArgumentException("Donor name and email are required.", nameof(donor));

            var entity = _mapper.Map<DonorModel>(donor);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<DonorDto>(created);
        }

        public async Task<DonorDto?> UpdateAsync(int id, DonorDto donor)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = donor.Name;
            existing.Email = donor.Email;
            existing.Address = donor.Address;
            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<DonorDto>(updated);
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
