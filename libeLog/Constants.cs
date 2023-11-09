namespace libeLog;

public static class Constants
{
    /// <summary>
    /// Максимальный размер файла логов.
    /// </summary>
    public const long MaxLogSize = 8388608;

    public const string DateTimeFormat = "dd.MM.yyyy HH:mm";
    public const string DateTimeWithSecsFormat = "dd.MM.yyyy HH:mm:ss";
    public const string TimeSpanFormat = @"hh\:mm\:ss";

    public class StatusTips
    {
        public const string Ok = "Всё в порядке.";
        public const string AccessError = "Ошибка при проверке доступа.";

        public const string NoFile = "Файл не существует.";

        public const string NoAccessToDirectory = "Директория не существует или отсутствуют права на её чтение.";
        public const string NoWriteAccess = "Нет доступа на запись. Работа в режиме чтения.";
        
    }
}
