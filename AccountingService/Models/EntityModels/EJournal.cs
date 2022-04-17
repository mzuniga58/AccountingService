namespace AccountingService.Models.EntityModels
{
    /// <summary>
    /// Journal entity model
    /// </summary>
    public class EJournal
    {
        /// <summary>
        /// Journal Id
        /// </summary>
        public int JournalId { get; set; } = 0;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
