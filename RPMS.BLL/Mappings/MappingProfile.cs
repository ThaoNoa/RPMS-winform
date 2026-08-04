using AutoMapper;
using RPMS.DAL.Entities;
using RPMS.DTO.Amenity;
using RPMS.DTO.Contract;
using RPMS.DTO.House;
using RPMS.DTO.Invoice;
using RPMS.DTO.Maintenance;
using RPMS.DTO.Post;
using RPMS.DTO.Role;
using RPMS.DTO.Room;
using RPMS.DTO.User;
using System;
using System.Linq;

namespace RPMS.BLL.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Role, RoleDto>();

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
            CreateMap<CreateUserDto, User>();

            CreateMap<House, HouseDto>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.FullName))
                .ForMember(dest => dest.TotalRooms, opt => opt.MapFrom(src => src.Rooms.Count));
            CreateMap<CreateHouseDto, House>();

            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.HouseName, opt => opt.MapFrom(src => src.House.HouseName));

            CreateMap<Room, RoomDetailDto>()
                .IncludeBase<Room, RoomDto>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.RoomImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath)))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.RoomAmenities.Select(ra => new AmenityDto { AmenityID = ra.AmenityID, AmenityName = ra.Amenity.AmenityName })));
            CreateMap<CreateRoomDto, Room>();

            CreateMap<Amenity, AmenityDto>();

            CreateMap<Post, PostDto>()
                .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src =>
                    src.PostImages.Where(pi => pi.IsMain).Select(pi => pi.ImagePath).FirstOrDefault()
                    ?? src.PostImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath).FirstOrDefault()
                    ?? (src.Room != null && src.Room.RoomImages != null
                        ? src.Room.RoomImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath).FirstOrDefault()
                        : null)
                    ?? ""))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : ""))
                .ForMember(dest => dest.HouseAddress, opt => opt.MapFrom(src =>
                    src.Room != null && src.Room.House != null ? src.Room.House.Address : ""));
            CreateMap<Post, PostDetailDto>()
                .IncludeBase<Post, PostDto>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Room != null ? src.Room.Area : 0))
                .ForMember(dest => dest.Furniture, opt => opt.MapFrom(src => src.Room != null ? src.Room.Furniture ?? "" : ""))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    (src.PostImages != null && src.PostImages.Any()
                        ? src.PostImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath)
                        : (src.Room != null && src.Room.RoomImages != null
                            ? src.Room.RoomImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath)
                            : Enumerable.Empty<string>()))
                    .ToList()))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src =>
                    src.Room != null && src.Room.RoomAmenities != null
                        ? src.Room.RoomAmenities.Select(ra => ra.Amenity != null ? ra.Amenity.AmenityName : "").Where(n => n != "").ToList()
                        : new System.Collections.Generic.List<string>()));

            CreateMap<Contract, ContractDto>()
                .ForMember(dest => dest.HouseID, opt => opt.MapFrom(src =>
                    src.Room != null ? src.Room.HouseID : 0))
                .ForMember(dest => dest.HouseName, opt => opt.MapFrom(src =>
                    src.Room != null && src.Room.House != null ? src.Room.House.HouseName : ""))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : ""))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src =>
                    src.Tenant != null ? src.Tenant.FullName : "(Chưa có khách)"));
            CreateMap<Contract, ContractDetailDto>()
                .IncludeBase<Contract, ContractDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? src.CreatedByUser.FullName : ""));

            CreateMap<MaintenanceRequest, MaintenanceRequestDto>()
                .ForMember(dest => dest.ContractID, opt => opt.MapFrom(src => src.ContractID))
                .ForMember(dest => dest.ContractCode, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.ContractCode : ""))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src =>
                    src.Contract != null && src.Contract.Room != null ? src.Contract.Room.RoomNumber : ""))
                .ForMember(dest => dest.HouseName, opt => opt.MapFrom(src =>
                    src.Contract != null && src.Contract.Room != null && src.Contract.Room.House != null
                        ? src.Contract.Room.House.HouseName : ""))
                .ForMember(dest => dest.HouseAddress, opt => opt.MapFrom(src =>
                    src.Contract != null && src.Contract.Room != null && src.Contract.Room.House != null
                        ? src.Contract.Room.House.Address : ""))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src =>
                    src.Contract != null && src.Contract.Tenant != null ? src.Contract.Tenant.FullName : ""))
                .ForMember(dest => dest.TenantPhone, opt => opt.MapFrom(src =>
                    src.Contract != null && src.Contract.Tenant != null ? src.Contract.Tenant.Phone ?? "" : ""))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.AssignedManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : ""));

            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.ContractCode, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.ContractCode : ""))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Room != null ? src.Contract.Room.RoomNumber : ""));
            CreateMap<Invoice, InvoiceDetailDto>()
                .IncludeBase<Invoice, InvoiceDto>()
                .ForMember(dest => dest.OldElectric, opt => opt.MapFrom(src => src.MeterReading != null ? src.MeterReading.OldElectric : 0))
                .ForMember(dest => dest.NewElectric, opt => opt.MapFrom(src => src.MeterReading != null ? src.MeterReading.NewElectric : 0))
                .ForMember(dest => dest.OldWater, opt => opt.MapFrom(src => src.MeterReading != null ? src.MeterReading.OldWater : 0))
                .ForMember(dest => dest.NewWater, opt => opt.MapFrom(src => src.MeterReading != null ? src.MeterReading.NewWater : 0))
                .ForMember(dest => dest.ReadingMonth, opt => opt.MapFrom(src => src.MeterReading != null ? (DateTime?)src.MeterReading.ReadingMonth : null))
                .ForMember(dest => dest.PaidDate, opt => opt.MapFrom(src => src.PaidDate))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Tenant != null ? src.Contract.Tenant.FullName : ""))
                .ForMember(dest => dest.TenantPhone, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Tenant != null ? src.Contract.Tenant.Phone ?? "" : ""))
                .ForMember(dest => dest.TenantEmail, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Tenant != null ? src.Contract.Tenant.Email ?? "" : ""))
                .ForMember(dest => dest.HouseName, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Room != null && src.Contract.Room.House != null ? src.Contract.Room.House.HouseName : ""))
                .ForMember(dest => dest.HouseAddress, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Room != null && src.Contract.Room.House != null ? src.Contract.Room.House.Address : ""))
                .ForMember(dest => dest.RoomArea, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Room != null ? (decimal?)src.Contract.Room.Area : null))
                .ForMember(dest => dest.RoomPrice, opt => opt.MapFrom(src => src.Contract != null && src.Contract.Room != null ? (decimal?)src.Contract.Room.Price : null))
                .ForMember(dest => dest.ContractStartDate, opt => opt.MapFrom(src => src.Contract != null ? (DateTime?)src.Contract.StartDate : null))
                .ForMember(dest => dest.ContractEndDate, opt => opt.MapFrom(src => src.Contract != null ? (DateTime?)src.Contract.EndDate : null))
                .ForMember(dest => dest.MoveInDate, opt => opt.MapFrom(src => src.Contract != null
                    ? (DateTime?)(src.Contract.MoveInDate == default ? src.Contract.StartDate : src.Contract.MoveInDate)
                    : null))
                .ForMember(dest => dest.MoveOutDate, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.MoveOutDate : null))
                .ForMember(dest => dest.ElectricPrice, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.ElectricPrice : 0))
                .ForMember(dest => dest.WaterPrice, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.WaterPrice : 0))
                .ForMember(dest => dest.ContractStatus, opt => opt.MapFrom(src => src.Contract != null ? src.Contract.Status : ""));
        }
    }
}