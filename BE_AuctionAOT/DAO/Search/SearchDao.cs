using AutoMapper;
using AutoMapper.QueryableExtensions;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace BE_AuctionAOT.DAO.Search
{
    public class SearchDao
    {
        private readonly DB_AuctionAOTContext _context;
        private readonly IMapper _mapper;
        public SearchDao(DB_AuctionAOTContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }
        public List<SearchResultDto> Search(string keyword)
        {
            keyword = keyword.ToLower(); // Chuyển từ khóa sang chữ thường

            var auctions = _context.Auctions
                .Where(a => a.IsActive == true && a.EndTime > DateTime.Now &&
                            (a.ProductName.ToLower().Contains(keyword) ||
                             a.Description.ToLower().Contains(keyword)))
                .Select(a => new
                {
                    Result = new SearchResultDto
                    {
                        Type = "Auction",
                        Id = a.AuctionId,
                        Name = a.ProductName,
                        Description = a.Description,
                        Url = $"/auction-details?auctionId={a.AuctionId}"
                    }
                })
                .Take(4)
                .ToList();

            var users = _context.Users
                .Where(u => u.IsActive == true &&
                            u.UserRoles.Any(ur => ur.Role.RoleName == "Customer") &&
                            u.Username.ToLower().Contains(keyword))
                .Select(u => new
                {
                    Result = new SearchResultDto
                    {
                        Type = "User",
                        Id = u.UserId,
                        Name = u.Username,
                        Description = null,
                        Url = $"/user-profile/{u.UserId}"
                    }
                })
                .Take(4)
                .ToList();
            var posts = _context.Posts
                .Where(p => p.Status == PostStatus.Approved && ( p.Title.ToLower().Contains(keyword) || p.Content.ToLower().Contains(keyword)))
                .Select(p => new
                {
                    Result = new SearchResultDto
                    {
                        Type = "Post",
                        Id = p.Id,
                        Name = p.Title,
                        Description = null,
                        Url = $"/blog-detail?blogId={p.Id}"
                    }
                })
                .Take(4)
                .ToList();

            var combinedResults = auctions
                  .Concat(users)
                  .Concat(posts)
                  .Select(x => new SearchResultDto
                  {
                      Type = x.Result.Type,
                      Id = x.Result.Id,
                      Name = x.Result.Name,
                      Description = x.Result.Description,
                      Url = x.Result.Url
                  })
                 .ToList();



            return combinedResults;
        }

        public List<AuctionSearchResultDto> SearchAuctions(string keyword)
        {
            keyword = keyword.ToLower();

            var auctions = _context.Auctions
                .Include(a => a.User)
                .Where(a => a.ProductName.ToLower().Contains(keyword) || a.Description.ToLower().Contains(keyword))
                .ProjectTo<AuctionSearchResultDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(a => a.EndTime)
                .ToList();

            return auctions;
        }
        public List<UserSearchResultDto> SearchUsers(string keyword)
        {
            keyword = keyword.ToLower();

            var users = _context.Users
                .Include(u => u.AuctionReviews)
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Customer") && (u.Username.Contains(keyword) || u.Username.ToLower().Contains(keyword)))
                .Select(u => new UserSearchResultDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Avatar = u.UserProfile!.Avatar,
                    AverageRating = u.AuctionReviews.Average(review => (double?)review.Rating),
                    ReviewCount = u.AuctionReviews.Count()
                })
                .ToList();

            return users;
        }
        public List<PostSearchResultDto> SearchPosts(string keyword)
        {
            keyword = keyword.ToLower();

            var posts = _context.Posts
                .Where(p => p.Status == PostStatus.Approved && ( p.Title.ToLower().Contains(keyword) || p.Content.ToLower().Contains(keyword)))
                .Select(p => new PostSearchResultDto
                {
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    total_comment = _context.Comments.Count(c => c.PostId == p.Id),
                    total_like = _context.Likes.Count(l => l.PostId == p.Id && l.DeletedAt == null),
                    Owner = _mapper.Map<PostUserDto>(_context.Users.Include(u => u.UserProfile).FirstOrDefault(u => u.UserId == p.UserId)),
                })
                .ToList();

            return posts;
        }
    }
}
