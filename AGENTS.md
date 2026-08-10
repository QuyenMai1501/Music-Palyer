# MusicPlayer

ASP.NET Core 8.0 Razor Pages app (Vietnamese UI) + one MVC API controller. EF Core 8 + Pomelo MySQL, TagLibSharp for MP3 duration. No test project.

## Run / verify

- `dotnet run` (profile `http` -> http://localhost:5122); `dotnet build` to verify.
- Requires local MySQL: create database `sql_music` first (host/port/creds hardcoded in `appsettings.json`: `root` / `Quyen_1501`).
- Migrations are NOT auto-applied at startup. After schema changes: `dotnet ef migrations add <Name>` then `dotnet ef database update`.
- Note: installed `dotnet-ef` global tool is v9.0.4 vs EF packages 8.0.3; commands still work.

## Auth (important - NOT ASP.NET Identity)

- Login/Register use a custom SHA-256 hex hash (`Pages/Account/Login.cshtml.cs:63`). Don't change the hashing without updating both pages.
- Authenticated state lives in `ISession` (`UserId`, `UserRole`, `Username`, `Useremail`) via `Helpers/SessionExtensions.cs`. The `AddAuthentication(Cookie...)` registration in `Program.cs` is effectively unused.
- Every handler guards admin pages with `HttpContext.Session.GetUserRole() == "Admin"`. Redirect target varies: `/Error`, `/Account/Login`, or `/Index` depending on page.
- No seed data: register always creates `Role="User"`. The first Admin must be inserted directly into the `Users` table (`Role='Admin'`, password = SHA-256 hex) or created by an existing admin via `/Admin/ManageUser`.

## Media storage & URLs

- `Program.cs:54-66` maps: `wwwroot/music/*`->`/music/*` (ServeUnknownFileTypes, `audio/mpeg`), `wwwroot/lyrics/*`->`/lyrics/*`. `wwwroot/images/*` is the default static folder.
- `Song.FilePath`/`LyricsPath`/`ImagePath` store URL paths starting with `/music/`, `/lyrics/`, `/images/`.
- Admin uploads (`Pages/Admin/AddSong.cshtml.cs`): song + image keep original filenames (spaces/Vietnamese diacritics OK), but lyrics files are always saved with a `Guid_` prefix to avoid collisions.
- Player page `OnGetPlay` serves the audio file from disk and records a `Play` row (drives weekly trending via `Services/MusicService.cs`).

## Conventions

- All user-facing strings, validation messages, and code comments are Vietnamese - keep new ones in Vietnamese.
- Page models use primary-constructor DI, e.g. `class X(AppDbContext context) : PageModel`.
- JSON API responses (e.g. `/api/Search/{query}`) keep PascalCase keys because `PropertyNamingPolicy = null` (`Program.cs:25-28`).
