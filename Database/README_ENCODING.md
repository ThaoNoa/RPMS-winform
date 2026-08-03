# RPMS SQL Script — Encoding

Khi chạy `RPMS_Full.sql` bằng **sqlcmd**, phải dùng UTF-8:

```bat
sqlcmd -S .\SQLEXPRESS -E -f 65001 -i RPMS_Full.sql
```

Hoặc mở file trong **SSMS**, đảm bảo file lưu **UTF-8 with BOM**, rồi Execute (F5).

Nếu quên, tiếng Việt trong DB sẽ bị lỗi dạng `Quáº£n trá»‹ viÃªn`.
Khi đó chạy:

```bat
dotnet run --project Database\FixUnicode\FixUnicode.csproj
```

Hoặc chỉ cần **khởi động lại app** (DataSeeder tự sửa tên sample).
