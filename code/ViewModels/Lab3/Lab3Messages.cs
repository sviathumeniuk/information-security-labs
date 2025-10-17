namespace code.ViewModels;

public static class Lab3Messages
{

    public const string EmptyPassword = "Введіть парольну фразу!";
    public const string EmptyInputFile = "Виберіть коректний вхідний файл!";

    public const string Encrypting = "Шифрування.";
    public const string Decrypting = "Дешифрування.";
    
    public const string FileTooSmall = "Файл занадто малий для дешифрування";
    
    public const string SelectFileTitle = "Виберіть файл для обробки";
    
    public const string EncryptedExtension = ".rc5";
    public const string DecryptedExtension = ".decrypted";
    
    public static string ErrorSelectingFile(string error) => $"Помилка при виборі файлу: {error}";
    public static string ErrorProcessing(string error) => $"Помилка: {error}";
    public static string SuccessEncryption(string fileName) => $"Файл успішно зашифровано! Збережено: Results/{fileName}";
    public static string SuccessDecryption(string fileName) => $"Файл успішно дешифровано! Збережено: Results/{fileName}";
}
