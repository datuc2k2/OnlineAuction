using AutoMapper;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest
{
	public class AuctionRequestConversion : Profile
	{
		public AuctionRequestConversion()
		{
			CreateMap<Models.AuctionRequest, AuctionRequestDto>()
				.ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
				.ForMember(dest => dest.Auction, opt => opt.MapFrom(src => src.Auction))
				.ReverseMap();

			CreateMap<Models.User, UserDto>()
				.ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.UserProfile)).ReverseMap();
			CreateMap<Models.UserProfile, UserProfileDto>().ReverseMap();

			CreateMap<Models.Auction, AuctionDto>()
				.ForMember(dest => dest.AuctionImages, opt => opt.MapFrom(src => src.AuctionImages)).ReverseMap();

			CreateMap<Models.AuctionImage, AuctionImageDto>().ReverseMap();
		}
	}
}
