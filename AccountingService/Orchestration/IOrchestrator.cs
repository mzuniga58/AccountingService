using AccountingService.Models.ResourceModels;

namespace AccountingService.Orchestration
{
    /// <summary>
    /// Orchestrator interface
    /// </summary>
    public interface IOrchestrator : IDisposable
    {
        /// <summary>
        /// The HttpRequest associated with this orchestrator
        /// </summary>
        HttpRequest? Request { get; set; }

        #region Chart of Accounts
        /// <summary>
        /// Gets the list of accounts
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<ResourceCollection<Account>> GetAccountsAsync(int start, int pageSize, string category);

        /// <summary>
        /// Gets a single account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Account> GetAccountAsync(int id);

        /// <summary>
        /// Add an account to the repository
        /// </summary>
        /// <param name="account">The account to add</param>
        /// <returns></returns>
        Task<Account> AddAccountAsync(Account account);

        /// <summary>
        /// Update an account in the repository
        /// </summary>
        /// <param name="account">The account to add</param>
        /// <returns></returns>
        Task UpdateAccountAsync(Account account);

        /// <summary>
        /// Delete an account from the repository
        /// </summary>
        /// <param name="id">The account to add</param>
        /// <returns></returns>
        Task DeleteAccountAsync(int id);
        #endregion

        #region Categories
        /// <summary>
        /// Returns the collection of categories
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<ResourceCollection<Category>> GetCategoriesAsync(int start, int pageSize);

        /// <summary>
        /// Returns a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category> GetCategoryAsync(string id);

        /// <summary>
        /// Globally change a category id
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        Task ChangeCategoryIdAsync(string oldId, string newId);

        /// <summary>
        /// Add a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<Category> AddCategoryAsync(Category category);

        /// <summary>
        /// Update a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task UpdateCategoryAsync(Category category);

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteCategoryAsync(string id);

        /// <summary>
        /// Returns a category, and all its children
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="id">The id of the parent category</param>
        /// <returns></returns>
        Task<ResourceCollection<Category>> GetCategoryAndChildrenAsync(int start, int pageSize, string id);

        #endregion

        #region Journals
        /// <summary>
        /// Returns the collection of journals
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<ResourceCollection<Journal>> GetJournalsAsync(int start, int pageSize);

        /// <summary>
        /// Returns a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Journal> GetJournalAsync(int id);

        /// <summary>
        /// Add a journal
        /// </summary>
        /// <param name="journal"></param>
        /// <returns></returns>
        Task<Journal> AddJournalAsync(Journal journal);

        /// <summary>
        /// Update a Journal
        /// </summary>
        /// <param name="journal"></param>
        /// <returns></returns>
        Task UpdateJournalAsync(Journal journal);

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteJournalAsync(int id);
        #endregion
    }
}
