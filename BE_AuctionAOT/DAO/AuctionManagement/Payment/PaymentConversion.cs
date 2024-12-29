using AutoMapper;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class PaymentConversion : Profile
	{
		public PaymentConversion()
		{
			CreateMap<UserProfile, UserProfileDto>().ReverseMap();

			CreateMap<User, UserDto>()
			.ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.UserProfile)).ReverseMap();

			CreateMap<PaymentHistory, PaymentHistoryDto>()
				.ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User)).ReverseMap();

			CreateMap<PointTransaction, PointTransactionDto>()
				.ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User)).ReverseMap();
		}

	}

}
