using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MusicPlayer.Services
{
    // Lưu trữ media (nhạc, ảnh, lời) ngoài wwwroot để không bị mất khi deploy.
    // Gốc được cấu hình qua "Media:RootPath" (appsettings / user-secrets / env Media__RootPath);
    // mặc định là <ContentRoot>/media.
    public class MediaStorage
    {
        private readonly string _rootPath;

        public MediaStorage(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _rootPath = ResolveRootPath(configuration, environment);
            EnsureDirectories(_rootPath);
        }

        public string RootPath => _rootPath;

        public static string ResolveRootPath(IConfiguration configuration, IWebHostEnvironment environment)
        {
            string configured = configuration["Media:RootPath"]?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(configured))
            {
                return Path.Combine(environment.ContentRootPath, "media");
            }
            return Path.GetFullPath(Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(environment.ContentRootPath, configured));
        }

        public static void EnsureDirectories(string rootPath)
        {
            Directory.CreateDirectory(Path.Combine(rootPath, "music"));
            Directory.CreateDirectory(Path.Combine(rootPath, "images"));
            Directory.CreateDirectory(Path.Combine(rootPath, "lyrics"));
        }

        // Map virtual path (/music/ten.mp3) sang đường dẫn vật lý trong media root. Chặn path traversal.
        public string GetPhysicalPath(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return string.Empty;
            }
            string root = Path.GetFullPath(_rootPath);
            string full = Path.GetFullPath(Path.Combine(root, virtualPath.TrimStart('/', '\\')));
            if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
            return full;
        }

        public bool Exists(string virtualPath)
        {
            string path = GetPhysicalPath(virtualPath);
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }

        // Lưu file vào thư mục con (music/images/lyrics), trả về virtual path /<subdir>/<fileName>.
        public string SaveFile(string subdir, IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File rỗng.", nameof(file));
            }
            string folder = Path.Combine(_rootPath, subdir);
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return $"/{subdir}/{fileName}";
        }

        public string ReadAllText(string virtualPath)
        {
            string path = GetPhysicalPath(virtualPath);
            return string.IsNullOrEmpty(path) ? string.Empty : File.ReadAllText(path);
        }

        // Lưu nội dung văn bản (UTF-8 không BOM) vào thư mục con, ghi đè nếu đã tồn tại,
        // trả về virtual path /<subdir>/<fileName>.
        public string SaveText(string subdir, string fileName, string content)
        {
            string folder = Path.Combine(_rootPath, subdir);
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, fileName);
            File.WriteAllText(fullPath, content, new UTF8Encoding(false));
            return $"/{subdir}/{fileName}";
        }
    }
}