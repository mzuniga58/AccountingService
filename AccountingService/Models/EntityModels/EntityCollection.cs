namespace AccountingService.Models.EntityModels
{
    /// <summary>
    /// Entity Collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityCollection<T>
    {
        /// <summary>
        /// Count
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// Items
        /// </summary>
        public List<T> Items { get; set; } = new List<T>(); 
    }
}
