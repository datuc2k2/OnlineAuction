using AutoMapper;

namespace BE_AuctionAOT.DAO.AuctionManagement.ListAuction
{
	public class ListAuctionConversion : Profile
	{
		public ListAuctionConversion()
		{
			CreateMap<Models.Auction, AuctionDto>()
				.ForMember(dest => dest.AuctionRequests, opt => opt.MapFrom(src => src.AuctionRequests)).ReverseMap();
			CreateMap<Models.AuctionRequest, AuctionRequestDto>().ReverseMap();

			CreateMap<Models.AuctionBid, AuctionBidDto>()
				.ForMember(dest => dest.Auction, opt => opt.MapFrom(src => src.Auction)).ReverseMap();
			CreateMap<Models.Auction, JoinedAuctionDto>().ReverseMap();
		}
	}
}
