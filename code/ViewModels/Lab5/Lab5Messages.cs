namespace code.ViewModels;

public static class Lab5Messages
{
    public const string SelectFileTitle = "Виберіть файл";
    public const string SelectPublicKeyTitle = "Виберіть публічний ключ";
    public const string SelectPrivateKeyTitle = "Виберіть приватний ключ";
    public const string SelectSignatureFileTitle = "Виберіть файл підпису";
    public const string SaveSignatureTitle = "Зберегти підпис";
    public const string SavePublicKeyTitle = "Зберегти публічний ключ";
    public const string SavePrivateKeyTitle = "Зберегти приватний ключ";
    
    public const string GeneratingKeys = "Генерація ключів...";
    public const string KeysCancelled = "Генерацію скасовано.";
    public const string KeysGenerated = "Ключі успішно згенеровано та збережено!";
    public const string KeysGenerationError = "Помилка генерації ключів: ";
    
    public const string PublicKeyLoaded = "Публічний ключ завантажено: ";
    public const string PrivateKeyLoaded = "Приватний ключ завантажено: ";
    public const string FileSelected = "Файл вибрано: ";
    public const string SignatureFileSelected = "Файл підпису вибрано: ";
    
    public const string CreatingSignature = "Створення підпису...";
    public const string CreatingFileSignature = "Створення підпису файлу...";
    public const string SignatureCreated = "Підпис успішно створено!";
    public const string SignatureError = "Помилка створення підпису: ";
    public const string SignatureSaved = "Підпис збережено: ";
    public const string NoSignatureToSave = "Немає підпису для збереження.";
    
    public const string VerifyingSignature = "Перевірка підпису...";
    public const string SignatureValid = "Підпис дійсний! Файл не змінювався.";
    public const string SignatureInvalid = "Підпис недійсний! Файл було змінено або підпис не відповідає.";
    public const string VerificationError = "Помилка перевірки підпису: ";
    
    public const string NoPrivateKeySelected = "Будь ласка, виберіть приватний ключ.";
    public const string NoPublicKeySelected = "Будь ласка, виберіть публічний ключ.";
    public const string NoInputText = "Будь ласка, введіть текст для підпису.";
    public const string NoFileSelected = "Будь ласка, виберіть файл.";
    public const string NoSignatureSelected = "Будь ласка, виберіть файл підпису.";
    public const string NoInputTextOrFile = "Введіть текст або виберіть файл для перевірки";
    
    public const string DataCleared = "Дані очищено.";
    public const string FileReadError = "Помилка читання файлу: ";
    public const string FileSaveError = "Помилка збереження файлу: ";
    public const string Error = "Помилка: ";
}
