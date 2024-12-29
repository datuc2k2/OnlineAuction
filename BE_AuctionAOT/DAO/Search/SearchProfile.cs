using AutoMapper;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.Search
{

    public class SearchProfile : Profile
    {
        public SearchProfile()
        {
            // Map User entity to AuctionUserDto
            CreateMap<User, AuctionUserDto>();

            // Map AuctionImage entity to AuctionImagetDto
            CreateMap<AuctionImage, AuctionImagetDto>();

            // Map Auction entity to AuctionSearchResultDto
            CreateMap<Auction, AuctionSearchResultDto>()
               .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
               .ForMember(dest => dest.AuctionImages, opt => opt.MapFrom(src => src.AuctionImages))
               .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            CreateMap<User, PostUserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.UserProfile!.Avatar));
        }
    }
}
