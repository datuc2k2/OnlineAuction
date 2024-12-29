using BE_AuctionAOT.DAO.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.Search
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchDao _searchDao;

        public SearchController(SearchDao searchDao)
        {
            _searchDao = searchDao;
        }

        [HttpGet]
        public IActionResult Search([FromQuery] string keyword)
        {
            var results = _searchDao.Search(keyword);

            return Ok(results);
        }
        [HttpGet("auctions")]
        public IActionResult SearchAuctions([FromQuery] string keyword)
        {

            var results = _searchDao.SearchAuctions(keyword);
            return Ok(results);
        }
        [HttpGet("users")]
        public IActionResult SearchUsers([FromQuery] string keyword)
        {
            var results = _searchDao.SearchUsers(keyword);
            return Ok(results);
        }
        [HttpGet("posts")]
        public IActionResult SearchPosts([FromQuery] string keyword)
        {
            var results = _searchDao.SearchPosts(keyword);
            return Ok(results);
        }
    }
}
