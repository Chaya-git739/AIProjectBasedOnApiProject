using AutoMapper;
using CatalogService.Models;
using CatalogService.Models.DTO;

namespace CatalogService.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _giftRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDonorRepository _donorRepository;
        private readonly IMapper _mapper;

        public GiftService(
            IGiftRepository giftRepository,
            ICategoryRepository categoryRepository,
            IDonorRepository donorRepository,
            IMapper mapper)
        {
            _giftRepository = giftRepository;
            _categoryRepository = categoryRepository;
            _donorRepository = donorRepository;
            _mapper = mapper;
        }

        public async Task<List<GiftDto>> GetAllAsync()
        {
            var gifts = await _giftRepository.GetAllAsync();
            return _mapper.Map<List<GiftDto>>(gifts);
        }

        public async Task<GiftDto?> GetByIdAsync(int id)
        {
            var gift = await _giftRepository.GetByIdAsync(id);
            return gift == null ? null : _mapper.Map<GiftDto>(gift);
        }

        public async Task<GiftDto> AddAsync(GiftDto gift)
        {
            if (string.IsNullOrWhiteSpace(gift.Name) || string.IsNullOrWhiteSpace(gift.Category) || string.IsNullOrWhiteSpace(gift.DonorName))
                throw new ArgumentException("Gift name, category, and donor name are required.", nameof(gift));

            // upsert logic belongs in the service — repository only does plain Add/Get
            var category = await _categoryRepository.GetByNameAsync(gift.Category)
                ?? await _categoryRepository.AddAsync(new CategoryModel { Name = gift.Category });

            var donor = await _donorRepository.GetByEmailAsync(gift.DonorName)
                ?? await _donorRepository.AddAsync(new DonorModel { Name = gift.DonorName, Email = gift.DonorName });

            var entity = _mapper.Map<GiftModel>(gift);
            entity.CategoryId = category.Id;
            entity.DonorId = donor.Id;

            var created = await _giftRepository.AddAsync(entity);
            return _mapper.Map<GiftDto>(created);
        }

        public async Task<GiftDto?> UpdateAsync(int id, GiftDto gift)
        {
            var existing = await _giftRepository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = gift.Name;
            existing.Description = gift.Description;
            existing.ImageUrl = gift.ImageUrl;
            existing.TicketPrice = gift.TicketPrice;

            var updated = await _giftRepository.UpdateAsync(existing);
            return _mapper.Map<GiftDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _giftRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _giftRepository.DeleteAsync(existing);
            return true;
        }
    }
}
