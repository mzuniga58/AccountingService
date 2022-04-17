namespace AccountingService.Models.ResourceModels
{
    /// <summary>
    /// Resource collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResourceCollection<T>
    {
        /// <summary>
        /// Hypertext reference to the collection
        /// </summary>
        public Uri HRef { get; set; } = new Uri("http://localhost");

        /// <summary>
        /// Hypertext reference to the next page of a paged collection
        /// </summary>
        public Uri? Next {get; set; }

        /// <summary>
        /// Hypertext reference to the previous page of a paged collection
        /// </summary>
        public Uri? Previous { get; set; }

        /// <summary>
        /// Hypertext reference to the first page of a paged collection
        /// </summary>
        public Uri? First { get; set; }

        /// <summary>
        /// The total number of items in the collection
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// The page size (number of items in the page)
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// The items in this page
        /// </summary>
        public T[]? Items { get; set; }
    }
}
