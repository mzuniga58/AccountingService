using AccountingService.Models.EntityModels;
using AccountingService.Models.ResourceModels;
using AccountingService.Services;
using AutoMapper;

namespace AccountingService.Mappings
{
	/// <summary>
	/// Journal Profile
	/// </summary>
    public class JournalProfile : Profile
    {
		///	<summary>
		///	Initializes the Account Profile
		///	</summary>
		public JournalProfile()
		{
			//	Creates a mapping to transform a Account model instance (the source)
			//	into a EAccount model instance (the destination).
			CreateMap<Journal, EJournal>()
				.ForMember(destination => destination.JournalId, opts => opts.MapFrom(source => source.Href == null ? default : source.Href.GetId<string>()));

			//	Creates a mapping to transform a EAccount model instance (the source)
			//	into a Account model instance (the destination).
			CreateMap<EJournal, Journal>()
				.ForMember(destination => destination.Href, opts => opts.MapFrom(source => new Uri(new Uri("https://localhost"), $"journals/id/{source.JournalId}")));
		}
	}
}
