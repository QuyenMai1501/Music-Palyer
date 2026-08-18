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
- `wwwroot/` - CSS/JS và ảnh mặc định (media tải lên nằm ở thư mục `media/` ngoài wwwroot)

## Hướng dẫn cài đặt

1. Cài .NET 8 SDK.
2. Chuẩn bị MySQL và tạo database `sql_music`:
   ```sql
   CREATE DATABASE sql_music;
   ```
3. Chuỗi kết nối không được commit. Cấu hình qua user-secrets (môi trường Development) hoặc biến môi trường `ConnectionStrings__MySQLConnection`:
   ```
   dotnet user-secrets set "ConnectionStrings:MySQLConnection" "server=localhost;port=3306;database=sql_music;user=root;password=<pass>"
   ```
4. Khôi phục các gói NuGet: `dotnet restore`
5. Tạo bảng từ migrations (KHÔNG tự chạy khi khởi động):
   ```
   dotnet ef database update
   ```
6. Chạy ứng dụng: `dotnet run` -> truy cập http://localhost:5122

## Tạo tài khoản Admin đầu tiên

Đăng ký luôn tạo tài khoản `Role="User"`. Cách nhanh nhất: đăng ký một tài khoản thường, sau đó nâng quyền trực tiếp trong MySQL:

```sql
UPDATE Users SET Role='Admin' WHERE Username='<tên-đăng-nhập>';
```

Hoặc sau khi đã có một Admin, dùng trang `/Admin/ManageUser` để nâng quyền cho người dùng khác.

> Lưu ý: mật khẩu được hash bằng PBKDF2 (`Services/PasswordService.cs`), không tự tạo cột `PasswordHash` bằng tay. Hash SHA-256 cũ sẽ tự nâng cấp lên PBKDF2 khi người dùng đăng nhập thành công.

## API

- `GET /api/Search/{query}` - tìm bài hát theo tên hoặc ca sĩ (trả về kết quả khớp đầu tiên).

## Lưu ý về file media

- Toàn bộ media (nhạc, ảnh, lời) được lưu trong thư mục gốc cấu hình được **ngoài `wwwroot`** (mặc định `<ContentRoot>/media`) để không bị mất khi deploy lại. Cấu hình qua `Media:RootPath` (appsettings / user-secrets / biến môi trường `Media__RootPath`); để trống sẽ dùng mặc định `media/`.
- File nhạc và ảnh giữ nguyên tên gốc khi upload; file lời bài hát (.lrc) được đặt tên có tiền tố `Guid_` để tránh trùng lặp.
- Định dạng được kiểm tra phía server: nhạc `.mp3` (tối đa 100MB), ảnh `.jpg/.jpeg/.png/.webp/.gif` (tối đa 5MB), lời `.lrc/.txt` (tối đa 1MB).
- Đường dẫn file được lưu trong database dưới dạng URL ảo: `/music/...`, `/lyrics/...`, `/images/...` (giữ nguyên so với trước, không cần migration).
- Ảnh mặc định của trang (`/images/default.png`, logo, ...) vẫn nằm trong `wwwroot/images`; chỉ file upload mới nằm trong media root.
- Thư mục `media/` được thêm vào `.gitignore` — không commit file media lên git.
