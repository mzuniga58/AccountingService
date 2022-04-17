using AccountingService.Orchestration;
using AccountingService.Repositories;
using AccountingService.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AccountingService.Models.ResourceModels
{
    /// <summary>
    /// Account
    /// </summary>
    public class Account
    {
        /// <summary>
        /// The hypertext reference to the account resource
        /// </summary>
        public Uri? Href { get; set; }

        /// <summary>
        /// The hypertext reference to the account category resource
        /// </summary>
        public Uri? Category { get; set; }

        /// <summary>
        /// The name of the account
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Validates whether or not this model is in a state where it can be added
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForAdd(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Category == null)
            {
                modelState.AddModelError("Category", "Category cannot be null.");
            }
            else
            {
                try
                {
                    var categoryId = Category.GetId<string>();

                    if (string.IsNullOrEmpty(categoryId))
                        modelState.AddModelError("Category", "Category cannot be blank or null.");
                    else if (categoryId.Length > 20)
                        modelState.AddModelError("Category", "Category cannot exceed 20 characters");

                    await orchestrator.GetCategoryAsync(categoryId);
                }
                catch (KeyNotFoundException)
                {
                    modelState.AddModelError("Category", "Category does not exist.");
                }
            }

            if (string.IsNullOrWhiteSpace(Name))
                modelState.AddModelError("Name", "Name cannot be null or blank.");
            else if (Name.Length > 128)
                modelState.AddModelError("Name", "Name cannot exceed 128 characters.");

            return modelState;
        }

        /// <summary>
        /// Validates whether or not this model is in a state where it can be updated
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForUpdate(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Href == null)
                modelState.AddModelError("Href", "No account id was provided.");
            else
            {
                try
                {
                    var oldAccount = await orchestrator.GetAccountAsync(Href.GetId<int>());
                }
                catch (KeyNotFoundException)
                {
                    modelState.AddModelError("Href", "No account with that account id exists.");
                }
            }

            if (Category == null)
            {
                modelState.AddModelError("Category", "Category cannot be null.");
            }
            else
            {
                try
                {
                    var categoryId = Category.GetId<string>();

                    if (string.IsNullOrEmpty(categoryId))
                        modelState.AddModelError("Category", "Category cannot be blank or null.");
                    else if (categoryId.Length > 20)
                        modelState.AddModelError("Category", "Category cannot exceed 20 characters");

                    await orchestrator.GetCategoryAsync(categoryId);
                }
                catch (KeyNotFoundException)
                {
                    modelState.AddModelError("Category", "Category does not exist.");
                }
            }

            if (string.IsNullOrWhiteSpace(Name))
                modelState.AddModelError("Name", "Name cannot be null or blank.");
            else if (Name.Length > 128)
                modelState.AddModelError("Name", "Name cannot exceed 128 characters.");

            return modelState;
        }

        /// <summary>
        /// Validates whether or not this model is in a state where it can be deleted
        /// </summary>
        /// <param name="orchestrator"></param>
        /// <returns></returns>
        public async Task<ModelStateDictionary> ValidateForDelete(IOrchestrator orchestrator)
        {
            var modelState = new ModelStateDictionary();

            if (Href == null)
                modelState.AddModelError("Href", "No account id was provided.");
            else
            {
                await Task.CompletedTask;
            }

            return modelState;
        }
    }
}
