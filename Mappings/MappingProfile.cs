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
using Drivious.DTOs.VehicleDocumnet;
using Drivious.Models;

namespace Drivious.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Driver, DriverGetDTO>();

            CreateMap<DriverCreateDTO, Driver>();

            CreateMap<DriverUpdateDTO, Driver>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Vehicle, VehicleGetDTO>();

            CreateMap<VehicleCreateDTO, Vehicle>();

            CreateMap<VehicleUpdateDTO, Vehicle>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Expense, ExpenseGetDTO>();

            CreateMap<ExpenseCreateDTO, Expense>();

            CreateMap<ExpenseUpdateDTO, Expense>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<FuelLog, FuelLogGetDTO>();

            CreateMap<FuelLogCreateDTO, FuelLog>();

            CreateMap<FuelLogUpdateDTO, FuelLog>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Income, IncomeGetDTO>();

            CreateMap<IncomeCreateDTO, Income>();

            CreateMap<IncomeUpdateDTO, Income>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Insurance, InsuranceGetDTO>();

            CreateMap<InsuranceCreateDTO, Insurance>();

            CreateMap<InsuranceUpdateDTO, Insurance>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Maintenance, MaintenanceGetDTO>();

            CreateMap<MaintenanceCreateDTO, Maintenance>();

            CreateMap<MaintenanceUpdateDTO, Maintenance>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Notification, NotificationGetDTO>();

            CreateMap<NotificationCreateDTO, Notification>();

            CreateMap<NotificationUpdateDTO, Notification>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<VehicleAssignment, VehicleAssignmentGetDTO>();

            CreateMap<VehicleAssignmentCreateDTO, VehicleAssignment>();

            CreateMap<VehicleAssignmentUpdateDTO, VehicleAssignment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<VehicleDocument, VehicleDocumentGetDTO>();

            CreateMap<VehicleDocumentCreateDTO, VehicleDocument>();

            CreateMap<VehicleDocumentUpdateDTO, VehicleDocument>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
