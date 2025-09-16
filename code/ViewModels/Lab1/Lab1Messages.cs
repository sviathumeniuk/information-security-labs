namespace code.ViewModels;

public static class Lab1Messages
{
    public const string EmptyCount = "Кількість чисел не вказана";
    public const string NotInteger = "Введіть ціле число";
    public const string NotPositive = "Кількість чисел має бути додатньою";

    public const string InfinitePeriod = "Період більший за модуль або послідовність нециклічна";
    public static string GetPeriodMessage(int period) =>
        $"Період послідовності: {period}";

    public const string NotEnoughNumbers = "Потрібно згенерувати принаймні 2 числа для тестування";
    public static string GetPiEstimationMessage(double pi, double error) =>
        $"Оцінка π: {pi:F6} (похибка: {error:E6})";

    public static string FileSavedMessage(string path) =>
        $"Успішно згенеровано та збережено у файл";
}