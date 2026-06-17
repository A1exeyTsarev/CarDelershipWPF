using System.Windows.Controls;

namespace CarDelershipWPF.AppData
{
    internal class AppFrame
    {
        public static Frame FrameMain;
        public static int CurrentUserID;
        public static string CurrentUserName;
        public static string CurrentUserRole;
        public static Users CurrentUser;

        // Вспомогательные свойства для проверки ролей
        public static bool IsAdmin => CurrentUserRole == "Администратор";
        public static bool IsManager => CurrentUserRole == "Менеджер";
        public static bool IsAdminOrManager => IsAdmin || IsManager;
        public static bool IsAuthenticated => CurrentUser != null;

        public static void Logout()
        {
            CurrentUserID = 0;
            CurrentUserName = null;
            CurrentUserRole = null;
            CurrentUser = null;
        }
    }
}