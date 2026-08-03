using RPMS.BLL.Helpers;
using RPMS.DAL.Data;
using RPMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(RPMSContext db)
        {
            await db.Database.EnsureCreatedAsync();

            if (!await db.Roles.AnyAsync())
            {
                db.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Landlord" },
                    new Role { RoleName = "Tenant" },
                    new Role { RoleName = "Manager" });
                await db.SaveChangesAsync();
            }

            var roles = await db.Roles.ToListAsync();
            int adminRole = roles.First(r => r.RoleName == "Admin").RoleID;
            int landlordRole = roles.First(r => r.RoleName == "Landlord").RoleID;
            int tenantRole = roles.First(r => r.RoleName == "Tenant").RoleID;
            int managerRole = roles.First(r => r.RoleName == "Manager").RoleID;

            async Task EnsureUser(string username, string fullName, int roleId, string email)
            {
                if (await db.Users.AnyAsync(u => u.Username == username)) return;
                db.Users.Add(new User
                {
                    Username = username,
                    Password = PasswordHelper.HashPassword("123456"),
                    FullName = fullName,
                    Email = email,
                    Phone = "0900000000",
                    Address = "TP.HCM",
                    RoleID = roleId,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
                await db.SaveChangesAsync();
            }

            await EnsureUser("admin", "System Admin", adminRole, "admin@rpms.local");
            await EnsureUser("landlord1", "Nguyen Van Chu", landlordRole, "landlord1@rpms.local");
            await EnsureUser("tenant1", "Tran Thi Thue", tenantRole, "tenant1@rpms.local");
            await EnsureUser("manager1", "Le Van Quan", managerRole, "manager1@rpms.local");

            if (!await db.Amenities.AnyAsync())
            {
                db.Amenities.AddRange(
                    new Amenity { AmenityName = "Điều hòa" },
                    new Amenity { AmenityName = "Wifi" },
                    new Amenity { AmenityName = "Máy giặt" },
                    new Amenity { AmenityName = "Chỗ để xe" },
                    new Amenity { AmenityName = "Cho phép thú cưng" },
                    new Amenity { AmenityName = "Nóng lạnh" },
                    new Amenity { AmenityName = "Tủ lạnh" });
                await db.SaveChangesAsync();
            }

            var landlord = await db.Users.FirstAsync(u => u.Username == "landlord1");
            if (!await db.Houses.AnyAsync(h => h.OwnerID == landlord.UserID))
            {
                var house = new House
                {
                    OwnerID = landlord.UserID,
                    HouseName = "Nhà trọ Bình Thạnh Demo",
                    Address = "123 Điện Biên Phủ, Bình Thạnh, TP.HCM",
                    Description = "Nhà trọ demo cho hệ thống RPMS",
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                db.Houses.Add(house);
                await db.SaveChangesAsync();

                var room = new Room
                {
                    HouseID = house.HouseID,
                    RoomNumber = "P101",
                    Floor = 1,
                    Area = 28,
                    Price = 4500000,
                    Capacity = 2,
                    Bedroom = 1,
                    Bathroom = 1,
                    Furniture = "Giường, tủ, bàn ghế",
                    Status = "Available",
                    Description = "Phòng demo đầy đủ nội thất",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                db.Rooms.Add(room);
                await db.SaveChangesAsync();

                var amenities = await db.Amenities.Take(4).ToListAsync();
                foreach (var a in amenities)
                {
                    db.RoomAmenities.Add(new RoomAmenity { RoomID = room.RoomID, AmenityID = a.AmenityID });
                }
                await db.SaveChangesAsync();

                var manager = await db.Users.FirstAsync(u => u.Username == "manager1");
                db.Assignments.Add(new Assignment
                {
                    HouseID = house.HouseID,
                    ManagerID = manager.UserID,
                    AssignedDate = DateTime.Now,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
                await db.SaveChangesAsync();

                db.Posts.Add(new Post
                {
                    RoomID = room.RoomID,
                    Title = "Cho thuê phòng P101 Bình Thạnh",
                    Description = "Phòng sạch sẽ, gần trung tâm, có tiện nghi đầy đủ.",
                    PriceSnapshot = room.Price,
                    Status = "Approved",
                    ViewCount = 0,
                    ExpiryDate = DateTime.Now.AddMonths(2),
                    IsFeatured = true,
                    ApprovedBy = (await db.Users.FirstAsync(u => u.Username == "admin")).UserID,
                    ApprovedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
