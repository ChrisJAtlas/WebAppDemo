using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        //Automapper knows to populate MemberDto's Age int with value from AppUser's GetAge() method
        CreateMap<AppUser, MemberDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()))
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain)!.Url));

        CreateMap<Photo, PhotoDto>();

        CreateMap<MemberUpdateDto, AppUser>();

        CreateMap<RegisterDto, AppUser>();
        
        CreateMap<string, DateOnly>().ConvertUsing((s => DateOnly.Parse(s)));
    }
}
