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
    /// Chart of Accounts Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly ILogger<ChartOfAccountsController> _logger;
        private readonly IOrchestrator _orchestrator;

        /// <summary>
        /// Chart of Accounts controller
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> interface.</param>
        /// <param name="orchestrator">The <see cref="IOrchestrator"/> interface.</param>
        public ChartOfAccountsController(ILogger<ChartOfAccountsController> logger, IOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;  
        }

        /// <summary>
        /// Returns a collection of accounts
        /// </summary>
        /// <returns></returns>
        /// <param name="start">The first record returned in this page (if not provided, 1 is assumed)</param>
        /// <param name="page_size">The maximum number of records returned in this set (if not provided, 200 is assumed)</param>
        /// <param name="category">The account category (if omitted, returns all accounts)</param>
        /// <response code="200">Returns the list of accounts</response>
        /// <response code="400">Invalid start value</response>
        [HttpGet]
        [Route("chart_of_accounts")]
        [Authorize(Policy="Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(ResourceCollection<Account>))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Get([FromQuery ] int? start, [FromQuery] int? page_size, [FromQuery] string category)
        {
            var theStart = start ?? 1;
            var thePageSize = page_size ?? 200;

            if (theStart < 1)
                return BadRequest();

            _logger.LogTrace("start={start} page_size={pagesize}", theStart, thePageSize);

            _orchestrator.Request = Request;
            ResourceCollection<Account> collection = await _orchestrator.GetAccountsAsync(theStart, thePageSize, category);

            return Ok(collection);
        }

        /// <summary>
        /// Returns an account by id
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the specified account</response>
        /// <response code="404">The specified account was not found</response>
        [HttpGet]
        [Route("chart_of_accounts/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK, null, typeof(Account))]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetSingle(int id)
        {
            try
            {
                _orchestrator.Request = Request;
                Account theAccount = await _orchestrator.GetAccountAsync(id);
                return Ok(theAccount);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Adds an account to the Chart of Accounts
        /// </summary>
        /// <returns></returns>
        /// <response code="201">Returns the newly created account</response>
        /// <response code="400">One or more of the validations for the new account did not pass</response>
        [HttpPost]
        [Route("chart_of_accounts")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.Created, null, typeof(Account))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Add([FromBody] Account account)
        {
            try
            {
                _orchestrator.Request = Request;
                var modelState = await account.ValidateForAdd(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                var theAccount = await _orchestrator.AddAccountAsync(account);

#pragma warning disable CS8604 // Possible null reference argument.
                return Created(theAccount.Href, theAccount);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Updates an account in the Chart of Accounts
        /// </summary>
        /// <returns></returns>
        /// <response code="200">The account was successfully updated.</response>
        /// <response code="400">One or more of the validations for the new account did not pass</response>
        [HttpPut]
        [Route("chart_of_accounts")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Update([FromBody] Account account)
        {
            _orchestrator.Request = Request;
            var modelState = await account.ValidateForUpdate(_orchestrator);

            if (modelState.Count > 0)
                return BadRequest(modelState);

            await _orchestrator.UpdateAccountAsync(account);
            return Ok();
        }

        /// <summary>
        /// Deletes an account from the Chart of Accounts
        /// </summary>
        /// <returns></returns>
        /// <response code="200">The account was successfully deleted.</response>
        /// <response code="400">One or more of the validations for the new account did not pass</response>
        [HttpDelete]
        [Route("chart_of_accounts/id/{id}")]
        [Authorize(Policy = "Trusted")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden)]
        [Produces("application/vnd.maz.v1+json", MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Delete(int id)
        {
            _orchestrator.Request = Request;
            try
            {
                var account = await _orchestrator.GetAccountAsync(id);

                var modelState = await account.ValidateForDelete(_orchestrator);

                if (modelState.Count > 0)
                    return BadRequest(modelState);

                await _orchestrator.UpdateAccountAsync(account);
                return Ok();
            }
            catch ( KeyNotFoundException)
            {
                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Href", "No account with that account id exists.");
                return BadRequest(modelState);
            }
        }
    }
}
