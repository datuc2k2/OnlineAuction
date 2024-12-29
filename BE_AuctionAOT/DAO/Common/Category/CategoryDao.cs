using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.Common.Category
{
    public class CategoryDao
    {
        private readonly DB_AuctionAOTContext _context;
        public CategoryDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }

        public GetAuctionCategoryOutputDto GetCategory(int type)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetAuctionCategoryOutputDto>();

                var cate = _context.Categories.Where(x => x.Type == type).Select(x => new Dropdown()
                {
                    Id = x.CategoryId,
                    Name = x.CategoryName,
                }).ToList();
                output.Categories = cate;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<GetAuctionCategoryOutputDto>();
            }
        }
    }
}
