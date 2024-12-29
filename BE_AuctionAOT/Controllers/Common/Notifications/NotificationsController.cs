using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.Controllers.Common.Notifications;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly DB_AuctionAOTContext _context;

    public NotificationsController(DB_AuctionAOTContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetNotifications()
    {
        return Ok(await _context.Notifications.OrderByDescending(n => n.CreatedAt).ToListAsync());

    }

    [HttpGet("Users")]
    public async Task<ActionResult> GetNotificationsByUser([FromQuery] int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId || n.UserId == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        if (notifications == null)
        {
            return NotFound();
        }

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetNotification(long id)
    {
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    [HttpPost]
    public async Task<ActionResult<List<Notification>>> PostNotification(NotificationDto notificationDto)
    {

        // check userIds have value or not
        if (notificationDto.UserIds.Count == 0)
        {
            var notification = new Notification
            {
                UserId = notificationDto.UserId,
                Title = notificationDto.Title,
                Content = notificationDto.Content,
                Type = notificationDto.Type,
                IsRead = false,
                IsActive = true,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return await _context.Notifications.OrderByDescending(n => n.CreatedAt).Take(1).ToListAsync();
        }

        for (int i = 0; i < notificationDto.UserIds.Count; i++)
        {
            var notification = new Notification
            {
                UserId = notificationDto.UserIds[i],
                Title = notificationDto.Title,
                Content = notificationDto.Content,
                Type = notificationDto.Type,
                IsRead = false,
                IsActive = true,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        return await _context.Notifications.OrderByDescending(n => n.CreatedAt).Take(notificationDto.UserIds.Count).ToListAsync();

    }


    [HttpPut("{id}/setRead")]
    public async Task<ActionResult> SetRead(long id)
    {
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(notification);
    }
}

