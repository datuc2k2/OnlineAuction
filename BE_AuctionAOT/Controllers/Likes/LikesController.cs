using Microsoft.AspNetCore.Mvc;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
namespace BE_AuctionAOT.Controllers.Likes;

[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly DB_AuctionAOTContext _context;

    public LikesController(DB_AuctionAOTContext context)
    {
        _context = context;
    }

    //Toggle like
    [HttpPost]
    public async Task<ActionResult> ToggleLike(Like like)
    {
        try
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == like.UserId && l.PostId == like.PostId && l.CommentId == like.CommentId);

            if (existingLike != null)
            {
                if (existingLike.DeletedAt == null)
                {
                    // Soft delete
                    existingLike.DeletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok("Like removed");
                }
                else
                {
                    // Restore like
                    existingLike.DeletedAt = null;
                    await _context.SaveChangesAsync();
                    return Ok("Like restored");
                }
            }
            else
            {
                await _context.Likes.AddAsync(like);
                await _context.SaveChangesAsync();
                return Ok("Like added");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



}