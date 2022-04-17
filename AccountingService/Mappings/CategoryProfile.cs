using AccountingService.Models.EntityModels;
using AccountingService.Models.ResourceModels;
using AccountingService.Services;
using AutoMapper;

namespace AccountingService.Mappings
{
    /// <summary>
    /// Category Profile
    /// </summary>
    public class CategoryProfile : Profile
    {
		///	<summary>
		///	Initializes the Account Profile
		///	</summary>
		public CategoryProfile()
		{
			//	Creates a mapping to transform a Account model instance (the source)
			//	into a EAccount model instance (the destination).
			CreateMap<Category, ECategory>()
				.ForMember(destination => destination.CategoryId, opts => opts.MapFrom(source => source.Href == null ? default : source.Href.GetId<string>()));

			//	Creates a mapping to transform a EAccount model instance (the source)
			//	into a Account model instance (the destination).
			CreateMap<ECategory, Category>()
				.ForMember(destination => destination.Href, opts => opts.MapFrom(source => new Uri(new Uri("https://localhost"), $"categories/id/{source.CategoryId}")));
		}
	}
}
