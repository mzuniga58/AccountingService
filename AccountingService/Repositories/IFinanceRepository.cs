using AccountingService.Models.EntityModels;
using AccountingService.Models.ResourceModels;

namespace AccountingService.Repositories
{
    /// <summary>
    /// Finance Repository Interface
    /// </summary>
    public interface IFinanceRepository : IDisposable
    {
        #region Chart of Accounts
        /// <summary>
        /// Gets the list of accounts
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<EntityCollection<EAccount>> GetAccountsAsync(int start, int pageSize, string category);

        /// <summary>
        /// Gets a single account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<EAccount> GetAccountAsync(int id);

        /// <summary>
        /// Adds an account to the chart of accounts
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<EAccount> AddAccountAsync(EAccount account);

        /// <summary>
        /// Updates an account in the chart of accounts
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task UpdateAccountAsync(EAccount account);

        /// <summary>
        /// Delete an account from the chart of accounts
        /// </summary>
        /// <param name="id"></param>
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
        Task<EntityCollection<ECategory>> GetCategoriesAsync(int start, int pageSize);

        /// <summary>
        /// Returns a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ECategory> GetCategoryAsync(string id);

        /// <summary>
        /// Returns a category by id, along with all its children
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<EntityCollection<ECategory>> GetCategoryAndChildrenAsync(int start, int pageSize, string id);

        /// <summary>
        /// Globally change a category id
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        Task ChangeCategoryIdAsync(string oldId, string newId);

        /// <summary>
        /// Updates an existing category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task UpdateCategoryAsync(ECategory category);

        /// <summary>
        /// Add a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<ECategory> AddCategoryAsync(ECategory category);

        /// <summary>
        /// Delete a category 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteCategoryAsync(string id);
        #endregion

        #region Journals
        /// <summary>
        /// Returns the collection of categories
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<EntityCollection<EJournal>> GetJournalsAsync(int start, int pageSize);

        /// <summary>
        /// Returns a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<EJournal> GetJournalAsync(int id);

        /// <summary>
        /// Updates an existing category
        /// </summary>
        /// <param name="journal"></param>
        /// <returns></returns>
        Task UpdateJournalAsync(EJournal journal);

        /// <summary>
        /// Add a category
        /// </summary>
        /// <param name="journal"></param>
        /// <returns></returns>
        Task<EJournal> AddJournalAsync(EJournal journal);

        /// <summary>
        /// Delete a category 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteJournalAsync(int id);
        #endregion
    }
}
