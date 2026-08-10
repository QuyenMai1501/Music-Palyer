# MusicPlayer

ASP.NET Core 8.0 Razor Pages app (Vietnamese UI) + one MVC API controller. EF Core 8 + Pomelo MySQL, TagLibSharp for MP3 duration. No test project.

## Run / verify

- `dotnet run` (profile `http` -> http://localhost:5122); `dotnet build` to verify.
- Requires local MySQL: create database `sql_music` first. The connection string is NOT committed — it is read from user-secrets (Development) or env var `ConnectionStrings__MySQLConnection`. To set locally: `dotnet user-secrets set "ConnectionStrings:MySQLConnection" "server=localhost;port=3306;database=sql_music;user=root;password=<pass>"`.
- Migrations are NOT auto-applied at startup. After schema changes: `dotnet ef migrations add <Name>` then `dotnet ef database update`.
- Note: installed `dotnet-ef` global tool is v9.0.4 vs EF packages 8.0.3; commands still work.

## Auth (important - NOT ASP.NET Identity)

- Passwords are hashed with PBKDF2 via `Services/PasswordService.cs` (wraps ASP.NET `PasswordHasher<User>`). Legacy SHA-256 hex hashes are detected and rehashed automatically on next successful login. Use `PasswordService` (not inline crypto) for all hashing.
- Login is rate-limited via session: 5 failed attempts locks the session for 15 minutes (`Helpers/SessionExtensions.cs`: `IsLoginLocked`, `RecordLoginFailure`, `ClearLoginFailures`).
- Authenticated state lives in `ISession` (`UserId`, `UserRole`, `Username`, `Useremail`) via `Helpers/SessionExtensions.cs`. The `AddAuthentication(Cookie...)` registration in `Program.cs` is effectively unused.
- Every handler guards admin pages with `HttpContext.Session.GetUserRole() == "Admin"`. Redirect target varies: `/Error`, `/Account/Login`, or `/Index` depending on page.
- No seed data: register always creates `Role="User"`. To create the first Admin: register a normal user, then promote directly in MySQL (`UPDATE Users SET Role='Admin' WHERE Username='...'`) or via `/Admin/ManageUser` once an Admin exists. Do not hand-insert `PasswordHash` (must be a PBKDF2 string, not SHA-256 hex).

## Media storage & URLs

- `Program.cs:54-66` maps: `wwwroot/music/*`->`/music/*` (ServeUnknownFileTypes, `audio/mpeg`), `wwwroot/lyrics/*`->`/lyrics/*`. `wwwroot/images/*` is the default static folder.
- `Song.FilePath`/`LyricsPath`/`ImagePath` store URL paths starting with `/music/`, `/lyrics/`, `/images/`.
- Admin uploads (`Pages/Admin/AddSong.cshtml.cs`, `Pages/Admin/EditSong.cshtml.cs`): song + image keep original filenames (spaces/Vietnamese diacritics OK), but lyrics files are always saved with a `Guid_` prefix to avoid collisions. Extensions are validated server-side: audio `.mp3`, image `.jpg/.jpeg/.png/.webp/.gif`, lyrics `.lrc/.txt`, with size caps (100MB / 5MB / 1MB).
- Player page `OnGetPlay` serves the audio file from disk and records a `Play` row (drives weekly trending via `Services/MusicService.cs`).

## Conventions

- All user-facing strings, validation messages, and code comments are Vietnamese - keep new ones in Vietnamese.
- Page models use primary-constructor DI, e.g. `class X(AppDbContext context) : PageModel`.
- JSON API responses (e.g. `/api/Search/{query}`) keep PascalCase keys because `PropertyNamingPolicy = null` (`Program.cs:25-28`).
