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
        }

        private static async Task ExecAsync(RPMSContext context, string sql)
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
