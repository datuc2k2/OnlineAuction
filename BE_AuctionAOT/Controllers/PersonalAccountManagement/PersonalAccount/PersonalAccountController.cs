using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE_AuctionAOT.Controllers.Personal_Account_Management.Personal_Account
{
	[Route("api/[controller]")]
	[ApiController]
	public class PersonalAccountController : ControllerBase
	{
		private readonly PersonalDao _personalDao;

		private readonly AuthUtility _authUtility;
		private readonly ImageEncryptionService _imageEncryptionService;
		private readonly Ekyc _ekyc;
		public PersonalAccountController(PersonalDao personalDao, AuthUtility authUtility, ImageEncryptionService imageEncryptionService, Ekyc ekyc)
		{
			_personalDao = personalDao;
			_authUtility = authUtility;
			_imageEncryptionService = imageEncryptionService;
			_ekyc = ekyc;
		}

		[Authorize]
		[HttpGet("GetProfile")]
		public async Task<IActionResult> GetUserProfile()
		{
			try
			{
				//String accountId = User.Claims.FirstOrDefault(claim => claim.Type == "ID")?.Value;
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var accountList = new PersonalDaoOutputDto();
				accountList = await _personalDao.GetProfile(uId.ToString());
				if (accountList.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accountList);
				};

				return Ok(accountList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpPost("EditProfile")]
		public async Task<IActionResult> EditUserProfile([FromForm] PersonalDaoInputDto Input)
		{
			try
			{
				//String accountId = User.Claims.FirstOrDefault(claim => claim.Type == "ID")?.Value;
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var accountList = new PersonalDaoOutputDto();
				accountList = await _personalDao.EditProfile(uId.ToString(), Input);
				if (accountList.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(accountList);
				};

				return Ok(accountList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpPost("eKyc")]
		public async Task<IActionResult> eKyc([FromForm] EkycInputDto Input)
		{
			try
			{
				var token_web = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token_web);
				var profileUser = new GetFrontIdCardOutputDto();
				profileUser = await _personalDao.GetFrontIdCard(uId.ToString());
				if (profileUser.ResultCd == ResultCd.FAILURE)
				{
					return BadRequest(profileUser);
				}
				var access_token = await _ekyc.GetAccessTokenAsync();
				if (access_token == null)
				{
					return BadRequest(access_token);
				}
				var hash_cccd = await _ekyc.UploadBase64ImageAsync(access_token, profileUser.FrontIdCard, $"front id card {uId}", "Upload to get hash cccd front", uId);
				if (hash_cccd == null)
				{
					return BadRequest(hash_cccd);
				}
				var hash_portrait = await _ekyc.UploadFileAsync(access_token, Input.portrait_img, $"portait user {uId}", "upload portrait to get hash");
				if (hash_portrait == null)
				{
					return BadRequest(hash_portrait);
				}

				CompareFaceResult compareFaceResult = await _ekyc.CompareFaceAsync(access_token, hash_cccd, hash_portrait, Input.clientSession);
				if (compareFaceResult == null)
				{
					return BadRequest(compareFaceResult);
				}
				else
				{
					var ekycUser = _personalDao.IsEkyc(uId);
					bool isEkyc = await ekycUser;
					if (isEkyc == false) {
						return BadRequest(ekycUser);
					}
				}
				return Ok(compareFaceResult);

			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
	}
}
