using BCrypt.Net;

Console.WriteLine("Đang tạo hash BCrypt cho mật khẩu mẫu...");
Console.WriteLine("==========================================");

// Tạo hash cho mật khẩu "admin123"
string hashAdmin = BCrypt.Net.BCrypt.HashPassword("admin123");
Console.WriteLine($"Hash cho 'admin123': {hashAdmin}");

// Tạo hash cho mật khẩu "123456"
string hashDefault = BCrypt.Net.BCrypt.HashPassword("123456");
Console.WriteLine($"Hash cho '123456': {hashDefault}");

Console.WriteLine("==========================================");
Console.WriteLine("Copy các hash trên để dùng trong SQL.");
Console.WriteLine("Nhấn phím bất kỳ để thoát...");
Console.ReadKey();