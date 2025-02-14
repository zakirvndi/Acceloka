using Acceloka.Entities;
using Acceloka.Models.DTOS;
using AutoMapper;

namespace Acceloka.Mapping
{
    public class BookedTicketProfile : Profile
    {
        public BookedTicketProfile()
        {
            CreateMap<BookedTicket, BookedTicketGetDetailDto>()
                .ForMember(dest => dest.TicketCode, opt => opt.MapFrom(src => src.TicketCode))
                .ForMember(dest => dest.TicketName, opt => opt.MapFrom(src => src.TicketCodeNavigation.TicketName)) 
                .ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.TicketCodeNavigation.EventDate)) 
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
        }


    }
}
