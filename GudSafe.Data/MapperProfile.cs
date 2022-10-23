using AutoMapper;
using GudSafe.Data.Entities;
using GudSafe.Data.Models;
using GudSafe.Data.Models.EntityModels;

namespace GudSafe.Data;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<User, UserModel>();
        CreateMap<GudFile, GudFileModel>();
        CreateMap<GudCollection, GudCollectionModel>();
    }
}