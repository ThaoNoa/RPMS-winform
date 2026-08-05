-- ============================================================
-- RPMS - SCRIPT TẠO DATABASE ĐẦY ĐỦ (Schema + Index + Sample Data)
-- ============================================================

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RPMS')
BEGIN
    ALTER DATABASE RPMS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RPMS;
END
GO

CREATE DATABASE RPMS
ON PRIMARY ( NAME = RPMS, FILENAME = 'C:\Users\ACER\RPMS\RPMS.mdf' )
LOG ON ( NAME = RPMS_log, FILENAME = 'C:\Users\ACER\RPMS\RPMS_log.ldf' );
GO

USE RPMS;
GO

CREATE TABLE Roles (
 RoleID INT IDENTITY(1,1) PRIMARY KEY,
 RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Users (
 UserID INT IDENTITY(1,1) PRIMARY KEY,
 RoleID INT NOT NULL,
 FullName NVARCHAR(100) NOT NULL,
 Phone NVARCHAR(20) NULL,
 Email NVARCHAR(100) NULL UNIQUE,
 Username NVARCHAR(50) NOT NULL UNIQUE,
 [Password] NVARCHAR(255) NOT NULL,
 [Address] NVARCHAR(255) NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Active',
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Users_Role FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
 CONSTRAINT CK_Users_Status CHECK ([Status] IN (N'Active', N'Inactive'))
);
GO

CREATE TABLE Houses (
 HouseID INT IDENTITY(1,1) PRIMARY KEY,
 OwnerID INT NOT NULL,
 HouseName NVARCHAR(100) NOT NULL,
 [Address] NVARCHAR(255) NOT NULL,
 Description NVARCHAR(MAX) NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Active',
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Houses_User FOREIGN KEY (OwnerID) REFERENCES Users(UserID),
 CONSTRAINT CK_Houses_Status CHECK ([Status] IN (N'Active', N'Inactive'))
);
GO

CREATE TABLE Rooms (
 RoomID INT IDENTITY(1,1) PRIMARY KEY,
 HouseID INT NOT NULL,
 RoomNumber NVARCHAR(20) NOT NULL,
 Floor INT NULL,
 Area DECIMAL(10,2) NOT NULL,
 Price DECIMAL(18,2) NOT NULL,
 Capacity INT NOT NULL DEFAULT 1,
 Bedroom INT NOT NULL DEFAULT 0,
 Bathroom INT NOT NULL DEFAULT 0,
 Furniture NVARCHAR(500) NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Available',
 Description NVARCHAR(MAX) NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Rooms_House FOREIGN KEY (HouseID) REFERENCES Houses(HouseID),
 CONSTRAINT UQ_Rooms_House_RoomNumber UNIQUE (HouseID, RoomNumber),
 CONSTRAINT CK_Rooms_Status CHECK ([Status] IN (N'Available', N'Occupied', N'Maintenance')),
 CONSTRAINT CK_Rooms_Price CHECK (Price > 0),
 CONSTRAINT CK_Rooms_Area CHECK (Area > 0),
 CONSTRAINT CK_Rooms_Capacity CHECK (Capacity >= 1),
 CONSTRAINT CK_Rooms_Bedroom CHECK (Bedroom >= 0),
 CONSTRAINT CK_Rooms_Bathroom CHECK (Bathroom >= 0)
);
GO

CREATE TABLE RoomImages (
 ImageID INT IDENTITY(1,1) PRIMARY KEY,
 RoomID INT NOT NULL,
 ImagePath NVARCHAR(255) NOT NULL,
 DisplayOrder INT NOT NULL DEFAULT 0,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_RoomImages_Room FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID) ON DELETE CASCADE
);
GO

CREATE TABLE Amenities (
 AmenityID INT IDENTITY(1,1) PRIMARY KEY,
 AmenityName NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE RoomAmenities (
 RoomAmenityID INT IDENTITY(1,1) PRIMARY KEY,
 RoomID INT NOT NULL,
 AmenityID INT NOT NULL,
 CONSTRAINT FK_RoomAmenities_Room FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID) ON DELETE CASCADE,
 CONSTRAINT FK_RoomAmenities_Amenity FOREIGN KEY (AmenityID) REFERENCES Amenities(AmenityID),
 CONSTRAINT UQ_RoomAmenities_Room_Amenity UNIQUE (RoomID, AmenityID)
);
GO

CREATE TABLE Posts (
 PostID INT IDENTITY(1,1) PRIMARY KEY,
 RoomID INT NOT NULL,
 Title NVARCHAR(200) NOT NULL,
 Description NVARCHAR(MAX) NULL,
 PriceSnapshot DECIMAL(18,2) NOT NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Pending',
 ViewCount INT NOT NULL DEFAULT 0,
 ExpiryDate DATE NULL,
 IsFeatured BIT NOT NULL DEFAULT 0,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 ApprovedBy INT NULL,
 ApprovedDate DATETIME NULL,
 CONSTRAINT FK_Posts_Room FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID) ON DELETE CASCADE,
 CONSTRAINT FK_Posts_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Users(UserID),
 CONSTRAINT CK_Posts_Status CHECK ([Status] IN (N'Pending', N'Approved', N'Rejected')),
 CONSTRAINT CK_Posts_PriceSnapshot CHECK (PriceSnapshot > 0)
);
GO

CREATE TABLE PostImages (
 PostImageID INT IDENTITY(1,1) PRIMARY KEY,
 PostID INT NOT NULL,
 ImagePath NVARCHAR(255) NOT NULL,
 IsMain BIT NOT NULL DEFAULT 0,
 DisplayOrder INT NOT NULL DEFAULT 0,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_PostImages_Post FOREIGN KEY (PostID) REFERENCES Posts(PostID) ON DELETE CASCADE
);
GO

CREATE TABLE Favorites (
 FavoriteID INT IDENTITY(1,1) PRIMARY KEY,
 UserID INT NOT NULL,
 RoomID INT NOT NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Favorites_User FOREIGN KEY (UserID) REFERENCES Users(UserID),
 CONSTRAINT FK_Favorites_Room FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID) ON DELETE CASCADE,
 CONSTRAINT UQ_Favorites_User_Room UNIQUE (UserID, RoomID)
);
GO

CREATE TABLE Appointments (
 AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
 RoomID INT NOT NULL,
 TenantID INT NOT NULL,
 AppointmentDate DATETIME NOT NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Pending',
 Note NVARCHAR(MAX) NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Appointments_Room FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID) ON DELETE CASCADE,
 CONSTRAINT FK_Appointments_Tenant FOREIGN KEY (TenantID) REFERENCES Users(UserID),
 CONSTRAINT CK_Appointments_Status CHECK ([Status] IN (N'Pending', N'Accepted', N'Rejected', N'Completed'))
);
GO

CREATE TABLE Contracts (
 ContractID INT IDENTITY(1,1) PRIMARY KEY,
 ContractCode NVARCHAR(20) NOT NULL UNIQUE,
 RoomID INT NOT NULL,
 TenantID INT NULL,
 StartDate DATE NOT NULL,
 EndDate DATE NOT NULL,
 MoveInDate DATE NULL,
 MoveOutDate DATE NULL,
 Deposit DECIMAL(18,2) NOT NULL DEFAULT 0,
 MonthlyRent DECIMAL(18,2) NOT NULL,
 ElectricPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
 WaterPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Active',
 CreatedBy INT NOT NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Contracts_Room FOREIGN KEY (RoomID) REFERENCES Rooms(RoomID),
 CONSTRAINT FK_Contracts_Tenant FOREIGN KEY (TenantID) REFERENCES Users(UserID),
 CONSTRAINT FK_Contracts_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID),
 CONSTRAINT CK_Contracts_Status CHECK ([Status] IN (N'Draft', N'PendingConfirm', N'Active', N'Expired', N'Terminated')),
 CONSTRAINT CK_Contracts_Date CHECK (EndDate >= StartDate),
 CONSTRAINT CK_Contracts_MoveOut CHECK (MoveOutDate IS NULL OR MoveOutDate >= MoveInDate),
 CONSTRAINT CK_Contracts_Deposit CHECK (Deposit >= 0),
 CONSTRAINT CK_Contracts_MonthlyRent CHECK (MonthlyRent > 0),
 CONSTRAINT CK_Contracts_ElectricWater CHECK (ElectricPrice >= 0 AND WaterPrice >= 0)
);
GO

CREATE TABLE MeterReadings (
 ReadingID INT IDENTITY(1,1) PRIMARY KEY,
 ContractID INT NOT NULL,
 ReadingMonth DATE NOT NULL,
 OldElectric DECIMAL(18,2) NOT NULL DEFAULT 0,
 NewElectric DECIMAL(18,2) NOT NULL DEFAULT 0,
 OldWater DECIMAL(18,2) NOT NULL DEFAULT 0,
 NewWater DECIMAL(18,2) NOT NULL DEFAULT 0,
 CreatedBy INT NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_MeterReadings_Contract FOREIGN KEY (ContractID) REFERENCES Contracts(ContractID),
 CONSTRAINT FK_MeterReadings_User FOREIGN KEY (CreatedBy) REFERENCES Users(UserID),
 CONSTRAINT CK_MeterReadings_Electric CHECK (NewElectric >= OldElectric),
 CONSTRAINT CK_MeterReadings_Water CHECK (NewWater >= OldWater)
);
GO

CREATE TABLE Invoices (
 InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
 InvoiceCode NVARCHAR(20) NOT NULL UNIQUE,
 ContractID INT NOT NULL,
 ReadingID INT NOT NULL,
 Rent DECIMAL(18,2) NOT NULL DEFAULT 0,
 ElectricCost DECIMAL(18,2) NOT NULL DEFAULT 0,
 WaterCost DECIMAL(18,2) NOT NULL DEFAULT 0,
 OtherFee DECIMAL(18,2) NOT NULL DEFAULT 0,
 Total DECIMAL(18,2) NOT NULL DEFAULT 0,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Unpaid',
 DueDate DATE NULL,
 PaidDate DATE NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Invoices_Contract FOREIGN KEY (ContractID) REFERENCES Contracts(ContractID),
 CONSTRAINT FK_Invoices_MeterReading FOREIGN KEY (ReadingID) REFERENCES MeterReadings(ReadingID),
 CONSTRAINT CK_Invoices_Status CHECK ([Status] IN (N'Unpaid', N'Paid', N'Overdue')),
 CONSTRAINT CK_Invoices_Total CHECK (Total >= 0),
 CONSTRAINT CK_Invoices_Rent CHECK (Rent >= 0),
 CONSTRAINT CK_Invoices_ElectricCost CHECK (ElectricCost >= 0),
 CONSTRAINT CK_Invoices_WaterCost CHECK (WaterCost >= 0),
 CONSTRAINT CK_Invoices_OtherFee CHECK (OtherFee >= 0)
);
GO

CREATE TABLE Payments (
 PaymentID INT IDENTITY(1,1) PRIMARY KEY,
 InvoiceID INT NOT NULL,
 PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
 Amount DECIMAL(18,2) NOT NULL,
 Method NVARCHAR(50) NOT NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Completed',
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Payments_Invoice FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID),
 CONSTRAINT CK_Payments_Method CHECK (Method IN (N'Cash', N'Banking', N'Momo', N'VNPay', N'ZaloPay')),
 CONSTRAINT CK_Payments_Status CHECK ([Status] IN (N'Pending', N'Completed', N'Failed')),
 CONSTRAINT CK_Payments_Amount CHECK (Amount > 0)
);
GO

CREATE TABLE MaintenanceRequests (
 RequestID INT IDENTITY(1,1) PRIMARY KEY,
 ContractID INT NOT NULL,
 Title NVARCHAR(200) NOT NULL,
 Description NVARCHAR(MAX) NULL,
 Image NVARCHAR(255) NULL,
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Pending',
 AssignedManager INT NULL,
 CompletedDate DATE NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_MaintenanceRequests_Contract FOREIGN KEY (ContractID) REFERENCES Contracts(ContractID),
 CONSTRAINT FK_MaintenanceRequests_Manager FOREIGN KEY (AssignedManager) REFERENCES Users(UserID),
 CONSTRAINT CK_MaintenanceRequests_Status CHECK ([Status] IN (N'Pending', N'Processing', N'Completed'))
);
GO

CREATE TABLE Assignments (
 AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
 HouseID INT NOT NULL,
 ManagerID INT NOT NULL,
 AssignedDate DATETIME NOT NULL DEFAULT GETDATE(),
 [Status] NVARCHAR(20) NOT NULL DEFAULT N'Active',
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Assignments_House FOREIGN KEY (HouseID) REFERENCES Houses(HouseID),
 CONSTRAINT FK_Assignments_Manager FOREIGN KEY (ManagerID) REFERENCES Users(UserID),
 CONSTRAINT UQ_Assignments_House_Manager UNIQUE (HouseID, ManagerID),
 CONSTRAINT CK_Assignments_Status CHECK ([Status] IN (N'Active', N'Inactive'))
);
GO

CREATE TABLE Reviews (
 ReviewID INT IDENTITY(1,1) PRIMARY KEY,
 ContractID INT NOT NULL UNIQUE,
 Rating INT NOT NULL,
 Comment NVARCHAR(MAX) NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Reviews_Contract FOREIGN KEY (ContractID) REFERENCES Contracts(ContractID),
 CONSTRAINT CK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5)
);
GO

CREATE TABLE Notifications (
 NotificationID INT IDENTITY(1,1) PRIMARY KEY,
 UserID INT NOT NULL,
 Title NVARCHAR(200) NOT NULL,
 Content NVARCHAR(MAX) NOT NULL,
 IsRead BIT NOT NULL DEFAULT 0,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_Notifications_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE ActivityLogs (
 LogID INT IDENTITY(1,1) PRIMARY KEY,
 UserID INT NOT NULL,
 Action NVARCHAR(200) NOT NULL,
 Details NVARCHAR(MAX) NULL,
 IPAddress NVARCHAR(45) NULL,
 CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
 CONSTRAINT FK_ActivityLogs_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_RoleID ON Users(RoleID);
CREATE INDEX IX_Users_Status ON Users([Status]);
CREATE INDEX IX_Houses_OwnerID ON Houses(OwnerID);
CREATE INDEX IX_Rooms_HouseID ON Rooms(HouseID);
CREATE INDEX IX_Rooms_Status ON Rooms([Status]);
CREATE INDEX IX_Rooms_Price ON Rooms(Price);
CREATE INDEX IX_RoomImages_RoomID ON RoomImages(RoomID);
CREATE INDEX IX_RoomAmenities_RoomID ON RoomAmenities(RoomID);
CREATE INDEX IX_Posts_RoomID ON Posts(RoomID);
CREATE INDEX IX_Posts_Status ON Posts([Status]);
CREATE INDEX IX_Posts_ExpiryDate ON Posts(ExpiryDate);
CREATE INDEX IX_Posts_IsFeatured ON Posts(IsFeatured);
CREATE INDEX IX_PostImages_PostID ON PostImages(PostID);
CREATE INDEX IX_Favorites_UserID ON Favorites(UserID);
CREATE INDEX IX_Favorites_RoomID ON Favorites(RoomID);
CREATE INDEX IX_Appointments_RoomID ON Appointments(RoomID);
CREATE INDEX IX_Appointments_TenantID ON Appointments(TenantID);
CREATE INDEX IX_Appointments_Status ON Appointments([Status]);
CREATE INDEX IX_Contracts_RoomID ON Contracts(RoomID);
CREATE INDEX IX_Contracts_TenantID ON Contracts(TenantID);
CREATE INDEX IX_Contracts_Status ON Contracts([Status]);
CREATE INDEX IX_Contracts_ContractCode ON Contracts(ContractCode);
CREATE INDEX IX_MeterReadings_ContractID ON MeterReadings(ContractID);
CREATE INDEX IX_MeterReadings_ReadingMonth ON MeterReadings(ReadingMonth);
CREATE INDEX IX_Invoices_ContractID ON Invoices(ContractID);
CREATE INDEX IX_Invoices_Status ON Invoices([Status]);
CREATE INDEX IX_Invoices_InvoiceCode ON Invoices(InvoiceCode);
CREATE INDEX IX_Payments_InvoiceID ON Payments(InvoiceID);
CREATE INDEX IX_Payments_Method ON Payments(Method);
CREATE INDEX IX_Payments_Status ON Payments([Status]);
CREATE INDEX IX_MaintenanceRequests_ContractID ON MaintenanceRequests(ContractID);
CREATE INDEX IX_MaintenanceRequests_Status ON MaintenanceRequests([Status]);
CREATE INDEX IX_Assignments_HouseID ON Assignments(HouseID);
CREATE INDEX IX_Assignments_ManagerID ON Assignments(ManagerID);
CREATE INDEX IX_Notifications_UserID ON Notifications(UserID);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_ActivityLogs_UserID ON ActivityLogs(UserID);
CREATE INDEX IX_ActivityLogs_CreatedDate ON ActivityLogs(CreatedDate);
GO

SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (RoleID, RoleName) VALUES (1, N'Admin'), (2, N'Landlord'), (3, N'Tenant'), (4, N'Manager');
SET IDENTITY_INSERT Roles OFF;
GO

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserID, RoleID, FullName, Phone, Email, Username, [Password], [Address], [Status], CreatedDate, UpdatedDate) VALUES
(1, 1, N'Quản trị viên', N'0900123456', N'admin@rpms.com', N'admin', N'admin123', N'Hà Nội', N'Active', GETDATE(), GETDATE()),
(2, 2, N'Nguyễn Văn Nam', N'0912345678', N'nam@landlord.com', N'namlandlord', N'123456', N'123 Đường A, Quận B, TP.HCM', N'Active', GETDATE(), GETDATE()),
(3, 3, N'Trần Văn An', N'0923456789', N'an@tenant.com', N'tenant', N'123456', N'456 Đường C, Quận D, TP.HCM', N'Active', GETDATE(), GETDATE()),
(4, 4, N'Lê Thị Mai', N'0934567890', N'mai@manager.com', N'manager', N'123456', N'789 Đường E, Quận F, TP.HCM', N'Active', GETDATE(), GETDATE());
SET IDENTITY_INSERT Users OFF;
GO

SET IDENTITY_INSERT Houses ON;
INSERT INTO Houses (HouseID, OwnerID, HouseName, [Address], Description, [Status], CreatedDate, UpdatedDate) VALUES
(1, 2, N'Nhà trọ Nam', N'123 Đường A, Quận B, TP.HCM', N'Nhà cho thuê nhiều phòng', N'Active', GETDATE(), GETDATE());
SET IDENTITY_INSERT Houses OFF;
GO

SET IDENTITY_INSERT Rooms ON;
INSERT INTO Rooms (RoomID, HouseID, RoomNumber, Floor, Area, Price, Capacity, Bedroom, Bathroom, Furniture, [Status], Description, CreatedDate, UpdatedDate) VALUES
(1, 1, N'101', 1, 25.0, 3000000, 2, 1, 1, N'Giường, tủ quần áo, điều hòa', N'Occupied', N'Phòng đẹp, có cửa sổ', GETDATE(), GETDATE()),
(2, 1, N'102', 1, 30.0, 3500000, 2, 1, 1, N'Giường, tủ, điều hòa, ban công', N'Available', N'Phòng rộng, có ban công', GETDATE(), GETDATE());
SET IDENTITY_INSERT Rooms OFF;
GO

SET IDENTITY_INSERT Amenities ON;
INSERT INTO Amenities (AmenityID, AmenityName) VALUES
(1, N'Điều hòa'), (2, N'Nóng lạnh'), (3, N'Wifi'), (4, N'Ban công'), (5, N'Bếp'), (6, N'Gara xe'),
(7, N'Máy giặt'), (8, N'Tủ lạnh'), (9, N'Tủ quần áo'), (10, N'Bồn rửa bát'), (11, N'Sofa'), (12, N'Bàn ghế');
SET IDENTITY_INSERT Amenities OFF;
GO

SET IDENTITY_INSERT RoomAmenities ON;
INSERT INTO RoomAmenities (RoomAmenityID, RoomID, AmenityID) VALUES
(1, 1, 1), (2, 1, 2), (3, 1, 3), (4, 2, 1), (5, 2, 3), (6, 2, 4);
SET IDENTITY_INSERT RoomAmenities OFF;
GO

SET IDENTITY_INSERT RoomImages ON;
INSERT INTO RoomImages (ImageID, RoomID, ImagePath, DisplayOrder, CreatedDate, UpdatedDate) VALUES
(1, 1, N'/uploads/rooms/101_1.jpg', 1, GETDATE(), GETDATE()),
(2, 1, N'/uploads/rooms/101_2.jpg', 2, GETDATE(), GETDATE()),
(3, 2, N'/uploads/rooms/102_1.jpg', 1, GETDATE(), GETDATE());
SET IDENTITY_INSERT RoomImages OFF;
GO

SET IDENTITY_INSERT Contracts ON;
INSERT INTO Contracts (ContractID, ContractCode, RoomID, TenantID, StartDate, EndDate, MoveInDate, MoveOutDate, Deposit, MonthlyRent, ElectricPrice, WaterPrice, [Status], CreatedBy, CreatedDate, UpdatedDate)
VALUES (1, N'HD00001', 1, 3,
  DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 15),
  DATEADD(day, -1, DATEADD(year, 1, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 15))),
  DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 15),
  NULL, 3000000, 3000000, 3500, 50000, N'Active', 2, GETDATE(), GETDATE());
SET IDENTITY_INSERT Contracts OFF;
GO

-- Seed tối thiểu tháng T-3; DataSeeder sẽ bổ sung đủ 3 tháng đã qua (T-3, T-2, T-1) khi mở app
SET IDENTITY_INSERT MeterReadings ON;
INSERT INTO MeterReadings (ReadingID, ContractID, ReadingMonth, OldElectric, NewElectric, OldWater, NewWater, CreatedBy, CreatedDate, UpdatedDate) VALUES
(1, 1, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 1), 1000, 1100, 50, 55, 4, GETDATE(), GETDATE());
SET IDENTITY_INSERT MeterReadings OFF;
GO

SET IDENTITY_INSERT Invoices ON;
INSERT INTO Invoices (InvoiceID, InvoiceCode, ContractID, ReadingID, Rent, ElectricCost, WaterCost, OtherFee, Total, [Status], DueDate, PaidDate, CreatedDate, UpdatedDate)
SELECT 1, N'INV00001', 1, 1,
  CAST(ROUND(3000000.0 * DATEDIFF(day, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 15), EOMONTH(DATEADD(month, -3, GETDATE()))) + 1) / DAY(EOMONTH(DATEADD(month, -3, GETDATE()))), 0) AS DECIMAL(18,2)),
  350000, 250000, 0,
  CAST(ROUND(3000000.0 * DATEDIFF(day, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 15), EOMONTH(DATEADD(month, -3, GETDATE()))) + 1) / DAY(EOMONTH(DATEADD(month, -3, GETDATE()))), 0) AS DECIMAL(18,2)) + 350000 + 250000,
  N'Paid',
  EOMONTH(DATEADD(month, -3, GETDATE())),
  DATEADD(day, 20, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 1)),
  GETDATE(), GETDATE();
SET IDENTITY_INSERT Invoices OFF;
GO

SET IDENTITY_INSERT Payments ON;
INSERT INTO Payments (PaymentID, InvoiceID, PaymentDate, Amount, Method, [Status], CreatedDate, UpdatedDate)
SELECT 1, 1,
  DATEADD(day, 20, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 1)),
  CAST(ROUND(3000000.0 * DATEDIFF(day, DATEFROMPARTS(YEAR(DATEADD(month, -3, GETDATE())), MONTH(DATEADD(month, -3, GETDATE())), 15), EOMONTH(DATEADD(month, -3, GETDATE()))) + 1) / DAY(EOMONTH(DATEADD(month, -3, GETDATE()))), 0) AS DECIMAL(18,2)) + 350000 + 250000,
  N'Banking', N'Completed', GETDATE(), GETDATE();
SET IDENTITY_INSERT Payments OFF;
GO

SET IDENTITY_INSERT Posts ON;
INSERT INTO Posts (PostID, RoomID, Title, Description, PriceSnapshot, [Status], ViewCount, ExpiryDate, IsFeatured, CreatedDate, UpdatedDate, ApprovedBy, ApprovedDate) VALUES
(1, 1, N'Cho thuê phòng 101 giá rẻ', N'Phòng đẹp, đầy đủ tiện nghi', 3000000, N'Approved', 150, DATEADD(month, 1, GETDATE()), 1, GETDATE(), GETDATE(), 1, GETDATE());
SET IDENTITY_INSERT Posts OFF;
GO

SET IDENTITY_INSERT PostImages ON;
INSERT INTO PostImages (PostImageID, PostID, ImagePath, IsMain, DisplayOrder, CreatedDate, UpdatedDate) VALUES
(1, 1, N'/uploads/posts/101_1.jpg', 1, 1, GETDATE(), GETDATE()),
(2, 1, N'/uploads/posts/101_2.jpg', 0, 2, GETDATE(), GETDATE());
SET IDENTITY_INSERT PostImages OFF;
GO

SET IDENTITY_INSERT Appointments ON;
INSERT INTO Appointments (AppointmentID, RoomID, TenantID, AppointmentDate, [Status], Note, CreatedDate, UpdatedDate) VALUES
(1, 2, 3, DATEADD(hour, 9, CAST(DATEADD(day, 2, CAST(GETDATE() AS DATE)) AS DATETIME)), N'Pending', N'Khách muốn xem phòng', GETDATE(), GETDATE());
SET IDENTITY_INSERT Appointments OFF;
GO

SET IDENTITY_INSERT Favorites ON;
INSERT INTO Favorites (FavoriteID, UserID, RoomID, CreatedDate) VALUES (1, 3, 2, GETDATE());
SET IDENTITY_INSERT Favorites OFF;
GO

SET IDENTITY_INSERT MaintenanceRequests ON;
INSERT INTO MaintenanceRequests (RequestID, ContractID, Title, Description, Image, [Status], AssignedManager, CompletedDate, CreatedDate, UpdatedDate) VALUES
(1, 1, N'Bóng đèn hỏng', N'Bóng đèn phòng tắm không sáng', NULL, N'Pending', NULL, NULL, GETDATE(), GETDATE());
SET IDENTITY_INSERT MaintenanceRequests OFF;
GO

SET IDENTITY_INSERT Assignments ON;
INSERT INTO Assignments (AssignmentID, HouseID, ManagerID, AssignedDate, [Status], CreatedDate, UpdatedDate)
VALUES (1, 1, 4, GETDATE(), N'Active', GETDATE(), GETDATE());
SET IDENTITY_INSERT Assignments OFF;
GO

SET IDENTITY_INSERT Reviews ON;
INSERT INTO Reviews (ReviewID, ContractID, Rating, Comment, CreatedDate, UpdatedDate) VALUES
(1, 1, 5, N'Phòng đẹp, chủ nhà thân thiện, tiện nghi đầy đủ. Rất hài lòng!', GETDATE(), GETDATE());
SET IDENTITY_INSERT Reviews OFF;
GO

SET IDENTITY_INSERT Notifications ON;
INSERT INTO Notifications (NotificationID, UserID, Title, Content, IsRead, CreatedDate, UpdatedDate) VALUES
(1, 2, N'Có lịch hẹn mới', N'Người thuê Trần Văn An đặt lịch xem phòng 102 (lịch gần đây)', 0, GETDATE(), GETDATE());
SET IDENTITY_INSERT Notifications OFF;
GO

SET IDENTITY_INSERT ActivityLogs ON;
INSERT INTO ActivityLogs (LogID, UserID, Action, Details, IPAddress, CreatedDate) VALUES
(1, 1, N'Đăng nhập', N'Admin đăng nhập hệ thống', N'192.168.1.1', GETDATE()),
(2, 2, N'Tạo hợp đồng', N'Tạo hợp đồng HD00001 cho phòng 101', N'192.168.1.2', GETDATE());
SET IDENTITY_INSERT ActivityLogs OFF;
GO

PRINT 'RPMS database created successfully.';
GO