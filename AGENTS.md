# MusicPlayer

ASP.NET Core 8.0 Razor Pages app (Vietnamese UI) + one MVC API controller. EF Core 8 + Pomelo MySQL, TagLibSharp for MP3 duration. No test project.

## Run / verify

- `dotnet run` (profile `http` -> http://localhost:5122); `dotnet build` to verify.
- Requires local MySQL: create database `sql_music` first. The connection string is NOT committed — it is read from user-secrets (Development) or env var `ConnectionStrings__MySQLConnection`. To set locally: `dotnet user-secrets set "ConnectionStrings:MySQLConnection" "server=localhost;port=3306;database=sql_music;user=root;password=<pass>"`. Uploaded media defaults to `<ContentRoot>/media` (override via `Media:RootPath`); existing `wwwroot/music|lyrics|images` files were copied there and are no longer served from `wwwroot`.
- Migrations are NOT auto-applied at startup. After schema changes: `dotnet ef migrations add <Name>` then `dotnet ef database update`.
- Note: installed `dotnet-ef` global tool is v9.0.4 vs EF packages 8.0.3; commands still work.

## Auth (important - NOT ASP.NET Identity)

- Passwords are hashed with PBKDF2 via `Services/PasswordService.cs` (wraps ASP.NET `PasswordHasher<User>`). Legacy SHA-256 hex hashes are detected and rehashed automatically on next successful login. Use `PasswordService` (not inline crypto) for all hashing.
- Login is rate-limited via session: 5 failed attempts locks the session for 15 minutes (`Helpers/SessionExtensions.cs`: `IsLoginLocked`, `RecordLoginFailure`, `ClearLoginFailures`).
- Authenticated state lives in `ISession` (`UserId`, `UserRole`, `Username`, `Useremail`) via `Helpers/SessionExtensions.cs`. The `AddAuthentication(Cookie...)` registration in `Program.cs` is effectively unused.
- Every handler guards admin pages with `HttpContext.Session.GetUserRole() == "Admin"`. Redirect target varies: `/Error`, `/Account/Login`, or `/Index` depending on page.
- No seed data: register always creates `Role="User"`. To create the first Admin: register a normal user, then promote directly in MySQL (`UPDATE Users SET Role='Admin' WHERE Username='...'`) or via `/Admin/ManageUser` once an Admin exists. Do not hand-insert `PasswordHash` (must be a PBKDF2 string, not SHA-256 hex).

## Media storage & URLs

- All media (mp3 / images / lyrics) lives under a configurable root OUTSIDE `wwwroot`, handled by `Services/MediaStorage.cs`. Root comes from `Media:RootPath` (appsettings / user-secrets / env `Media__RootPath`); default is `<ContentRoot>/media` (auto-created, gitignored — do not commit uploaded files). Use `MediaStorage` (not `_environment.WebRootPath`/`wwwroot` paths) for all media file I/O — it maps virtual paths and blocks path traversal.
- `Program.cs:61-81` maps `<root>/music` -> `/music/*` (ServeUnknownFileTypes, `audio/mpeg`), `<root>/lyrics` -> `/lyrics/*`, `<root>/images` -> `/images/*`. The `/images` mapping is registered after the default static-files middleware, so committed `wwwroot/images/*` (default art, logo) still win — only uploaded images are served from the media root.
- `Song.FilePath`/`LyricsPath`/`ImagePath` store URL paths starting with `/music/`, `/lyrics/`, `/images/`. This virtual-path format is unchanged, so no DB migration was needed when files moved out of `wwwroot`.
- Admin uploads (`Pages/Admin/AddSong.cshtml.cs`, `Pages/Admin/EditSong.cshtml.cs`): song + image keep original filenames (spaces/Vietnamese diacritics OK), but lyrics files are always saved with a `Guid_` prefix to avoid collisions. Extensions are validated server-side: audio `.mp3`, image `.jpg/.jpeg/.png/.webp/.gif`, lyrics `.lrc/.txt`, with size caps (100MB / 5MB / 1MB). Files are saved to the media root via `MediaStorage.SaveFile("music|images|lyrics", ...)`.
- Player page `OnGetPlay` streams the audio from the media root (range requests) and records a `Play` row (drives weekly trending via `Services/MusicService.cs`).

## Conventions

- All user-facing strings, validation messages, and code comments are Vietnamese - keep new ones in Vietnamese.
- Page models use primary-constructor DI, e.g. `class X(AppDbContext context) : PageModel`.
- JSON API responses are camelCase because `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase` (`Program.cs:26-29`). E.g. `/api/Search/{query}` returns `title`, `artist`, `filePath`, `found`, `playLink`.
