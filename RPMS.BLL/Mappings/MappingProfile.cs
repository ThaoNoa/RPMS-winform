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
                .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src => src.PostImages.FirstOrDefault(pi => pi.IsMain) != null ? src.PostImages.FirstOrDefault(pi => pi.IsMain).ImagePath : ""))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
                .ForMember(dest => dest.HouseAddress, opt => opt.MapFrom(src => src.Room.House.Address));
            CreateMap<Post, PostDetailDto>()
                .IncludeBase<Post, PostDto>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Room.Area))
                .ForMember(dest => dest.Furniture, opt => opt.MapFrom(src => src.Room.Furniture))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PostImages.OrderBy(x => x.DisplayOrder).Select(x => x.ImagePath)))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.Room.RoomAmenities.Select(ra => ra.Amenity.AmenityName)));

            CreateMap<Contract, ContractDto>()
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room.RoomNumber))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.FullName));
            CreateMap<Contract, ContractDetailDto>()
                .IncludeBase<Contract, ContractDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser.FullName));

            CreateMap<MaintenanceRequest, MaintenanceRequestDto>()
                .ForMember(dest => dest.ContractCode, opt => opt.MapFrom(src => src.Contract.ContractCode))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Contract.Room.RoomNumber))
                .ForMember(dest => dest.AssignedManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.FullName : ""));

            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.ContractCode, opt => opt.MapFrom(src => src.Contract.ContractCode))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Contract.Room.RoomNumber));
            CreateMap<Invoice, InvoiceDetailDto>()
                .IncludeBase<Invoice, InvoiceDto>()
                .ForMember(dest => dest.OldElectric, opt => opt.MapFrom(src => src.MeterReading.OldElectric))
                .ForMember(dest => dest.NewElectric, opt => opt.MapFrom(src => src.MeterReading.NewElectric))
                .ForMember(dest => dest.OldWater, opt => opt.MapFrom(src => src.MeterReading.OldWater))
                .ForMember(dest => dest.NewWater, opt => opt.MapFrom(src => src.MeterReading.NewWater))
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Contract.Tenant.FullName));
        }
    }
}