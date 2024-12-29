using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BE_AuctionAOT.Controllers.Comments;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{

    private readonly DB_AuctionAOTContext _context;
    public CommentsController(DB_AuctionAOTContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetComments([FromQuery] int post_id)
    {
        try
        {
            // Step 1: Fetch all comments with user details in one query
            var comments = await _context.Comments
                .Where(c => c.PostId == post_id)
                .Select(c => new Comment
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId,
                    ParentId = c.ParentId,
                    PostId = c.PostId
                })
            .ToListAsync();

            // Step 2: Fetch likes for all comments in one query
            var commentIds = comments.Select(c => c.Id).ToList();
            var likes = await _context.Likes
                       .Where(l => commentIds.Contains((int)l.CommentId) && l.DeletedAt == null)
                       .GroupBy(l => l.CommentId)
                       .Select(g => new { CommentId = g.Key, TotalLike = g.Count() })
                       .ToDictionaryAsync(g => g.CommentId, g => g.TotalLike);

            var likesNonNull = likes.Where(kvp => kvp.Key.HasValue).ToDictionary(kvp => kvp.Key.Value, kvp => kvp.Value);

            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var users = await _context.UserProfiles
                .Where(u => userIds.Contains(u.UserId))
               .ToDictionaryAsync(u => u.UserId, u => u);
            // Step 3: Map comments into nested structure
            var nestedComments = comments
                .Where(c => c.ParentId == null)
                .Select(c => MapComment(c, comments, likesNonNull, users))
                .ToList();


            // Step 4: Return the response
            return Ok(new { data = nestedComments });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateComment(Comment comment)
    {
        try
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { data = comment });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateComment(Comment comment)
    {
        try
        {
            //Only allow the owner to update the comment content
            var existingComment = await _context.Comments.FindAsync(comment.Id);



            if (existingComment == null)
            {
                return NotFound(new { error = "Comment not found" });
            }


            existingComment.Content = comment.Content;
            await _context.SaveChangesAsync();

            return Ok(new { data = existingComment });

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteComment(int comment_id)
    {
        try
        {
            var existingComment = await _context.Comments.FindAsync(comment_id);
            if (existingComment == null)
            {
                return NotFound(new { error = "Comment not found" });
            }

            _context.Comments.Remove(existingComment);
            await _context.SaveChangesAsync();

            return Ok(new { data = existingComment });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }


    private object MapComment(dynamic comment, List<Comment> comments,
    Dictionary<int, int> likes, Dictionary<long, UserProfile> users)
    {
        return new
        {
            comment_id = comment.Id,
            post_id = comment.PostId,
            content = comment.Content,
            total_like = likes.ContainsKey(comment.Id) ? likes[comment.Id] : 0,
            parent_id = comment.ParentId,
            created_at = comment.CreatedAt,
            owner = new
            {
                user_id = comment.UserId,
                name = users[comment.UserId].FullName,
                avatar = users[comment.UserId].Avatar
            },
            sub_comments = comments
                .Where(c => c.ParentId == comment.Id)
                .Select(c => MapComment(c, comments, likes, users))
                .ToList()
        };
    }


}