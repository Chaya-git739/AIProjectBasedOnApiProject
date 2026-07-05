using CatalogService.Models;

namespace CatalogService.Services
{
    public interface IDonorRepository
    {
        Task<List<DonorModel>> GetAllAsync();
        Task<DonorModel?> GetByEmailAsync(string email);
        Task<DonorModel?> GetByIdAsync(int id);
        Task<DonorModel> AddAsync(DonorModel donor);
        Task<DonorModel> UpdateAsync(DonorModel donor);
        Task DeleteAsync(DonorModel donor);
    }
}
