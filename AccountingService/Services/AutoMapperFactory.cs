using AutoMapper;
using System.Reflection;

namespace AccountingService.Services
{
	/// <summary>
	/// Used to perform translations from entity to resource models and vice versa.
	/// </summary>
	public static class AutoMapperFactory
	{
		/// <summary>
		/// Gets or sets the MapperConfiguration for AutoMapper
		/// </summary>
		public static MapperConfiguration MapperConfiguration { get; set; } = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

		/// <summary>
		/// The mapper
		/// </summary>
		public static IMapper Mapper { get; set; } = MapperConfiguration.CreateMapper();

		/// <summary>
		///	Creates a mapper
		/// </summary>
		public static IMapper CreateMapper()
		{
			Mapper = MapperConfiguration.CreateMapper();
			return Mapper;
		}

        /// <summary>
        /// Translates one representation of an object to another.
        /// </summary>
        /// <typeparam name="TSource">The object type we are translating from.</typeparam>
        /// <typeparam name="TDestination">The object type we are translating to.</typeparam>
        /// <param name="source">The object we are translating.</param>
        /// <param name="p"></param>
        /// <returns>The translated object</returns>
        public static TDestination? Map<TSource, TDestination>(TSource source, object p)
		{
			try
			{
				if ( Mapper != null )
					return Mapper.Map<TSource, TDestination>(source);

				throw new InvalidOperationException();
			}
			catch (Exception error)
			{
				Console.WriteLine(error.Message);
				return default;
			}
		}

		/// <summary>
		/// Translates one representation of an object to another.
		/// </summary>
		/// <param name="item">The object we are translating.</param>
		/// <param name="TSource">The object type we are translating from.</param>
		/// <param name="TDestination">The object type we are translating to.</param>
		/// <returns>The translated object</returns>
		public static object? Map(object item, Type TSource, Type TDestination)
		{
			if ( Mapper != null)	
				return Mapper.Map(item, TSource, TDestination);

			return null;
		}
	}
}
