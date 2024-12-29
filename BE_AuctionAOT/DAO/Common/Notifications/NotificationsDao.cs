using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.DAO.Common.Notifications
{
	public class NotificationsDao
	{

		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;
		public NotificationsDao(DB_AuctionAOTContext context, IMapper mapper)
		{
			_mapper = mapper;
			_context = context;
		}

		public async Task<GetTypeOutputDto> GetTypeNotifications(long type)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<GetTypeOutputDto>();

				var status = _context.Categories.Where(x => x.Type == type).Select(x => new Dropdown()
				{
					Id = x.CategoryId,
					Name = x.CategoryName,
				}).ToList();
				output.Type = status;
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<GetTypeOutputDto>();
			}
		}

		public async Task<CreateNotiOutputDto> CreateNotification(CreateNotiInputDto input)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<CreateNotiOutputDto>();
				Models.Notification notification = new Models.Notification();
				notification.Title = input.title;
				if (input.type == 14)
				{
					notification.UserId = 1;
				}
				else
				{
					notification.UserId = input.userId;
				}
				notification.Type = input.type;
				notification.Content = input.description;
				_context.Add(notification);
				await _context.SaveChangesAsync();
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co loi ", parameters).WithException(ex).Create<CreateNotiOutputDto>();
			}
		}
		public async Task<ListNotiAdminOutputDto> GetListNotiAdmin(ListNotiAmindInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListNotiAdminOutputDto>();
				var listNotiAdmin = await _context.Notifications.Where(o => (inputDto.type == null || o.Type == inputDto.type) &&
																(!inputDto.startDate.HasValue || o.CreatedAt >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
																(!inputDto.endDate.HasValue || o.CreatedAt <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue)))
																.OrderByDescending(n => n.CreatedAt)
																.ToListAsync();
				var allRecord = listNotiAdmin.Count();
				listNotiAdmin = listNotiAdmin.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();

				output.allRecords = allRecord;
				output.Notifications = listNotiAdmin;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co loi ", parameters).WithException(ex).Create<ListNotiAdminOutputDto>();
			}
		}
		public async Task<ListNotiUserOutputDto> GetListNotiUser(ListNotiUserInputDto inputDto,long userId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListNotiUserOutputDto>();
				var listNotiUser = await _context.Notifications.Where(o=> o.UserId == userId || o.UserId == 1).OrderByDescending(n => n.CreatedAt)
																		.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
																		.Take((int)inputDto.DisplayCount.PageCount).ToListAsync();
				output.Notifications = listNotiUser;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co loi ", parameters).WithException(ex).Create<ListNotiUserOutputDto>();
			}
		}
		public async Task<NotiDetailOutputDto> GetNotificationDetails(NotiDetailInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<NotiDetailOutputDto>();
				var notiDetail = await _context.Notifications.FirstOrDefaultAsync(o=>o.NotificationId == inputDto.notiId);
				output.Notification = notiDetail;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co loi ", parameters).WithException(ex).Create<NotiDetailOutputDto>();
			}
		}
	}
}
