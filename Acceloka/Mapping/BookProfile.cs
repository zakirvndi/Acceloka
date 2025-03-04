using AutoMapper;
using Acceloka.Entities;
using Acceloka.Models.DTOS;
using Acceloka.Models;

namespace Acceloka.Mapping
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<BookTicketDto, BookedTicket>()
                .ForMember(dest => dest.TicketCode, opt => opt.MapFrom(src => src.TicketCode))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

            CreateMap<Book, BookModel>()
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.TotalQuantity));

            CreateMap<BookedTicket, BookedTicketDetailDto>()
                .ForMember(dest => dest.TicketCode, opt => opt.MapFrom(src => src.TicketCode))
                .ForMember(dest => dest.TicketName, opt => opt.Ignore()) 
                .ForMember(dest => dest.Price, opt => opt.Ignore()); 

            CreateMap<Book, BookedTicketResponseDto>()
                .ForMember(dest => dest.PriceSummary, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.TicketsPerCategories, opt => opt.Ignore()); 

        }
    }
}
