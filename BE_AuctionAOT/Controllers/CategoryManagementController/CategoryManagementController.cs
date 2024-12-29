using BE_AuctionAOT.DAO.CategoryManagement;
using Microsoft.AspNetCore.Mvc;
using BE_AuctionAOT.Common.Constants;

namespace BE_AuctionAOT.Controllers.CategoryManagementController
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryManagementController : ControllerBase
    {
        private readonly CategoryManagementDao _categoryManagementDao;
        public CategoryManagementController(CategoryManagementDao categoryManagementDao)
        {
            _categoryManagementDao = categoryManagementDao;
        }

        [HttpGet]
        [Route("GetListByType/{type}")]
        public async Task<IActionResult> GetListByType(int type)
        {
            try
            {
                var data = await _categoryManagementDao.GetListByType(type);
                if (data.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(data);
                }
                return Ok(data);    
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
