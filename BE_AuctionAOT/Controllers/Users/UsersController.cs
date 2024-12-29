using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionReviewDao;
using BE_AuctionAOT.DAO.Common.SystemMessages;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.Controllers.Users;

public class AddCommentRequest
{
    public long UserId { get; set; }
    public string Comment { get; set; }
    public long ParentId { get; set; }
    public List<IFormFile>? Images { get; set; }
}
public class AddReviewRequest
{
    public string Comment { get; set; }
    public byte Rating { get; set; }
}


[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly DB_AuctionAOTContext _context;
    private readonly AuctionReviewDao _auctionReviewDao;
    private readonly AuthUtility _authUtility;
    private readonly MessageService _messageService;
    public UsersController(DB_AuctionAOTContext context, AuctionReviewDao auctionReviewDao, AuthUtility authUtility, MessageService messageService)
    {
        _context = context;
        _auctionReviewDao = auctionReviewDao;
        _authUtility = authUtility;
        _messageService = messageService;
    }


    [HttpGet]
    public async Task<ActionResult> GetActiveUsers()
    {
        var filteredUsers = await _context.Users
        .Where(u => u.IsActive == true)
        .Select(u => new
        {
            u.UserId,
            u.Username,
            Email = u.UserProfile.Email

        }).ToListAsync();

        return Ok(filteredUsers);
    }

    [HttpGet("{userId}/reviews")]
    public IActionResult GetReviewsByUserId(long userId)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var currentUserId = _authUtility.GetIdInHeader(token);

            var reviews = _auctionReviewDao.GetReviewsByUserId(userId, currentUserId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _messageService.GetSystemMessages().First(x => x.Code == "0001").Message, error = ex.Message });
        }
    }
    [HttpPost("{userId}/reviews")]
    public IActionResult CreateReview(long userId, [FromBody] AddReviewRequest request)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var currentUserId = _authUtility.GetIdInHeader(token);

            var reviews = _auctionReviewDao.CreateReview(userId, currentUserId, request.Rating, request.Comment);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _messageService.GetSystemMessages().First(x => x.Code == "0001").Message, error = ex.Message });
        }
    }
    [HttpGet("comments/{commentId}")]
    public IActionResult GetCommentsByUserId(long commentId)
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var currentUserId = _authUtility.GetIdInHeader(token);

            var comments = _auctionReviewDao.GetCommentsByReviewId(commentId, currentUserId);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _messageService.GetSystemMessages().First(x => x.Code == "0001").Message, error = ex.Message });
        }
    }

    [HttpPost("comments")]
    public async Task<IActionResult> AddComment([FromForm] AddCommentRequest request)
    {
        var review = await _auctionReviewDao.AddComment(request);
        return Ok(review);
    }
    [HttpPut("comments/{commentId}")]
    public async Task<IActionResult> UpdateComment(long commentId, [FromForm] UpdateCommentRequest request)
    {
        var result = await _auctionReviewDao.UpdateComment(commentId, request);

        if (!result)
        {
            return BadRequest(_messageService.GetSystemMessages().First(x => x.Code == "1004").Message);
       
        }
        return Ok(_messageService.GetSystemMessages().First(x => x.Code == "1006").Message);
    }
    [HttpDelete("comments/{commentId}")]
    public IActionResult DeleteReview(long commentId)
    {
        var result = _auctionReviewDao.DeleteComment(commentId);
        if (!result)
        {
            return BadRequest(_messageService.GetSystemMessages().First(x => x.Code == "1003").Message);
        }
        return Ok(_messageService.GetSystemMessages().First(x => x.Code == "1005").Message);
    }


    [HttpGet("toggle/{reviewId}")]
    public async Task<IActionResult> ToggleLike(long reviewId)
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var currentUserId = _authUtility.GetIdInHeader(token);

        var isLiked = await _auctionReviewDao.ToggleLikeAsync(currentUserId, reviewId);

        return Ok(new { success = isLiked });
    }
}