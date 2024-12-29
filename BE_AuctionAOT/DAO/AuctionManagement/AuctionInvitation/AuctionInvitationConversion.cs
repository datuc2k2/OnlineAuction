using AutoMapper;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation
{
	public class AuctionInvitationConversion : Profile
	{
		public AuctionInvitationConversion() {
			CreateMap<Models.AuctionInvitation, AuctionInvitationDto>()
				.ForMember(dest => dest.Auction, opt => opt.MapFrom(src => src.Auction)).ReverseMap();

			CreateMap<Models.Auction, AuctionDto>().ReverseMap();
		}
	}
}
