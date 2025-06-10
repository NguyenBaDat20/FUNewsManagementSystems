using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services;

namespace ODataAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [EnableQuery(PageSize = 5)]
        [HttpGet("GetOData")]
        public async Task<IActionResult> Get()
        {
            var articles = await _service.GetCategories();
            return Ok(articles.AsQueryable());
        }
    }

}
