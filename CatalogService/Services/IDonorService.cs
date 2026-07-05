using CatalogService.Models.DTO;

namespace CatalogService.Services
{
    public interface IDonorService
    {
        Task<List<DonorDto>> GetAllAsync();
        Task<DonorDto> AddAsync(DonorDto donor);
        Task<DonorDto?> UpdateAsync(int id, DonorDto donor);
        Task<bool> DeleteAsync(int id);
    }
}
