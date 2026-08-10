# MusicPlayer

Trang web nghe nhạc trực tuyến xây dựng bằng ASP.NET Core 8.0 (Razor Pages), giao diện tiếng Việt.

## Công nghệ sử dụng

- ASP.NET Core 8.0 (Razor Pages + 1 MVC API controller)
- Entity Framework Core 8 + Pomelo.EntityFrameworkCore.MySql
- TagLibSharp (đọc thời lượng file MP3)
- Bootstrap 5, jQuery + jQuery Validation

## Tính năng

### Người dùng
- Đăng ký / Đăng nhập / Đăng xuất (mật khẩu hash SHA-256, trạng thái đăng nhập lưu trong Session)
- Quên mật khẩu / Đặt lại mật khẩu
- Nghe nhạc với trình phát, hiển thị lời bài hát (.lrc), thích / bỏ thích bài hát
- Tạo / quản lý playlist cá nhân (thêm bài, sửa tên, xóa)
- Xem bài hát thịnh hành trong tuần (tổng lượt nghe + lượt thích)
- Tìm kiếm bài hát theo tên hoặc ca sĩ
- Gửi phản hồi / liên hệ admin và xem lịch sử phản hồi
- Xem tin tức & blog

### Admin
- Quản lý bài hát: thêm, sửa, ẩn / hiện, xóa
- Quản lý người dùng: khóa / mở khóa tài khoản, nâng quyền Admin
- Trả lời phản hồi của người dùng

## Cấu trúc thư mục

- `Pages/` - Razor Pages (Account, Admin, Music, Playlists, Blog, ...)
- `Controllers/` - MVC API controller (`/api/Search/{query}`)
- `Models/` - entity EF Core (User, Song, Playlist, Play, Like, Message, ...)
- `Data/AppDbContext.cs` - DbContext
- `Services/MusicService.cs` - tính xếp hạng thịnh hành tuần
- `Helpers/SessionExtensions.cs` - tiện ích đọc / ghi Session
- `Migrations/` - migrations EF Core
- `wwwroot/` - CSS/JS và file media (music, lyrics, images)

## Hướng dẫn cài đặt

1. Cài .NET 8 SDK.
2. Chuẩn bị MySQL và tạo database `sql_music`:
   ```sql
   CREATE DATABASE sql_music;
   ```
3. Kiểm tra chuỗi kết nối trong `appsettings.json` (mặc định: server `localhost`, port `3306`, user `root`, password `Quyen_1501`).
4. Khôi phục các gói NuGet: `dotnet restore`
5. Tạo bảng từ migrations (KHÔNG tự chạy khi khởi động):
   ```
   dotnet ef database update
   ```
6. Chạy ứng dụng: `dotnet run` -> truy cập http://localhost:5122

## Tạo tài khoản Admin đầu tiên

Đăng ký luôn tạo tài khoản `Role="User"`. Tài khoản Admin đầu tiên phải được tạo thủ công trong bảng `Users` (mật khẩu là chuỗi SHA-256 hex):

```sql
INSERT INTO Users (Username, PasswordHash, Email, Role, CreateDate, IsLocked)
VALUES ('admin', '<sha256-hex-của-mật-khẩu>', 'admin@example.com', 'Admin', NOW(), 0);
```

Hoặc sau khi đã có một Admin, dùng trang `/Admin/ManageUser` để nâng quyền cho người dùng khác.

## API

- `GET /api/Search/{query}` - tìm bài hát theo tên hoặc ca sĩ (trả về kết quả khớp đầu tiên).

## Lưu ý về file media

- File nhạc và ảnh giữ nguyên tên gốc khi upload; file lời bài hát (.lrc) được đặt tên có tiền tố `Guid_` để tránh trùng lặp.
- Đường dẫn file được lưu trong database dưới dạng URL ảo: `/music/...`, `/lyrics/...`, `/images/...`.
