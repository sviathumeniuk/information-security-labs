namespace code.ViewModels;

public static class Lab2Messages
{
    public const string EmptyInput = "Введіть текст для хешування";
    public const string InvalidFilePath = "Вкажіть коректний шлях до файлу";
    public const string EmptyExpectedHash = "Введіть очікуваний хеш для перевірки";
    
    // Повідомлення про результати операцій
    public const string HashComputed = "Хеш успішно обчислено";
    public const string FileIntact = "Перевірка успішна: цілісність файлу підтверджено";
    public const string FileCorrupted = "Перевірка не пройдена: файл був змінений або хеш неправильний";
    public const string NoHashToSave = "Немає обчисленого хешу для збереження";
    public const string TestSuiteCompleted = "Тести RFC 1321 завершено";
    
    public static string FileSavedMessage(string path) =>
        $"Хеш успішно збережено у файл: {path}";
    
    public static string FileHashMessage(string fileName) =>
        $"Обчислено хеш для файлу: {fileName}";
    
    public static string TestResult(bool allPassed) =>
        allPassed ? "Всі тести пройдено успішно" : "Деякі тести не пройдено";
}