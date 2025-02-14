using AutoMapper;
using Acceloka.Models;
using Acceloka.Entities;
using Acceloka.Models.DTOS;

public class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<Ticket, TicketDto>(); 
    }
}