using AccountingService.Orchestration;
using AccountingService.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AccountingService.Models.ResourceModels
{
    /// <summary>
    /// Journal
    /// </summary>
    public class Journal
    {
        /// <summary>
        /// Hyptertext reference to a Journal resource
        /// </summary>
        public Uri? Href { get; set; }

        /// <summary>
        /// The name of the journal
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Validates that the model is suitable for adding
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForAdd(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (string.IsNullOrWhiteSpace(Name))
                modelState.AddModelError("Name", "The name cannot be null or blank.");
            else if (Name.Length > 128)
                modelState.AddModelError("Name", "The name cannont exceed 128 characters.");

            await Task.CompletedTask;

            return modelState;
        }

        /// <summary>
        /// Validates that the model is suitable for adding
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForUpdate(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Href == null)
            {
                modelState.AddModelError("Href", "Missing the journal id");
            }
            else
            {
                try
                {
                    await orchestrator.GetJournalAsync(Href.GetId<int>());
                }
                catch (KeyNotFoundException)
                {
                    modelState.AddModelError("Href", "A journal with this journal id does not exist.");
                }
            }

            if (string.IsNullOrWhiteSpace(Name))
                modelState.AddModelError("Name", "The name cannot be null or blank.");
            else if (Name.Length > 128)
                modelState.AddModelError("Name", "The name cannont exceed 128 characters.");

            return modelState;
        }

        /// <summary>
        /// Validates that the model is suitable for adding
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForDelete(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Href == null)
            {
                modelState.AddModelError("Href", "Missing the journal id");
            }
            else
            {
                await Task.CompletedTask;
            }

            return modelState;
        }

    }
}
