using AutoMapper;
using CatalogService.Models;
using CatalogService.Models.DTO;

namespace CatalogService.Mappings
{
    public class CatalogMappingProfile : Profile
    {
        public CatalogMappingProfile()
        {
            CreateMap<CategoryModel, CategoryDto>();
            CreateMap<CategoryDto, CategoryModel>();

            CreateMap<DonorModel, DonorDto>();
            CreateMap<DonorDto, DonorModel>();

            CreateMap<GiftModel, GiftDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.DonorName, opt => opt.MapFrom(s => s.Donor != null ? s.Donor.Name : string.Empty));

            CreateMap<GiftDto, GiftModel>()
                .ForMember(d => d.Category, opt => opt.Ignore())
                .ForMember(d => d.Donor, opt => opt.Ignore())
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.DonorId, opt => opt.Ignore());
        }
    }
}
