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
    public class JournalsController : ControllerBase
    {
        private readonly ILogger<ChartOfAccountsController> _logger;
        private readonly IOrchestrator _orchestrator;

        /// <summary>
        /// Instantiates a categories controller
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> interface.</param>
        /// <param name="orchestrator">The <see cref="IOrchestrator"/> interface.</param>
        public JournalsController(ILogger<ChartOfAccountsController> logger, IOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;
        }

        /// <summary>
        /// Get the list of journals
        /// </summary>
        /// <returns></returns>
        /// <param name="start">The first record returned in this page (if not provided, 1 is assumed)</param>
        /// <param name="page_size">The maximum number of records returned in this set (if not provided, 200 is assumed)</param>
        /// <response code="200">Returns the list of journals</response>
        /// <response code="400">Invalid start value</response>
        [HttpGet]
        [Route("journals")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(ResourceCollection<Journal>))]
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
            ResourceCollection<Journal> collection = await _orchestrator.GetJournalsAsync(theStart, thePageSize);

            return Ok(collection);
        }

        /// <summary>
        /// Get a journal by id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("journals/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(Journal))]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
                _orchestrator.Request = Request;
                Journal theCategory = await _orchestrator.GetJournalAsync(id);
                return Ok(theCategory);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Adds a journal to the list of journals
        /// </summary>
        /// <param name="journal">The journal to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("journals")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AddCategory([FromBody] Journal journal)
        {
            try
            {
                _orchestrator.Request = Request;
                var modelState = await journal.ValidateForAdd(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                var newCategory = await _orchestrator.AddJournalAsync(journal);
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
        /// Updates a journal in the list of journals
        /// </summary>
        /// <param name="journal">The journal to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("journals")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdateJournal([FromBody] Journal journal)
        {
            try
            {
                _orchestrator.Request = Request;
                var modelState = await journal.ValidateForUpdate(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                await _orchestrator.UpdateJournalAsync(journal);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes a journal from the list of journals
        /// </summary>
        /// <param name="id">The id of the journal to delete</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("journals/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> DelegteJournal(int id)
        {
            try
            {
                _orchestrator.Request = Request;

                var journal = await _orchestrator.GetJournalAsync(id);

                var modelState = await journal.ValidateForDelete(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                await _orchestrator.DeleteJournalAsync(id);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Href", "No journal with that id exists.");
                return BadRequest(modelState);
            }
        }
    }
}

