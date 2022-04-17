using AccountingService.Models.EntityModels;
using AccountingService.Models.ResourceModels;
using Npgsql;

namespace AccountingService.Repositories
{
    /// <summary>
    /// Finance repository
    /// </summary>
    public class FinanceRepository : IFinanceRepository
    {
        private readonly ILogger<FinanceRepository> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Instantiates a Finance Repository
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public FinanceRepository(ILogger<FinanceRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _logger.LogTrace("FinanceRepository instantiated");
        }

        #region Chart of Accounts
        /// <summary>
        /// Get the list of accounts
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<EntityCollection<EAccount>> GetAccountsAsync(int start, int pageSize, string category)
        {
            var collection = new EntityCollection<EAccount>();
            var accounts = new List<EAccount>();
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            if (string.IsNullOrWhiteSpace(category))
            {
                var query = @"select count(*) from fin.""COA""";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        collection.Count = reader.GetInt32(0);
                    }
                }

                query = @"
select coa.""AccountId"", coa.""CategoryId"", coa.""Name""
   from fin.""COA"" as coa
   limit @pagesize
   offset @start
";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@start", start - 1);

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var account = new EAccount
                        {
                            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                            CategoryId = reader.GetString(reader.GetOrdinal("CategoryId")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        accounts.Add(account);
                    }
                }

                collection.Items = accounts;
            }
            else
            {
                var query = @"select count(*) from fin.""COA"" as coa where coa.""CategoryId"" like(@category)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@category", $"{category}%");

                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        collection.Count = reader.GetInt32(0);
                    }
                }

                query = @"
select coa.""AccountId"", coa.""CategoryId"", coa.""Name""
   from fin.""COA"" as coa
  where coa.""CategoryId"" like (@category)
   limit @pagesize
   offset @start
";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@start", start - 1);
                    command.Parameters.AddWithValue("@category", $"{category}%");

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var account = new EAccount
                        {
                            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                            CategoryId = reader.GetString(reader.GetOrdinal("CategoryId")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        accounts.Add(account);
                    }
                }

                collection.Items = accounts;
            }

            return collection;
        }

        /// <summary>
        /// Get a single account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<EAccount> GetAccountAsync(int accountId)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"select coa.""AccountId"", coa.""CategoryId"", coa.""Name"" from fin.""COA"" as coa where ""AccountId"" = @AccountId";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AccountId", accountId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var account = new EAccount
                    {
                        AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                        CategoryId = reader.GetString(reader.GetOrdinal("CategoryId")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    };

                    return account;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Adds an account to the chart of accounts
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<EAccount> AddAccountAsync(EAccount account)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"insert into fin.""COA"" ( ""CategoryId"", ""Name"") values (@category, @name) returning ""AccountId""";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@category", account.CategoryId);
            command.Parameters.AddWithValue("@name", account.Name);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                account.AccountId = reader.GetInt32(0);
                return account;
            }

            throw new Exception("Could not read new id.");
        }

        /// <summary>
        /// Updates an account in the chart of accounts
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task UpdateAccountAsync(EAccount account)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
update fin.""COA"" 
   set ""CategoryId"" = @category,
       ""Name"" = @name
 where ""AccountId"" = @accountId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@category", account.CategoryId);
            command.Parameters.AddWithValue("@name", account.Name);
            command.Parameters.AddWithValue("@accountId", account.AccountId);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Delete an account from the chart of accounts
        /// </summary>
        /// <param name="id">The account id of the account to delete</param>
        /// <returns></returns>
        public async Task DeleteAccountAsync(int id)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
delete fin.""COA"" 
 where ""AccountId"" = @accountId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@accountId", id);

            await command.ExecuteNonQueryAsync();
        }
        #endregion

        #region Categories
        /// <summary>
        /// Returns the collection of categories
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<EntityCollection<ECategory>> GetCategoriesAsync(int start, int pageSize)
        {
            var collection = new EntityCollection<ECategory>();
            var categories = new List<ECategory>();
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"select count(*) from fin.""Category""";

            using (var command = new NpgsqlCommand(query, connection))
            {
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    collection.Count = reader.GetInt32(0);
                }
            }

            query = @"
select c.""CategoryId"", c.""Name""
   from fin.""Category"" as c
   limit @pagesize
   offset @start
";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pageSize", pageSize);
                command.Parameters.AddWithValue("@start", start - 1);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var category = new ECategory
                    {
                        CategoryId = reader.GetString(reader.GetOrdinal("CategoryId")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    };

                    categories.Add(category);
                }
            }

            collection.Items = categories;

            return collection;
        }

        /// <summary>
        /// Get a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<ECategory> GetCategoryAsync(string id)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"select c.""CategoryId"", c.""Name"" from fin.""Category"" as c where ""CategoryId"" = @id";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var category = new ECategory
                    {
                        CategoryId = reader.GetString(reader.GetOrdinal("CategoryId")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    };

                    return category;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Globally change a category id
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        public async Task ChangeCategoryIdAsync(string oldId, string newId)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var newCategory = await GetCategoryAsync(oldId);
            newCategory.CategoryId = newId;

            await AddCategoryAsync(newCategory);

            var query = @"update fin.""COA"" set fin.""CategoryId"" = @newId where fin.""CategoryId"" = @oldId";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@newId", newId);
                command.Parameters.AddWithValue("@oldId", oldId);

                await command.ExecuteNonQueryAsync();
            }

            await DeleteCategoryAsync(oldId);
        }

        /// <summary>
        /// Update a category
        /// </summary>
        /// <param name="category">The category to update</param>
        /// <returns></returns>
        public async Task UpdateCategoryAsync(ECategory category)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
update fin.""Category"" 
   set ""Name"" = @name
 where ""AccountId"" = @categoryId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@categoryId", category.CategoryId);
            command.Parameters.AddWithValue("@name", category.Name);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Add a category
        /// </summary>
        /// <param name="category">The category to update</param>
        /// <returns></returns>
        public async Task<ECategory> AddCategoryAsync(ECategory category)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"insert into fin.""Category"" ( ""CategoryId"", ""Name"" ) values ( @categoryId, @name)";


            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@categoryId", category.CategoryId);
            command.Parameters.AddWithValue("@name", category.Name);

            await command.ExecuteNonQueryAsync();

            return category;
        }

        /// <summary>
        /// Update a category
        /// </summary>
        /// <param name="id">The category to delete</param>
        /// <returns></returns>
        public async Task DeleteCategoryAsync(string id)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
delete from fin.""Category"" 
 where ""CategoryId"" = @categoryId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@categoryId", id);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Get a category by id, and all its children
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<EntityCollection<ECategory>> GetCategoryAndChildrenAsync(int start, int pageSize, string id)
        {
            var collection = new EntityCollection<ECategory>();
            var categories = new List<ECategory>();
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"select count(*) from fin.""Category"" where ""CategoryId like (@key)";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@key", $"{id}%");
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    collection.Count = reader.GetInt32(0);
                }
            }

            query = @"
select c.""CategoryId"", c.""Name""
  from fin.""Category"" as c
 where ""CategoryId like (@key)   
 limit @pagesize
offset @start
";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pageSize", pageSize);
                command.Parameters.AddWithValue("@start", start - 1);
                command.Parameters.AddWithValue("@key", $"{id}%");

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var category = new ECategory
                    {
                        CategoryId = reader.GetString(reader.GetOrdinal("CategoryId")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    };

                    categories.Add(category);
                }
            }

            collection.Items = categories;

            return collection;
        }


        #endregion

        #region Journals
        /// <summary>
        /// Returns the collection of journals
        /// </summary>
        /// <param name="start"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<EntityCollection<EJournal>> GetJournalsAsync(int start, int pageSize)
        {
            var collection = new EntityCollection<EJournal>();
            var journals = new List<EJournal>();
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"select count(*) from fin.""Journals""";

            using (var command = new NpgsqlCommand(query, connection))
            {
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    collection.Count = reader.GetInt32(0);
                }
            }

            query = @"
select c.""JournalId"", c.""Name""
   from fin.""Journals"" as c
   limit @pagesize
   offset @start
";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pageSize", pageSize);
                command.Parameters.AddWithValue("@start", start - 1);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var journal = new EJournal
                    {
                        JournalId = reader.GetInt32(reader.GetOrdinal("JournalId")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    };

                    journals.Add(journal);
                }
            }

            collection.Items = journals;

            return collection;
        }

        /// <summary>
        /// Get a journal by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<EJournal> GetJournalAsync(int id)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"select c.""JournalId"", c.""Name"" from fin.""Journals"" as c where ""JournalId"" = @id";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var journal = new EJournal
                    {
                        JournalId = reader.GetInt32(reader.GetOrdinal("JournalId")),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    };

                    return journal;
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Update a journal
        /// </summary>
        /// <param name="journal">The category to update</param>
        /// <returns></returns>
        public async Task UpdateJournalAsync(EJournal journal)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
update fin.""Journals"" 
   set ""Name"" = @name
 where ""JournalId"" = @journalId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@categoryId", journal.JournalId);
            command.Parameters.AddWithValue("@name", journal.Name);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Add a journal
        /// </summary>
        /// <param name="journal">The category to update</param>
        /// <returns></returns>
        public async Task<EJournal> AddJournalAsync(EJournal journal)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"insert into fin.""Journal"" ( ""Name"" ) values ( @name) returning JournalId";


            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", journal.Name);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                journal.JournalId = reader.GetInt32(0);
            }

            return journal;
        }

        /// <summary>
        /// Delete a journal
        /// </summary>
        /// <param name="id">The category to delete</param>
        /// <returns></returns>
        public async Task DeleteJournalAsync(int id)
        {
            var connectionStringsSection = _configuration.GetSection("ConnectionStrings");
            var connectionString = connectionStringsSection.GetValue<string>("DefaultConnection");
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
delete from fin.""Journals"" 
 where ""JournalId"" = @journalId";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@journalId", id);

            await command.ExecuteNonQueryAsync();
        }


        #endregion

        #region Dispose Pattern
        private bool disposedValue;

        /// <summary>
        /// Called to dispose this instance
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.LogTrace("FinanceRepository disposing...");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
                _logger.LogTrace("FinanceRepository disposed");
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FinanceRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Called to dispose this instance
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
