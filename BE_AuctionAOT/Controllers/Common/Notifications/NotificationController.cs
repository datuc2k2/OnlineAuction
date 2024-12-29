using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.Common.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.Common.Notifications
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		private readonly NotificationsDao _notificationDao;
		private readonly AuthUtility _authUtility;

		public NotificationController(NotificationsDao notificationsDao, AuthUtility authUtility)
		{
			_notificationDao = notificationsDao;
			_authUtility = authUtility;
		}


		[Authorize]
		[HttpGet("GetType")]
		public async Task<IActionResult> GetTypeNotification()
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<GetTypeOutputDto>();
				var status = await _notificationDao.GetTypeNotifications(4);
				if (status.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(status);
				}
				output.Type = status.Type;
				return Ok(output);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpPost("CreateNotification")]
		public async Task<IActionResult> CreateNotification(CreateNotiInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<CreateNotiOutputDto>();
				var createNoti = await _notificationDao.CreateNotification(inputDto);
				if (createNoti.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(createNoti);
				}
				return Ok(output);

			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("AdminListNotification")]
		public async Task<IActionResult> GetListNotiAdmin(ListNotiAmindInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListNotiAdminOutputDto>();
				var listNoti = await _notificationDao.GetListNotiAdmin(inputDto);
				if (listNoti.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(listNoti);
				}
				output = listNoti;
				return Ok(output);

			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpPost("UserListNotification")]
		public async Task<IActionResult> GetListNotiUser(ListNotiUserInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListNotiUserOutputDto>();
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var listNoti = await _notificationDao.GetListNotiUser(inputDto, uId);
				if (listNoti.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(listNoti);
				}
				output = listNoti;
				return Ok(output);

			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("NotificationDetail")]
		public async Task<IActionResult> GetNotiDetail(NotiDetailInputDto inputDto )
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<NotiDetailOutputDto>();
				var notification = await _notificationDao.GetNotificationDetails(inputDto);
				if (notification.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(notification);
				}
				output = notification;
				return Ok(output);

			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}


	}
}
