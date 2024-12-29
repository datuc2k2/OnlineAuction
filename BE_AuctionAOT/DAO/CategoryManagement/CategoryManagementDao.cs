using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
namespace BE_AuctionAOT.DAO.CategoryManagement
{
    public class CategoryManagementDao
    {
        private readonly DB_AuctionAOTContext _context;
        public CategoryManagementDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }
        public async Task<CategoryOutputDto> GetListByType(int type)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<CategoryOutputDto>();
                output.Categories = await (from row in _context.Categories
                                           where row.Type == type
                                           orderby row.CategoryId descending
                                           select row).ToListAsync();
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<CategoryOutputDto>();
            }
        }
    }
}
