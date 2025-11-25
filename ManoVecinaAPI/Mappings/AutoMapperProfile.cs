using AutoMapper;
using ManoVecinaAPI.DTOs.Gigs;
using ManoVecinaAPI.DTOs.Orders;
using ManoVecinaAPI.DTOs.Reviews;
using ManoVecinaAPI.DTOs.Users;
using ManoVecinaAPI.Models;

namespace ManoVecinaAPI.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ============================================================
        // USER
        // ============================================================
        CreateMap<User, UserProfileDto>();


        // ============================================================
        // GIG
        // ============================================================
        CreateMap<Gig, GigResponseDto>()
            .ForMember(dest => dest.WorkerName,
                opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : ""))

            .ForMember(dest => dest.ImageUrls,
                opt => opt.MapFrom(src =>
                    (src.ImageUrls ?? new List<string>())
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .Select(url => url.Replace("\\", "/").Trim())
                        .ToList()
                ));


        // ============================================================
        // ORDER
        // ============================================================
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.ClientName,
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : ""))

            .ForMember(dest => dest.WorkerName,
                opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : ""))

            .ForMember(dest => dest.GigTitle,
                opt => opt.MapFrom(src => src.Gig != null ? src.Gig.Title : ""))

            .ForMember(dest => dest.GigCategory,
                opt => opt.MapFrom(src => src.Gig != null ? src.Gig.Category : ""))

            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status));


        // ============================================================
        // REVIEWS
        // ============================================================
        CreateMap<Review, ReviewResponseDto>();
    }
}
