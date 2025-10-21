using System;

namespace code.ViewModels;

public static class Lab4Messages
{
    public const string KeyPathNotSpecified = "Шлях до файлу ключа не вказано.";
    public const string InputFileRequired = "Виберіть вхідний файл.";
    public const string OutputFileRequired = "Вкажіть вихідний файл.";
    public const string PublicKeyRequired = "Виберіть публічний ключ.";
    public const string PrivateKeyRequired = "Виберіть приватний ключ.";
    public const string InputFileMissing = "Вхідний файл не існує.";
    public const string PrivateKeyMissing = "Приватний ключ не знайдено.";
    
    public static string GetKeysGeneratedMessage(string publicKeyPath, string privateKeyPath) =>
        $"Ключі успішно згенеровано:\n• Публічний: {publicKeyPath}\n• Приватний: {privateKeyPath}";
    
    public static string GetEncryptionSuccessMessage(string outputPath, TimeSpan duration) =>
        $"Файл успішно зашифровано за {FormatDuration(duration)}:\n{outputPath}";

    public static string GetDecryptionSuccessMessage(string outputPath, TimeSpan duration) =>
        $"Файл успішно розшифровано за {FormatDuration(duration)}:\n{outputPath}";

    public static string FilePickerError(string message) =>
        $"Помилка вибору файлу: {message}";

    public static string PublicKeyNotFound(string path) =>
        $"Публічний ключ не знайдено: {path}";

    public static string PrivateKeyNotFound(string path) =>
        $"Приватний ключ не знайдено: {path}";

    public static string EncryptionInProgress(long length) =>
        $"Шифрування файлу ({length} байт)...";

    public static string CryptographyError(string message) =>
        $"Помилка криптографії: {message}\n\nПереконайтеся, що ключ згенеровано правильно.";

    public static string UnauthorizedAccess(string message) =>
        $"Немає доступу до файлу: {message}";

    public static string IoError(string message) =>
        $"Помилка введення/виведення: {message}";

    public static string EncryptionError(Exception ex) =>
        $"Помилка шифрування: {ex.GetType().Name}\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}";

    public static string KeyGenerationError(string message) =>
        $"Помилка генерації ключів: {message}";

    public static string DecryptionError(string message) =>
        $"Помилка дешифрування: {message}";

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalSeconds >= 1
            ? $"{duration.TotalSeconds:F2} с"
            : $"{duration.TotalMilliseconds:F0} мс";
}