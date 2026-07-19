using AutoMapper;
using Drivious.DTOs.Driver;
using Drivious.Models;

namespace Drivious.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Driver, DriverGetDTO>();

            CreateMap<DriverCreateDTO, Driver>();

            CreateMap<DriverUpdateDTO, Driver>();
        }
    }
}
