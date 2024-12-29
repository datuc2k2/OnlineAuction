using AutoMapper;

namespace BE_AuctionAOT.DAO.AccountManagement.Account
{
    public class AccountConversion : Profile
    {
        public AccountConversion()
        {
            CreateMap<Models.User, UserDto>()
                .ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.UserProfile)).ReverseMap();
            CreateMap<Models.UserProfile, UserProfileDto>().ReverseMap();
        }
    }
}
