using AutoMapper;
using ProniaFrontToBack.Areas.Manage.ViewModels.Product;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.MapProfile;

public class MapProfiles : Profile
{
    public MapProfiles()
    {
        CreateMap<CreateProductVm, Product>();
    }
}