using AccountingService.Models.EntityModels;
using AccountingService.Models.ResourceModels;
using AccountingService.Services;
using AutoMapper;

namespace AccountingService.Mappings
{
	/// <summary>
	/// Account Profile
	/// </summary>
	public class AccountProfile : Profile
	{
		///	<summary>
		///	Initializes the Account Profile
		///	</summary>
		public AccountProfile()
		{
			//	Creates a mapping to transform a Account model instance (the source)
			//	into a EAccount model instance (the destination).
			CreateMap<Account, EAccount>()
				.ForMember(destination => destination.AccountId, opts => opts.MapFrom(source => source.Href == null ? default : source.Href.GetId<int>()))
				.ForMember(destination => destination.CategoryId, opts => opts.MapFrom(source => source.Category == null ? default : source.Category.GetId<string>()));

			//	Creates a mapping to transform a EAccount model instance (the source)
			//	into a Account model instance (the destination).
			CreateMap<EAccount, Account>()
				.ForMember(destination => destination.Href, opts => opts.MapFrom(source => new Uri(new Uri("https://localhost"), $"chart_of_accounts/id/{source.AccountId}")))
				.ForMember(destination => destination.Category, opts => opts.MapFrom(source => new Uri(new Uri("https://localhost"), $"categories/id/{source.CategoryId}")));
		}
	}
}
