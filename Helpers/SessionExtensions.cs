namespace MusicPlayer.Helpers
{
    public static class SessionExtensions{
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
    }
}