using AccountingService.Orchestration;
using AccountingService.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AccountingService.Models.ResourceModels
{
    /// <summary>
    /// Category model
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Category Id
        /// </summary>
        public Uri? Href { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string Name { get; set; } = string.Empty ;

        /// <summary>
        /// Validates that the model is suitable for adding
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForAdd(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Href == null)
            {
                modelState.AddModelError("Href", "Missing the category id");
            }
            else
            {
                try
                {
                    var oldCategory = await orchestrator.GetCategoryAsync(Href.GetId<string>());
                    modelState.AddModelError("Href", "A category with this category id already exists.");
                }
                catch (KeyNotFoundException)
                {
                    //  This is okay, and is expected. We can't add a category that already
                    //  exists.
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
        public async Task<ModelStateDictionary> ValidateForUpdate(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Href == null)
            {
                modelState.AddModelError("Href", "Missing the category id");
            }
            else
            {
                try
                {
                    var oldCategory = await orchestrator.GetCategoryAsync(Href.GetId<string>());
                }
                catch (KeyNotFoundException)
                {
                    modelState.AddModelError("Href", "A category with this category id does not exist.");
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
                modelState.AddModelError("Href", "Missing the category id");
            }
            else
            {
#pragma warning disable CS8604 // Possible null reference argument.
                var accountCollection = await orchestrator.GetAccountsAsync(1, 10, Href.GetId<string>());

                if (accountCollection.Count > 0 )
                {
                    modelState.AddModelError("Href", "This category is used by one or more accounts.");
                }
#pragma warning restore CS8604 // Possible null reference argument.
            }

            return modelState;
        }
    }
}
