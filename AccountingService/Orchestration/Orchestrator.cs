using AccountingService.Models.EntityModels;
using AccountingService.Models.ResourceModels;
using AccountingService.Repositories;
using AccountingService.Services;

namespace AccountingService.Orchestration
{
    /// <summary>
    /// The orchestrator for the service
    /// </summary>
    public class Orchestrator : IOrchestrator
    {
        private readonly ILogger<Orchestrator> _logger;
        private readonly IFinanceRepository _repository;
        private readonly IConfiguration     _configuration;

        /// <summary>
        /// Instantiates an orchestrator
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="repository"></param>
        /// <param name="configuration"></param>
        public Orchestrator(ILogger<Orchestrator> logger, IFinanceRepository repository, IConfiguration configuration)
        {
            _logger = logger;   
            _repository = repository;
            _configuration = configuration;
            _logger.LogTrace("Orchestrator instantiated");
        }

        /// <summary>
        /// The <see cref="HttpRequest"/> associated with this instance
        /// </summary>
        public HttpRequest? Request { get; set; }

        #region Chart of Accounts
        /// <summary>
        /// Get the list of accounts
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ResourceCollection<Account>> GetAccountsAsync(int start, int pageSize, string category)
        {
            if (Request == null)
                throw new InvalidProgramException("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");
            var entityCollection = await _repository.GetAccountsAsync(start, pageSize, category);

            var rl = AutoMapperFactory.Mapper.Map<List<EAccount>, List<Account>>(entityCollection.Items, opts => opts.AfterMap((src, dest) =>
            {
                foreach (Account account in dest)
                {
                    account.Href = account.Href != null ? new Uri(rootUrl, account.Href.AbsolutePath) : null;
                    account.Category = account.Category != null ? new Uri(rootUrl, account.Category.AbsolutePath) : null;
                }
            }));

            ResourceCollection<Account> resourceCollection = GenerateResourceCollection(start, pageSize, "chart_of_accounts", entityCollection.Count, rl);

            if (resourceCollection == null)
                throw new Exception();

            return resourceCollection;
        }

        /// <summary>
        /// Get a single account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Account> GetAccountAsync(int id)
        {
            if (Request == null)
                throw new Exception("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");

            var eAccount = await _repository.GetAccountAsync(id);

            var account = AutoMapperFactory.Mapper.Map<EAccount, Account>(eAccount, opts => opts.AfterMap((src, dest) =>
            {
                dest.Href = dest.Href != null ? new Uri(rootUrl, dest.Href.AbsolutePath) : null;
                dest.Category = dest.Category != null ? new Uri(rootUrl, dest.Category.AbsolutePath) : null;
            }));

            return account;
        }

        /// <summary>
        /// Adds an account to the Chart of Accounts
        /// </summary>
        /// <param name="account">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Account> AddAccountAsync(Account account)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Convert to entity model
            var eAccount = AutoMapperFactory.Mapper.Map<Account, EAccount>(account);

            //  Update the entity model
            eAccount = await _repository.AddAccountAsync(eAccount);

            //  Convert back to resource model and return
            return AutoMapperFactory.Mapper.Map<EAccount, Account>(eAccount);
        }

        /// <summary>
        /// Update an account in the Chart of Accounts
        /// </summary>
        /// <param name="account">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateAccountAsync(Account account)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Convert to entity model
            var eAccount = AutoMapperFactory.Mapper.Map<Account, EAccount>(account);

            //  Update the entity model
            await _repository.UpdateAccountAsync(eAccount);
        }

        /// <summary>
        /// Delete an account from the Chart of Accounts
        /// </summary>
        /// <param name="id">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DeleteAccountAsync(int id)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Update the entity model
            await _repository.DeleteAccountAsync(id);
        }

        #endregion

        #region Categories
        /// <summary>
        /// Returns the collection of categories
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<ResourceCollection<Category>> GetCategoriesAsync(int start, int pageSize)
        {
            if (Request == null)
                throw new InvalidProgramException("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");
            var entityCollection = await _repository.GetCategoriesAsync(start, pageSize);

            var rl = AutoMapperFactory.Mapper.Map<List<ECategory>, List<Category>>(entityCollection.Items, opts => opts.AfterMap((src, dest) =>
            {
                foreach (Category category in dest)
                {
                    category.Href = category.Href != null ? new Uri(rootUrl, category.Href.AbsolutePath) : null;
                }
            }));

            ResourceCollection<Category> resourceCollection = GenerateResourceCollection(start, pageSize, "categories", entityCollection.Count, rl);

            if (resourceCollection == null)
                throw new Exception();

            return resourceCollection;
        }

        /// <summary>
        /// Get a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Category> GetCategoryAsync(string id)
        {
            if (Request == null)
                throw new Exception("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");

            var eAccount = await _repository.GetCategoryAsync(id);

            var account = AutoMapperFactory.Mapper.Map<ECategory, Category>(eAccount, opts => opts.AfterMap((src, dest) =>
            {
                dest.Href = dest.Href != null ? new Uri(rootUrl, dest.Href.AbsolutePath) : null;
            }));

            return account;
        }

        /// <summary>
        /// Globally change a category id
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        public async Task ChangeCategoryIdAsync(string oldId, string newId)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  If this doesn't throw an exception, we know the old id exists
            await _repository.GetCategoryAsync(oldId);

            try
            {
                //  If this doesn't throw an exception, we know that the new id already exists
                //  We can't change the value to an already existing value
                await _repository.GetCategoryAsync(newId);
                throw new InvalidOperationException();
            }
            catch (KeyNotFoundException)
            {
                //  It's okay, the new value does not exist
            }

            await _repository.ChangeCategoryIdAsync(oldId, newId);
        }

        /// <summary>
        /// Adds an account to the Chart of Accounts
        /// </summary>
        /// <param name="category">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Category> AddCategoryAsync(Category category)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Convert to entity model
            var eCategory = AutoMapperFactory.Mapper.Map<Category, ECategory>(category);

            //  Update the entity model
            eCategory = await _repository.AddCategoryAsync(eCategory);

            //  Convert back to resource model and return
            return AutoMapperFactory.Mapper.Map<ECategory, Category>(eCategory);
        }

        /// <summary>
        /// Update a category 
        /// </summary>
        /// <param name="category">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateCategoryAsync(Category category)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Convert to entity model
            var eCategory = AutoMapperFactory.Mapper.Map<Category, ECategory>(category);

            //  Update the entity model
            await _repository.UpdateCategoryAsync(eCategory);
        }

        /// <summary>
        /// Delete a category 
        /// </summary>
        /// <param name="id">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DeleteCategoryAsync(string id)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Update the entity model
            await _repository.DeleteCategoryAsync(id);
        }

        /// <summary>
        /// Returns a category, and all its children
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="id">The id of the parent category</param>
        /// <returns></returns>
        public async Task<ResourceCollection<Category>> GetCategoryAndChildrenAsync(int start, int pageSize, string id)
        {
            if (Request == null)
                throw new InvalidProgramException("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");
            var entityCollection = await _repository.GetCategoryAndChildrenAsync(start, pageSize, id);

            var rl = AutoMapperFactory.Mapper.Map<List<ECategory>, List<Category>>(entityCollection.Items, opts => opts.AfterMap((src, dest) =>
            {
                foreach (Category category in dest)
                {
                    category.Href = category.Href != null ? new Uri(rootUrl, category.Href.AbsolutePath) : null;
                }
            }));

            ResourceCollection<Category> resourceCollection = GenerateResourceCollection(start, pageSize, "categories", entityCollection.Count, rl);

            if (resourceCollection == null)
                throw new Exception();

            return resourceCollection;
        }


        #endregion

        #region Journals
        /// <summary>
        /// Returns the collection of journals
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<ResourceCollection<Journal>> GetJournalsAsync(int start, int pageSize)
        {
            if (Request == null)
                throw new InvalidProgramException("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");
            var entityCollection = await _repository.GetJournalsAsync(start, pageSize);

            var rl = AutoMapperFactory.Mapper.Map<List<EJournal>, List<Journal>>(entityCollection.Items, opts => opts.AfterMap((src, dest) =>
            {
                foreach (Journal journal in dest)
                {
                    journal.Href = journal.Href != null ? new Uri(rootUrl, journal.Href.AbsolutePath) : null;
                }
            }));

            ResourceCollection<Journal> resourceCollection = GenerateResourceCollection(start, pageSize, "journals", entityCollection.Count, rl);

            if (resourceCollection == null)
                throw new Exception();

            return resourceCollection;
        }

        /// <summary>
        /// Get a journal by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Journal> GetJournalAsync(int id)
        {
            if (Request == null)
                throw new Exception("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");

            var eJournal = await _repository.GetJournalAsync(id);

            var account = AutoMapperFactory.Mapper.Map<EJournal, Journal>(eJournal, opts => opts.AfterMap((src, dest) =>
            {
                dest.Href = dest.Href != null ? new Uri(rootUrl, dest.Href.AbsolutePath) : null;
            }));

            return account;
        }

        /// <summary>
        /// Adds a journal to the list of journals
        /// </summary>
        /// <param name="journal">The journal to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Journal> AddJournalAsync(Journal journal)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Convert to entity model
            var eJournal = AutoMapperFactory.Mapper.Map<Journal, EJournal>(journal);

            //  Update the entity model
            eJournal = await _repository.AddJournalAsync(eJournal);

            //  Convert back to resource model and return
            return AutoMapperFactory.Mapper.Map<EJournal, Journal>(eJournal);
        }

        /// <summary>
        /// Update a journal 
        /// </summary>
        /// <param name="journal">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateJournalAsync(Journal journal)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Convert to entity model
            var eJournal = AutoMapperFactory.Mapper.Map<Journal, EJournal>(journal);

            //  Update the entity model
            await _repository.UpdateJournalAsync(eJournal);
        }

        /// <summary>
        /// Delete a journal 
        /// </summary>
        /// <param name="id">The account to add</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DeleteJournalAsync(int id)
        {
            if (Request == null)
                throw new Exception("Request is null");

            //  Update the entity model
            await _repository.DeleteJournalAsync(id);
        }
        #endregion

        #region Helper Functions
        private ResourceCollection<T> GenerateResourceCollection<T>(int start, int pageSize, string domain, int entityCount, List<T> rl)
        {
            if (Request == null)
                throw new Exception("Request is null");

            var rootUrl = new Uri($"{Request.Scheme}://{Request.Host}");

            var resourceCollection = new ResourceCollection<T>()
            {
                HRef = new Uri(rootUrl, $"{domain}?start={start}&page_size={pageSize}"),
                Count = entityCount,
                Items = rl.ToArray()
            };

            if (entityCount > rl.Count)
            {
                var next = start + pageSize;
                var previous = start - pageSize;

                if (previous < 1)
                    previous = 1;

                if (next < entityCount)
                    resourceCollection.Next = new Uri(rootUrl, $"{domain}?start={next}&page_size={pageSize}");

                resourceCollection.First = new Uri(rootUrl, $"{domain}?start=1&page_size={pageSize}");

                if (start > 1)
                {
                    resourceCollection.Previous = new Uri(rootUrl, $"{domain}?start={previous}&page_size={pageSize}");
                }

                resourceCollection.PageSize = pageSize;
            }
            else
            {
                resourceCollection.PageSize = entityCount;
            }

            return resourceCollection;
        }
        #endregion

        #region Dispose Pattern
        private bool disposedValue = false;

        /// <summary>
        /// Called to dispose this object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.LogTrace("Orchestrator disposing");
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
                _logger.LogTrace("Orchestrator disposed");
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Orchestrator()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Called to dispose this object
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
