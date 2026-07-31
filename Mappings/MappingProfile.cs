using AutoMapper;
using Drivious.DTOs.Driver;
using Drivious.DTOs.Expense;
using Drivious.DTOs.FuelLog;
using Drivious.DTOs.Income;
using Drivious.DTOs.Insurance;
using Drivious.DTOs.Maintenance;
using Drivious.DTOs.Notification;
using Drivious.DTOs.Vehicle;
using Drivious.DTOs.VehicleAssignment;
using Drivious.DTOs.VehicleDocument;
using Drivious.Models;

namespace Drivious.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Image / ImageUrl are filled by the services after the upload is written to disk.
            // Without these Ignore() calls AutoMapper maps the IFormFile over them via ToString().
            CreateMap<Driver, DriverGetDTO>()
                .ForMember(x => x.FullName,
                    opts => opts.MapFrom(src => src.FirstName + " " + src.LastName));

            CreateMap<DriverCreateDTO, Driver>()
                .ForMember(x => x.Image, opts => opts.Ignore())
                .ForMember(x => x.ImageUrl, opts => opts.Ignore());

            CreateMap<DriverUpdateDTO, Driver>()
                .ForMember(x => x.Image, opts => opts.Ignore())
                .ForMember(x => x.ImageUrl, opts => opts.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Vehicle, VehicleGetDTO>();

            CreateMap<VehicleCreateDTO, Vehicle>()
                .ForMember(x => x.Image, opts => opts.Ignore())
                .ForMember(x => x.ImageURL, opts => opts.Ignore());

            CreateMap<VehicleUpdateDTO, Vehicle>()
                .ForMember(x => x.Image, opts => opts.Ignore())
                .ForMember(x => x.ImageURL, opts => opts.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // The related names below are resolved by ProjectTo as a join, so the list
            // endpoints return display-ready rows without a second round trip.
            CreateMap<Expense, ExpenseGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model));

            CreateMap<ExpenseCreateDTO, Expense>();

            CreateMap<ExpenseUpdateDTO, Expense>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<FuelLog, FuelLogGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model));

            CreateMap<FuelLogCreateDTO, FuelLog>();

            CreateMap<FuelLogUpdateDTO, FuelLog>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Income, IncomeGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model))
                .ForMember(x => x.DriverFullName,
                    opts => opts.MapFrom(src => src.Driver.FirstName + " " + src.Driver.LastName));

            CreateMap<IncomeCreateDTO, Income>();

            CreateMap<IncomeUpdateDTO, Income>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Insurance, InsuranceGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model));

            CreateMap<InsuranceCreateDTO, Insurance>();

            CreateMap<InsuranceUpdateDTO, Insurance>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Maintenance, MaintenanceGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model));

            CreateMap<MaintenanceCreateDTO, Maintenance>();

            CreateMap<MaintenanceUpdateDTO, Maintenance>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Notification, NotificationGetDTO>();

            CreateMap<NotificationCreateDTO, Notification>();

            CreateMap<NotificationUpdateDTO, Notification>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<VehicleAssignment, VehicleAssignmentGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model))
                .ForMember(x => x.DriverFullName,
                    opts => opts.MapFrom(src => src.Driver.FirstName + " " + src.Driver.LastName));

            CreateMap<VehicleAssignmentCreateDTO, VehicleAssignment>();

            CreateMap<VehicleAssignmentUpdateDTO, VehicleAssignment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<VehicleDocument, VehicleDocumentGetDTO>()
                .ForMember(x => x.VehiclePlateNumber,
                    opts => opts.MapFrom(src => src.Vehicle.PlateNumber))
                .ForMember(x => x.VehicleName,
                    opts => opts.MapFrom(src => src.Vehicle.Brand + " " + src.Vehicle.Model));

            CreateMap<VehicleDocumentCreateDTO, VehicleDocument>()
                .ForMember(x => x.FileName, opts => opts.Ignore())
                .ForMember(x => x.FileUrl, opts => opts.Ignore())
                .ForMember(x => x.UploadDate, opts => opts.Ignore());

            CreateMap<VehicleDocumentUpdateDTO, VehicleDocument>()
                .ForMember(x => x.FileName, opts => opts.Ignore())
                .ForMember(x => x.FileUrl, opts => opts.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
