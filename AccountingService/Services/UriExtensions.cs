namespace AccountingService.Services
{
	/// <summary>
	/// UriExtensions class
	/// </summary>
    public static class UriExtensions
    {
		/// <summary>
		/// Returns the ID of the url
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static T GetId<T>(this Uri uri)
		{
			string urlString = uri.ToString();
			string idValue = urlString[(urlString.LastIndexOf("/") + 1)..];

			if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var underlyingType = Nullable.GetUnderlyingType(typeof(T));

				if (underlyingType == null)
					throw new InvalidCastException();

				return (T)Convert.ChangeType(idValue, underlyingType);
			}
			else
				return (T)Convert.ChangeType(idValue, typeof(T));
		}

		/// <summary>
		/// Returns the Id of a specific key within the Url
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="uri">The <see cref="Uri"/> containing the key values</param>
		/// <param name="position">The position of the key.</param>
		/// <returns>The value of the key</returns>
		/// <remarks>The position is counted from the end. Therefore, using https://domain.com/x/y/id/1/2/3, position 0 would return 3, while position 1 would return 2, etc.</remarks>
		public static T GetId<T>(this Uri uri, int position)
		{
			var segments = uri.Segments;
			var index = segments.Length - position - 1;

			var text = segments[index];

			if (text.EndsWith("\\") || text.EndsWith("/"))
				text = text[0..^1];

			if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var underlyingType = Nullable.GetUnderlyingType(typeof(T));

				if (underlyingType == null)
					throw new InvalidCastException();

                return (T)Convert.ChangeType(text, underlyingType);
			}
			else
				return (T)Convert.ChangeType(text, typeof(T));
		}
	}
}
