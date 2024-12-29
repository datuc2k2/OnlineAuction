using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using BE_AuctionAOT.DAO.PostManagement.Post;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using Microsoft.AspNetCore.Authorization;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.DAO.Common.SystemConfiguration;
using BE_AuctionAOT.DAO.Common.Category;

namespace BE_AuctionAOT.Controllers.Posts;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{

    private readonly DB_AuctionAOTContext _context;
    private readonly PostDao _postDao;
    private readonly CategoryDao _categoryDao;
    private readonly AuthUtility _authUtility;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly SystemConfigurationDao _systemConfigurationDao;
    public PostsController(DB_AuctionAOTContext context, PostDao postDao, CategoryDao categoryDao, BlobServiceClient blobServiceClient, AuthUtility authUtility, SystemConfigurationDao systemConfigurationDao)
    {
        _context = context;
        _postDao = postDao;
        _authUtility = authUtility;
        _blobServiceClient = blobServiceClient;
        _categoryDao = categoryDao;
        _systemConfigurationDao = systemConfigurationDao;
    }

    [HttpGet("getPostCategory")]
    public async Task<IActionResult> GetAuctionCategory()
    {
        try
        {
            var output = this.Output(ResultCd.SUCCESS).Create<GetPostCategoryOutputDto>();

            var type = await _systemConfigurationDao.GetValue("0009");
            var cate = _categoryDao.GetCategory(int.Parse(type));
            if (cate.ResultCd != ResultCd.SUCCESS)
            {
                return BadRequest(cate);
            }
            output.Categories = cate.Categories;

            return Ok(output);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetPosts([FromQuery] bool isAdmin)
    {
        try
        {
            var postsQuery = from post in _context.Posts
                             join user in _context.UserProfiles on post.UserId equals user.UserId
                             where isAdmin || post.Status == PostStatus.Approved
                             select new
                             {
                                 post_id = post.Id,
                                 title = post.Title,
                                 content = post.Content,
                                 status = post.Status,
                                 category = new Dropdown { Id = (long)post.CategoryId, Name = _context.Categories.Where(x => x.CategoryId == post.CategoryId).Select(x => x.CategoryName).First() },
                                 Images = _context.PostImages.Where(x => x.PostId == post.Id)
                                 .OrderBy(x => x.SortOrder)
                                 .Select(x => new
                                 {
                                     MediaUrl = x.MediaUrl,
                                     MediaType = x.MediaType,
                                 }).ToList(),
                                 create_at = post.CreatedAt,
                                 total_comment = _context.Comments.Count(c => c.PostId == post.Id),
                                 total_like = _context.Likes.Count(l => l.PostId == post.Id && l.DeletedAt == null),
                                 owner = new
                                 {
                                     name = user.FullName,
                                     avatar = user.Avatar
                                 },
                             };

            var posts = await postsQuery.OrderByDescending(x => x.create_at).ToListAsync();

            return Ok(new { data = posts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while fetching posts.", details = ex.Message });
        }
    }

    [HttpGet("getPostByUserId/{UserId}")]
    public async Task<ActionResult> GetPostByUserId(int UserId)
    {
        try
        {
            var postsQuery = from post in _context.Posts
                             join user in _context.UserProfiles on post.UserId equals user.UserId
                             where post.Status == PostStatus.Approved && post.UserId == UserId
                             select new
                             {
                                 post_id = post.Id,
                                 title = post.Title,
                                 content = post.Content,
                                 status = post.Status,
                                 category = new Dropdown { Id = (long)post.CategoryId, Name = _context.Categories.Where(x => x.CategoryId == post.CategoryId).Select(x => x.CategoryName).First() },
                                 Images = _context.PostImages.Where(x => x.PostId == post.Id)
                                 .OrderBy(x => x.SortOrder)
                                 .Select(x => new
                                 {
                                     MediaUrl = x.MediaUrl,
                                     MediaType = x.MediaType,
                                 }).ToList(),
                                 create_at = post.CreatedAt,
                                 total_comment = _context.Comments.Count(c => c.PostId == post.Id),
                                 total_like = _context.Likes.Count(l => l.PostId == post.Id && l.DeletedAt == null),
                                 owner = new
                                 {
                                     name = user.FullName,
                                     avatar = user.Avatar
                                 },
                             };

            var posts = await postsQuery.OrderByDescending(x => x.create_at).ToListAsync();

            return Ok(new { data = posts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while fetching posts.", details = ex.Message });
        }
    }

    [HttpGet("getPostByUserIdAndAuctionId")]
    public async Task<ActionResult> GetPostByUserId(int UserId, int AuctionId)
    {
        try
        {
            var postsQuery = from post in _context.Posts
                             join user in _context.UserProfiles on post.UserId equals user.UserId
                             join auctionPost in _context.AuctionPosts on post.Id equals auctionPost.PostId
                             where post.Status == PostStatus.Approved && post.UserId == UserId && auctionPost.AuctionId == AuctionId
                             select new
                             {
                                 post_id = post.Id,
                                 title = post.Title,
                                 content = post.Content,
                                 status = post.Status,
                                 category = new Dropdown { Id = (long)post.CategoryId, Name = _context.Categories.Where(x => x.CategoryId == post.CategoryId).Select(x => x.CategoryName).First() },
                                 Images = _context.PostImages.Where(x => x.PostId == post.Id)
                                 .OrderBy(x => x.SortOrder)
                                 .Select(x => new
                                 {
                                     MediaUrl = x.MediaUrl,
                                     MediaType = x.MediaType,
                                 }).ToList(),
                                 create_at = post.CreatedAt,
                                 total_comment = _context.Comments.Count(c => c.PostId == post.Id),
                                 total_like = _context.Likes.Count(l => l.PostId == post.Id && l.DeletedAt == null),
                                 owner = new
                                 {
                                     name = user.FullName,
                                     avatar = user.Avatar
                                 },
                             };

            var posts = await postsQuery.OrderByDescending(x => x.create_at).ToListAsync();

            return Ok(new { data = posts });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while fetching posts.", details = ex.Message });
        }
    }

    [HttpPost("stickPostToAuction")]
    public async Task<ActionResult> StickPostToAuction(StickPostDto stickPostDto)
    {
        try
        {
            var allAuctionPostByAuctionId = _context.AuctionPosts.Where(ap => ap.AuctionId == stickPostDto.AuctionId).ToList();
            _context.RemoveRange(allAuctionPostByAuctionId);
            _context.SaveChanges();

            //Stick Post 
            foreach(var postId in stickPostDto.PostIds)
            {
                AuctionPost auctionPost = new AuctionPost()
                {
                    PostId = postId,
                    AuctionId = stickPostDto.AuctionId,
                    CreatedAt = DateTime.Now,
                };
                _context.Add(auctionPost);
            }
            _context.SaveChanges();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while fetching posts.", details = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreatePost([FromForm]CreatePostInputDto inputDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var uId = _authUtility.GetIdInHeader(token);

            var inputCreate = new Models.Post
            {
                Title = inputDto.Post.Title,
                Content = inputDto.Post.Content,
                CategoryId = inputDto.Post.CategoryId,
                CreatedAt = DateTime.Now,
                UserId = uId,
            };
            CreatePostOutputDto outputDto = await _postDao.CreatePost(inputCreate);

            if (outputDto.ResultCd != ResultCd.SUCCESS)
            {
                return BadRequest(outputDto);
            }

            string containerName = "postimg";
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var addedUrls = new List<PostImage>();
            var count = -1;
            if(inputDto.Images != null)
            {
                foreach (var file in inputDto.Images)
                {
                    if (file != null && file.Length > 0)
                    {
                        var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                        BlobClient blobClient = containerClient.GetBlobClient(blobName);

                        var blobHttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType
                        };

                        using (var stream = file.OpenReadStream())
                        {
                            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                        }
                        string fileUrl = blobClient.Uri.ToString();
                        addedUrls.Add(new PostImage()
                        {
                            PostId = outputDto.Id,
                            MediaUrl = fileUrl,
                            MediaType = Path.GetExtension(file.FileName),
                            SortOrder = ++count
                        });
                    }
                }
                if (addedUrls.Count > 0)
                {
                    var saveImgOutput = _postDao.SaveImgPost(addedUrls);
                    if (saveImgOutput.ResultCd != ResultCd.SUCCESS)
                    {
                        return Ok(saveImgOutput);
                    }
                }
            }

            return Ok(new { data = inputDto.Post });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while creating post.", details = ex.Message });
        }
    }


    [HttpPut]
    public async Task<ActionResult> UpdatePostStatus(int PostId, int PostStatus)
    {
        try
        {
            var postToUpdate = await _context.Posts.FindAsync(PostId);
            if (postToUpdate == null)
            {
                return NotFound(new { error = "Post not found." });
            }

            postToUpdate.Status = (PostStatus) PostStatus;
            await _context.SaveChangesAsync();

            return Ok(new { data = postToUpdate });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while updating post status.", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        try
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound(new { error = "Post not found." });
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while deleting post.", details = ex.Message });
        }
    }


}