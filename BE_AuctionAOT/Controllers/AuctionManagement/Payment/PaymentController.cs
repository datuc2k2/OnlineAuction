using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionManagement.Dispute;
using BE_AuctionAOT.DAO.AuctionManagement.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BE_AuctionAOT.Controllers.AuctionManagement.Payment
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly PaymentDao _paymentDao;
		private readonly AuthUtility _authUtility;
		public PaymentController(PaymentDao paymentDao, AuthUtility authUtility)
		{
			_paymentDao = paymentDao;
			_authUtility = authUtility;
		}
		[Authorize]
		[HttpGet("GetPoint")]
		public async Task<IActionResult> GetUserPoint()
		{
			try
			{
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var point = new PointOutputDto();
				point = await _paymentDao.GetPointUser(uId);
				return Ok(point);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


		[Authorize]
		[HttpPost("List")]
		public async Task<IActionResult> GetListPayment(ListPaymentInputDto inputDto)
		{
			try
			{
				//String userId = User.Claims.FirstOrDefault(claim => claim.Type == "ID")?.Value;
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var uRole = _authUtility.GetRoleInHeader(token);
				var paymentList = new ListPaymentOutputDto();
				if (uRole == "1" || uRole == "3")
				{
					paymentList = await _paymentDao.GetListPaymentAdmin(inputDto);
					if (paymentList.ResultCd == ResultCd.FAILURE)
					{
						return BadRequest(paymentList);
					};
				}
				if (uRole == "2")
				{
					paymentList = await _paymentDao.GetListPaymentUser(inputDto, uId.ToString());
					if (paymentList.ResultCd == ResultCd.FAILURE)
					{
						return BadRequest(paymentList);
					};
				}
				return Ok(paymentList);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize]
		[HttpPost("AddPayment")]
		public async Task<IActionResult> AddPayment(PaymentInputDto paymentInputDto)
		{
			try
			{
				var addPayment = await _paymentDao.AddPaymentHistory(paymentInputDto);
				if (addPayment.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(addPayment);
				}
				return Ok(addPayment);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		[Authorize]
		[HttpPost("AddDeposit")]
		public async Task<IActionResult> AddDeposit(DepositInputDto depositInputDto)
		{
			try
			{
				var addDeposit = await _paymentDao.AddDeposit(depositInputDto);
				if (addDeposit.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(addDeposit);
				}
				return Ok(addDeposit);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		[Authorize]
		[HttpPost("ReturnDeposit")]
		public async Task<IActionResult> ReturnDeposit(DepositInputDto depositInputDto)
		{
			try
			{
				var returnDeposit = await _paymentDao.ReturnDeposit(depositInputDto);
				if (returnDeposit.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(returnDeposit);
				}
				return Ok(returnDeposit);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		[HttpPost("FeeCaculation")]
		public async Task<IActionResult> FeeCaculation(FeeCaculationInputDto inputDto)
		{
			try
			{
				var returnfee = await _paymentDao.FeeCaculation(inputDto);
				if (returnfee.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(returnfee);
				}
				return Ok(returnfee);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}


	}
}
