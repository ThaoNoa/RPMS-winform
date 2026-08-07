using Microsoft.EntityFrameworkCore;
using RPMS.DAL.Data;
using System.Threading.Tasks;

namespace RPMS.DAL
{
    public static class DatabaseSchemaUpdater
    {
        public static async Task EnsureUpdatedAsync(RPMSContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Cột mở rộng Reviews (script SQL gốc chưa có)
            await ExecAsync(context, @"
IF OBJECT_ID('Reviews', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Reviews', 'LandlordReply') IS NULL
        ALTER TABLE Reviews ADD LandlordReply nvarchar(max) NULL;
    IF COL_LENGTH('Reviews', 'LandlordReplyDate') IS NULL
        ALTER TABLE Reviews ADD LandlordReplyDate datetime NULL;
END
");

            // Chat (app có, script SQL gốc chưa có)
            await ExecAsync(context, @"
IF OBJECT_ID('Users', 'U') IS NOT NULL AND OBJECT_ID('ChatConversations', 'U') IS NULL
BEGIN
    CREATE TABLE ChatConversations (
        ConversationID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LandlordID int NOT NULL,
        TenantID int NOT NULL,
        CreatedDate datetime NOT NULL CONSTRAINT DF_ChatConversations_CreatedDate DEFAULT GETDATE(),
        UpdatedDate datetime NOT NULL CONSTRAINT DF_ChatConversations_UpdatedDate DEFAULT GETDATE(),
        LastMessageAt datetime NULL,
        CONSTRAINT UQ_ChatConversations_Pair UNIQUE (LandlordID, TenantID),
        CONSTRAINT FK_ChatConversations_Landlord FOREIGN KEY (LandlordID) REFERENCES Users(UserID),
        CONSTRAINT FK_ChatConversations_Tenant FOREIGN KEY (TenantID) REFERENCES Users(UserID)
    );
END
");

            await ExecAsync(context, @"
IF OBJECT_ID('ChatConversations', 'U') IS NOT NULL AND OBJECT_ID('ChatMessages', 'U') IS NULL
BEGIN
    CREATE TABLE ChatMessages (
        MessageID int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ConversationID int NOT NULL,
        SenderID int NOT NULL,
        Content nvarchar(4000) NOT NULL,
        ImagePath nvarchar(255) NULL,
        IsRead bit NOT NULL CONSTRAINT DF_ChatMessages_IsRead DEFAULT 0,
        CreatedDate datetime NOT NULL CONSTRAINT DF_ChatMessages_CreatedDate DEFAULT GETDATE(),
        CONSTRAINT FK_ChatMessages_Conversation FOREIGN KEY (ConversationID) REFERENCES ChatConversations(ConversationID) ON DELETE CASCADE,
        CONSTRAINT FK_ChatMessages_Sender FOREIGN KEY (SenderID) REFERENCES Users(UserID)
    );
    CREATE INDEX IX_ChatMessages_ConversationID ON ChatMessages(ConversationID);
END
");

            // Hợp đồng nháp: TenantID nullable (tách riêng — không phụ thuộc CHECK)
            await ExecAsync(context, @"
IF OBJECT_ID('Contracts', 'U') IS NOT NULL
AND EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Contracts') AND name = 'TenantID' AND is_nullable = 0)
BEGIN
    ALTER TABLE Contracts ALTER COLUMN TenantID int NULL;
END
");

            // Status: Draft / PendingConfirm / Active / Expired / Terminated
            await ExecAsync(context, @"
IF OBJECT_ID('Contracts', 'U') IS NOT NULL
AND EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_Contracts_Status'
      AND parent_object_id = OBJECT_ID('Contracts')
      AND definition NOT LIKE N'%PendingConfirm%')
BEGIN
    ALTER TABLE Contracts DROP CONSTRAINT CK_Contracts_Status;
    ALTER TABLE Contracts WITH NOCHECK ADD CONSTRAINT CK_Contracts_Status
        CHECK ([Status] IN (N'Draft', N'PendingConfirm', N'Active', N'Expired', N'Terminated'));
END
");

            // Status cho phép Draft — chỉ đổi khi định nghĩa cũ thiếu Draft (legacy)
            await ExecAsync(context, @"
IF OBJECT_ID('Contracts', 'U') IS NOT NULL
AND EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_Contracts_Status'
      AND parent_object_id = OBJECT_ID('Contracts')
      AND definition NOT LIKE N'%Draft%')
BEGIN
    ALTER TABLE Contracts DROP CONSTRAINT CK_Contracts_Status;
    ALTER TABLE Contracts WITH NOCHECK ADD CONSTRAINT CK_Contracts_Status
        CHECK ([Status] IN (N'Draft', N'PendingConfirm', N'Active', N'Expired', N'Terminated'));
END
");

            // Cột sửa HĐ / đổi giá — mỗi cột một lệnh để không bị rollback cả batch
            await EnsureContractColumnAsync(context, "PendingMonthlyRent", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PendingElectricPrice", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PendingWaterPrice", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PendingDeposit", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PendingEndDate", "date NULL");
            await EnsureContractColumnAsync(context, "PendingEditStatus", "nvarchar(20) NULL");
            await EnsureContractColumnAsync(context, "PendingEditNote", "nvarchar(500) NULL");
            await EnsureContractColumnAsync(context, "PendingEditAt", "datetime NULL");
            await EnsureContractColumnAsync(context, "PreviousMonthlyRent", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PreviousElectricPrice", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PreviousWaterPrice", "decimal(18,2) NULL");
            await EnsureContractColumnAsync(context, "PriceEffectiveDate", "datetime NULL");
            await EnsureContractColumnAsync(context, "CancelRequestStatus", "nvarchar(20) NULL");
            await EnsureContractColumnAsync(context, "CancelRequestedBy", "nvarchar(20) NULL");
            await EnsureContractColumnAsync(context, "CancelRequestNote", "nvarchar(500) NULL");
            await EnsureContractColumnAsync(context, "CancelRequestAt", "datetime NULL");

            await ExecAsync(context, @"
IF OBJECT_ID('Notifications', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Notifications', 'ActionType') IS NULL
        ALTER TABLE Notifications ADD ActionType nvarchar(50) NULL;
    IF COL_LENGTH('Notifications', 'RelatedID') IS NULL
        ALTER TABLE Notifications ADD RelatedID int NULL;
    IF COL_LENGTH('Notifications', 'ActionStatus') IS NULL
        ALTER TABLE Notifications ADD ActionStatus nvarchar(20) NULL;
END
");

            // Amenities catalog: không insert ở đây.
            // Sample SQL bị mojibake + INSERT theo tên đúng Unicode → trùng khi DataSeeder rename theo ID.
            // DataSeeder.EnsureAmenitiesAsync gộp/xóa bản thừa và bổ sung thiếu.
        }

        private static async Task EnsureContractColumnAsync(RPMSContext context, string columnName, string sqlType)
        {
            await ExecAsync(context, $@"
IF OBJECT_ID('Contracts', 'U') IS NOT NULL AND COL_LENGTH('Contracts', '{columnName}') IS NULL
    ALTER TABLE Contracts ADD [{columnName}] {sqlType};
");
        }

        private static async Task ExecAsync(RPMSContext context, string sql)
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
