
using AutoMapper;
using BallastLaneApi.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace API.Extensions
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            // Pothole mappings
            CreateMap<Pothole, PotholeDto>();
            CreateMap<PotholeDto, Pothole>();

            // Create DTO -> Pothole (for incoming create requests)
            CreateMap<PotholeToCreateDto, Pothole>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
