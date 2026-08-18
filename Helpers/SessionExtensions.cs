namespace MusicPlayer.Helpers
{
    using System.Globalization;

    public static class SessionExtensions{
        private const string LoginFailCountKey = "LoginFailCount";
        private const string LoginFailStartKey = "LoginFailStart";
        private const string PlayedSongsKey = "PlayedSongIds";
        public const int MaxLoginFails = 5;
        public static readonly TimeSpan LoginLockWindow = TimeSpan.FromMinutes(15);

        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32("UserId");
        }

        public static void SetUserId(this ISession session, int userId)
        {
            session.SetInt32("UserId", userId);
        }

        public static string GetUserRole(this ISession session)
        {
            return session.GetString("UserRole") ?? string.Empty;
        }

        public static void SetUserRole(this ISession session, string role)
        {
            session.SetString("UserRole", role);
        }

        public static void ClearUser(this ISession session)
        {
            session.Remove("UserId");
            session.Remove("UserRole");
            session.Remove("Useremail");
            session.Remove("Username");
        }

        public static bool IsLoginLocked(this ISession session)
        {
            int? failCount = session.GetInt32(LoginFailCountKey);
            DateTime? failStart = session.GetDateTime(LoginFailStartKey);
            if (failCount == null || failStart == null)
            {
                return false;
            }
            if (DateTime.UtcNow - failStart > LoginLockWindow)
            {
                session.ClearLoginFailures();
                return false;
            }
            return failCount >= MaxLoginFails;
        }

        public static void RecordLoginFailure(this ISession session)
        {
            if (session.GetDateTime(LoginFailStartKey) == null)
            {
                session.SetDateTime(LoginFailStartKey, DateTime.UtcNow);
            }
            session.SetInt32(LoginFailCountKey, (session.GetInt32(LoginFailCountKey) ?? 0) + 1);
        }

        public static void ClearLoginFailures(this ISession session)
        {
            session.Remove(LoginFailCountKey);
            session.Remove(LoginFailStartKey);
        }

        public static bool HasPlayedSong(this ISession session, int songId)
        {
            string? raw = session.GetString(PlayedSongsKey);
            if (string.IsNullOrEmpty(raw))
            {
                return false;
            }
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(songId.ToString());
        }

        public static void MarkSongPlayed(this ISession session, int songId)
        {
            string? raw = session.GetString(PlayedSongsKey);
            var ids = string.IsNullOrEmpty(raw)
                ? new List<string>()
                : raw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!ids.Contains(songId.ToString()))
            {
                ids.Add(songId.ToString());
                session.SetString(PlayedSongsKey, string.Join(",", ids));
            }
        }

        private static DateTime? GetDateTime(this ISession session, string key)
        {
            string? value = session.GetString(key);
            return string.IsNullOrEmpty(value) ? null : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        private static void SetDateTime(this ISession session, string key, DateTime value)
        {
            session.SetString(key, value.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}