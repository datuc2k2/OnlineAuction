using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Controllers.Posts;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace BE_AuctionAOT.DAO.PostManagement.Post
{
    public class PostDao
    {
        private readonly DB_AuctionAOTContext _context;
        public PostDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }

        public async Task<CreatePostOutputDto> CreatePost(Models.Post post)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<CreatePostOutputDto>();
                await _context.AddAsync(post);
                await _context.SaveChangesAsync();
                var newId = post.Id;
                output.Id = newId;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<CreatePostOutputDto>();
            }
        }

        public BaseOutputDto SaveImgPost(List<PostImage> inputDto)
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
    }
}
