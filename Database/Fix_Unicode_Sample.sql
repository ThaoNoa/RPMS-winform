USE RPMS;
GO

-- Fix mojibake from sqlcmd without UTF-8 codepage
UPDATE Users SET FullName = NNguoi_placeholder, [Address] = NAddr_placeholder WHERE UserID = 1;
GO