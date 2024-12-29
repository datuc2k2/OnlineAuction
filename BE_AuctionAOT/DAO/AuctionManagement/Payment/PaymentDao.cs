using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.AuctionManagement.Dispute;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class PaymentDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;

		public PaymentDao(DB_AuctionAOTContext context, IMapper mapper)
		{
			_mapper = mapper;
			_context = context;
		}
		public async Task<ListPaymentOutputDto> GetListPaymentUser(ListPaymentInputDto inputDto, String userId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListPaymentOutputDto>();
				List<Models.PointTransaction> transactionList = await _context.PointTransactions
															.Include(a => a.User).ThenInclude(u => u.UserProfile)
															.Where(record =>
																(record.UserId == long.Parse(userId)) &&
																(string.IsNullOrEmpty(inputDto.searchName) || record.User.Username.ToLower().Contains(inputDto.searchName.ToLower())) &&
																(!inputDto.startDate.HasValue || record.TransactionTime >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
																(!inputDto.endDate.HasValue || record.TransactionTime <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue)))
															.OrderByDescending(n => n.TransactionTime).ToListAsync();
				if (transactionList == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dont have payment record").Create<ListPaymentOutputDto>();
					return output;
				}


				var totalRecords = transactionList.Count();
				//phan trang
				transactionList = transactionList.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();

				var transactionListDto = _mapper.Map<List<Models.PointTransaction>, List<PointTransactionDto>>(transactionList);
				foreach (var transaction in transactionListDto) {
					if (!string.IsNullOrEmpty(transaction.TransactionCode))
					{
						transaction.TransactionCode = transaction.TransactionCode.ToUpper();
					}
				}

				output.allRecords = totalRecords;
				output.PointTransactions = transactionListDto;

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<ListPaymentOutputDto>();
			}
		}
		public async Task<ListPaymentOutputDto> GetListPaymentAdmin(ListPaymentInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListPaymentOutputDto>();
				List<Models.PointTransaction> transactionList = await _context.PointTransactions.Include(a => a.User).ThenInclude(u => u.UserProfile)
					.Where(record =>
							(string.IsNullOrEmpty(inputDto.searchName) || record.User.Username.ToLower().Contains(inputDto.searchName.ToLower())) &&
							(!inputDto.startDate.HasValue || record.TransactionTime >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
							(!inputDto.endDate.HasValue || record.TransactionTime <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue))).OrderByDescending(n => n.TransactionTime)
					.ToListAsync();
				if (transactionList == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dont have payment record").Create<ListPaymentOutputDto>();
					return output;
				}

				var totalRecords = transactionList.Count();
				//phan trang
				transactionList = transactionList.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();

				var transactionListDto = _mapper.Map<List<Models.PointTransaction>, List<PointTransactionDto>>(transactionList);
				foreach (var transaction in transactionListDto)
				{
					if (!string.IsNullOrEmpty(transaction.TransactionCode))
					{
						transaction.TransactionCode = transaction.TransactionCode.ToUpper();
					}
				}
				output.allRecords = totalRecords;
				output.PointTransactions = transactionListDto;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<ListPaymentOutputDto>();
			}
		}
		public async Task<PointOutputDto> GetPointUser(long userId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<PointOutputDto>();
				Models.Point point = await _context.Points.FirstOrDefaultAsync(x => x.UserId == userId);
				if (point == null)
				{
					Models.Point newPoint = new Models.Point();
					newPoint.UserId = userId;
					newPoint.PointsAmount = 0;
					newPoint.Currency = "VND";
					newPoint.LastUpdated = DateTime.Now;
					_context.Points.Add(newPoint);
					await _context.SaveChangesAsync();
					output.point = newPoint;
				}
				else
				{
					output.point = point;
				}

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<PointOutputDto>();
			}
		}


		/// <summary>
		/// Dung de luu lai lich su thanh toan va cong point sau khi nap tien bang ma qr
		/// </summary>
		/// <param name="paymentInputDto"></param>
		/// <returns>Thong bao thanh cong hay khong</returns>
		public async Task<PaymentOutputDto> AddPaymentHistory(PaymentInputDto paymentInputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<PaymentOutputDto>();
				//create code transaction
				String guid = Guid.NewGuid().ToString("N").Substring(0, 16);
				//lay giao dich thuc da duoc thuc hien
				ApiResponse response = await GetPaymentReal();
				bool checkPaymentIsReal = false;
				bool checkPaymentWashave = false;
				List<Models.PaymentHistory> paymentHisCheck = await _context.PaymentHistories.Where(o => o.UserId == paymentInputDto.UserId).OrderByDescending(n => n.PaymentTime).ToListAsync();
				foreach (var payment in paymentHisCheck)
				{
					if (payment.Description == paymentInputDto.Description) {
						checkPaymentWashave=true;
						break;
					}
				}
				if (checkPaymentWashave)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Đã lưu giao dịch từ trước rồi").Create<PaymentOutputDto>();
				}
				foreach (var giaoDich in response.Data)
				{
					TimeSpan timeDifference = DateTime.Now - giaoDich.NgayDienRa;
					if (giaoDich.MoTa == paymentInputDto.Description && Math.Abs(timeDifference.TotalMinutes) < 3 && giaoDich.GiaTri == paymentInputDto.PaymentAmount)
					{
						checkPaymentIsReal = true;
						break;
					}
				}
				if (!checkPaymentIsReal)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Lỗi xác thực thanh toán").Create<PaymentOutputDto>();
				}

				Models.Point point = await _context.Points.Where(o => o.UserId == paymentInputDto.UserId).FirstOrDefaultAsync();
				if (paymentInputDto.AuctionId == 0)
				{
					//update point
					point.PointsAmount = point.PointsAmount + paymentInputDto.PaymentAmount;
					point.LastUpdated = DateTime.Now;
					//tao transaction
					Models.PointTransaction newPointTransaction = new Models.PointTransaction();
					newPointTransaction.UserId = paymentInputDto.UserId;
					newPointTransaction.Amount = paymentInputDto.PaymentAmount;
					newPointTransaction.Currency = paymentInputDto.Currency.ToString();
					newPointTransaction.TransactionTime = DateTime.Now;
					newPointTransaction.Description = paymentInputDto.Description;
					newPointTransaction.TransactionCode = guid;

					_context.Points.Update(point);
					_context.PointTransactions.Add(newPointTransaction);
					await _context.SaveChangesAsync();
				}
				else
				{
					//update auction
					Models.Auction auction = await _context.Auctions.Where(o => o.AuctionId == paymentInputDto.AuctionId).FirstOrDefaultAsync();
					auction.PaymentStatus = AuctionConst.Category.PAID;
					auction.Status = AuctionConst.Category.COMINGSOON;
					//update point, do co auctionId nen tru point luon, point ko thay doi
					point.PointsAmount = point.PointsAmount;
					point.LastUpdated = DateTime.Now;
					//tao transaction nap tien
					Models.PointTransaction newAddPointTransaction = new Models.PointTransaction();
					newAddPointTransaction.UserId = paymentInputDto.UserId;
					newAddPointTransaction.Amount = paymentInputDto.PaymentAmount;
					newAddPointTransaction.Currency = paymentInputDto.Currency.ToString();
					newAddPointTransaction.TransactionTime = DateTime.Now;
					newAddPointTransaction.Description = paymentInputDto.Description;
					newAddPointTransaction.TransactionCode = guid;
					//tao transaction tru phi tao phien
					Models.PointTransaction newMinusPointTransaction = new Models.PointTransaction();
					newMinusPointTransaction.UserId = paymentInputDto.UserId;
					newMinusPointTransaction.Amount = -paymentInputDto.PaymentAmount;
					newMinusPointTransaction.Currency = paymentInputDto.Currency.ToString();
					newMinusPointTransaction.TransactionTime = DateTime.Now;
					newMinusPointTransaction.Description = $"Trừ phí tạo phiên: AuctionId: {paymentInputDto.AuctionId}, tên phiên: {auction.ProductName}";
					newMinusPointTransaction.TransactionCode = Guid.NewGuid().ToString("N").Substring(0, 16);

					_context.Auctions.Update(auction);
					_context.Points.Update(point);
					_context.PointTransactions.Add(newAddPointTransaction);
					_context.PointTransactions.Add(newMinusPointTransaction);
					await _context.SaveChangesAsync();
				}
				//tao paymentHistory
				Models.PaymentHistory paymentHistory = new Models.PaymentHistory();
				paymentHistory.UserId = paymentInputDto.UserId;
				paymentHistory.AuctionId = paymentInputDto.AuctionId ?? 0;
				paymentHistory.PaymentAmount = paymentInputDto.PaymentAmount;
				paymentHistory.Currency = paymentInputDto.Currency;
				paymentHistory.PaymentTime = paymentInputDto.PaymentTime;
				paymentHistory.Description = paymentInputDto.Description;
				paymentHistory.AccountNumber = "00";
				paymentHistory.PaymentCode = guid;
				_context.PaymentHistories.Add(paymentHistory);
				await _context.SaveChangesAsync();

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<PaymentOutputDto>();
			}

		}
		/// <summary>
		/// Dung de tru tien coc sau khi ng dung nhan tham gia dau gia
		/// </summary>
		/// <returns> Success or fail</returns>
		public async Task<PaymentOutputDto> AddDeposit(DepositInputDto depositInputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<PaymentOutputDto>();
				//lấy point của user
				Models.Point point = await _context.Points.Where(o => o.UserId == depositInputDto.UserId).FirstOrDefaultAsync();
				if (point == null || point.PointsAmount < depositInputDto.DepositAmount)
				{
					//chưa có đủ point
					output.ResultCd = ResultCd.FAILURE;
					return output;
				}
				//tạo mới deposit
				Models.Deposit deposit = new Models.Deposit();
				deposit.UserId = depositInputDto.UserId;
				deposit.AuctionId = depositInputDto.AuctionId;
				deposit.DepositAmount = depositInputDto.DepositAmount;
				deposit.Currency = depositInputDto.Currency;
				deposit.DepositStatus = "True";
				deposit.DepositDate = DateTime.Now;
				//update point
				point.PointsAmount = point.PointsAmount - depositInputDto.DepositAmount;
				point.LastUpdated = DateTime.Now;

				_context.Deposits.Add(deposit);
				_context.Points.Update(point);
				await _context.SaveChangesAsync();

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<PaymentOutputDto>();
			}


		}
		/// <summary>
		/// Dung de hoan lai tien coc cho toan bo nguoi dung tham gia dau gia thua cuoc
		/// </summary>
		/// <returns>Success or fail</returns>
		public async Task<PaymentOutputDto> ReturnDeposit(DepositInputDto depositInputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<PaymentOutputDto>();
				List<Models.Point> pointList = _context.Points.ToList();
				List<Models.Deposit> depositList = await _context.Deposits.Where(o => o.AuctionId == depositInputDto.AuctionId).ToListAsync();
				foreach (var deposit in depositList)
				{
					foreach (var point in pointList)
					{
						if (deposit.UserId != depositInputDto.UserId && point.UserId == deposit.UserId)
						{
							point.PointsAmount = point.PointsAmount + deposit.DepositAmount;
							point.LastUpdated = DateTime.Now;

							deposit.DepositStatus = "False";
							deposit.DepositDate = DateTime.Now;

							_context.Points.Update(point);
							_context.Deposits.Update(deposit);
							await _context.SaveChangesAsync();
						}
						if (deposit.UserId == depositInputDto.UserId && point.UserId == deposit.UserId)
						{
							var auctionName = await _context.Auctions.Where(o => o.AuctionId == depositInputDto.AuctionId).Select(auc => new
							{
								auc.AuctionId,
								auc.ProductName
							}).FirstOrDefaultAsync();
							Models.PointTransaction newPointTransaction = new Models.PointTransaction();
							newPointTransaction.UserId = depositInputDto.UserId;
							newPointTransaction.Amount = -deposit.DepositAmount;
							newPointTransaction.Currency = depositInputDto.Currency.ToString();
							newPointTransaction.TransactionTime = DateTime.Now;
							newPointTransaction.Description = $"Trừ tiền cọc cho phiên đấu giá: AuctionId: {depositInputDto.AuctionId}, tên phiên: {auctionName.ProductName}";
							newPointTransaction.TransactionCode = Guid.NewGuid().ToString("N").Substring(0, 16);

							_context.PointTransactions.Add(newPointTransaction);
							await _context.SaveChangesAsync();
						}
					}
				}

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<PaymentOutputDto>();
			}
		}

		public async Task<FeeCaculationOutputDto> FeeCaculation(FeeCaculationInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<FeeCaculationOutputDto>();
				Models.Auction auction = await _context.Auctions.FirstOrDefaultAsync(o => o.AuctionId == inputDto.auctionId);
				if (auction == null)
				{
					output.ResultCd = ResultCd.FAILURE;
					return output;
				}
				var categoryDict = _context.Categories
												.Where(x => x.Type == 0)
												.Select(x => new { x.CategoryId, x.Value })
												.ToDictionary(x => x.CategoryId, x => x.Value);
				if (categoryDict.TryGetValue(auction.CategoryId, out decimal? Value))
				{
					output.Fee = Value.HasValue ? (long)Value.Value : 0;
				}
				if (output.Fee == 0)
				{
					output.ResultCd = ResultCd.FAILURE;
					return output;
				}
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<FeeCaculationOutputDto>();
			}
		}
		public async Task<ApiResponse> GetPaymentReal()
		{
			string apiUrl = "https://script.googleusercontent.com/macros/echo?user_content_key=k_30FV9OfP2BvaPsxLxNIYi6xgsP6A0KYGb_pO35C2Z1zs6I37UnVZ_P9C6Oz-MxRY2xKECKl2lV3Ye2_O4t-hXcnjM2B_bgm5_BxDlH2jW0nuo2oDemN9CCS2h10ox_1xSncGQajx_ryfhECjZEnK9toBhp6qeoCj9PERXUDuzqVAWhZ-bjiCBrO51u6xxj3Noidjx1iHmTm0EGKX7iRvT-Wau1EG4OpOp9QHeNsAYPhIuOCVWdldz9Jw9Md8uu&lib=Mtb4AvmSLWJfjON_ZMa49EArN_imPsKv7";

			try
			{
				using (HttpClient client = new HttpClient())
				{
					HttpResponseMessage response = await client.GetAsync(apiUrl);
					response.EnsureSuccessStatusCode();
					string responseContent = await response.Content.ReadAsStringAsync();
					var options = new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					};
					ApiResponse apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, options);

					return apiResponse;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
				return null;
			}
		}



	}
}
