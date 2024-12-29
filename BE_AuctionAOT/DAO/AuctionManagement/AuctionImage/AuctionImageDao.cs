using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionImage
{
    public class AuctionImageDao
    {
        private readonly DB_AuctionAOTContext _context;
        public AuctionImageDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }


        public BaseOutputDto SaveImgProduct(List<Models.AuctionImage> inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

                _context.AddRange(inputDto);
                _context.SaveChanges();
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public BaseOutputDto UpdateImgProduct(long newAucId, int index)
        {
            throw new NotImplementedException();
        }

        public BaseOutputDto DeleteImgProduct(List<Models.AuctionImage> deletedUrls)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

                _context.RemoveRange(
                    _context.AuctionImages
                    .Where(x => x.AuctionId == deletedUrls[0].AuctionId 
                    && deletedUrls.Select(a => a.ImageUrl).ToList()
                    .Contains(x.ImageUrl))
                    .ToList());
                _context.SaveChanges();
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }
    }
}
