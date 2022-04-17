using AccountingService.Models.ResourceModels;
using AccountingService.Orchestration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Mime;

namespace AccountingService.Controllers
{
    /// <summary>
    /// Categories Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<ChartOfAccountsController> _logger;
        private readonly IOrchestrator _orchestrator;

        /// <summary>
        /// Instantiates a categories controller
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> interface.</param>
        /// <param name="orchestrator">The <see cref="IOrchestrator"/> interface.</param>
        public CategoriesController(ILogger<ChartOfAccountsController> logger, IOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;
        }

        /// <summary>
        /// Get the list of categories
        /// </summary>
        /// <returns></returns>
        /// <param name="start">The first record returned in this page (if not provided, 1 is assumed)</param>
        /// <param name="page_size">The maximum number of records returned in this set (if not provided, 200 is assumed)</param>
        /// <response code="200">Returns the list of categories</response>
        /// <response code="400">Invalid start value</response>
        [HttpGet]
        [Route("categories")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(ResourceCollection<Account>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Get([FromQuery] int? start, [FromQuery] int? page_size)
        {
            var theStart = start ?? 1;
            var thePageSize = page_size ?? 200;

            if (theStart < 1)
                return BadRequest();

            _logger.LogTrace("start={start}  page_size={pagesize}", theStart, thePageSize);

            _orchestrator.Request = Request;
            ResourceCollection<Category> collection = await _orchestrator.GetCategoriesAsync(theStart, thePageSize);

            return Ok(collection);
        }

        /// <summary>
        /// Get a category by id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("categories/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(Account))]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetSingle(string id)
        {
            try
            {
                _orchestrator.Request = Request;
                Category theCategory = await _orchestrator.GetCategoryAsync(id);
                return Ok(theCategory);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Get a category by id, and all its children
        /// </summary>
        /// <param name="id">The id of the parent category</param>
        /// <param name="page_size">The page size of a paged collection (defaults to 200)</param>
        /// <param name="start">The record number of the first record in the page (defaults to 1)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("categories/children/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(ResourceCollection<Category>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetCategoryAndChildren([FromQuery] int? start, [FromQuery] int? page_size, string id)
        {
            var theStart = start ?? 1;
            var thePageSize = page_size ?? 200;

            _orchestrator.Request = Request;
            ResourceCollection<Category> collection = await _orchestrator.GetCategoryAndChildrenAsync(theStart, thePageSize, id);
            return Ok(collection);
        }

        /// <summary>
        /// Globally change the category id of a category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("categories/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> ChangeCategoryId(string id, [FromBody] NewCategoryId newId)
        {
            try
            {
                _orchestrator.Request = Request;
                await _orchestrator.ChangeCategoryIdAsync(id, newId.CategoryId);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Adds a category to the list of categories
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("categories")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            try
            {
                _orchestrator.Request = Request;
                var modelState = await category.ValidateForAdd(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                var newCategory = await _orchestrator.AddCategoryAsync(category);
#pragma warning disable CS8604 // Possible null reference argument.
                return Created(newCategory.Href, newCategory);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Updates a category in the list of categories
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("categories")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateCategory([FromBody] Category category)
        {
            try
            {
                _orchestrator.Request = Request;
                var modelState = await category.ValidateForUpdate(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                await _orchestrator.UpdateCategoryAsync(category);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes a category from the list of categories
        /// </summary>
        /// <param name="id">The id of the category to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("categories/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> DelegteCategory(string id)
        {
            try
            {
                _orchestrator.Request = Request;

                var category = await _orchestrator.GetCategoryAsync(id);

                var modelState = await category.ValidateForDelete(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                await _orchestrator.DeleteCategoryAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Href", "No category with that id exists.");
                return BadRequest(modelState);
            }
        }
    }
}
