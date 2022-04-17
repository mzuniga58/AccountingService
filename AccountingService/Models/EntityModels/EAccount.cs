namespace AccountingService.Models.EntityModels
{
    /// <summary>
    /// EAccount
    /// </summary>
    public class EAccount
    {
        /// <summary>
        /// Account Id
        /// </summary>
        public int AccountId { get; set; }  

        /// <summary>
        /// Category Id
        /// </summary>
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
