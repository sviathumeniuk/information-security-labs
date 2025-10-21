using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using code.Models;

namespace code.ViewModels;

public partial class Lab4ViewModel : ViewModelBase
{
    private readonly IRsaCipher _rsaCipher;
    private readonly INavigator _navigator;

    [ObservableProperty] private string? _inputFilePath;
    [ObservableProperty] private string? _outputFilePath;
    [ObservableProperty] private string? _keyFilePath;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isGenerateMode = true;
    [ObservableProperty] private bool _isEncryptMode = false;
    [ObservableProperty] private bool _isDecryptMode = false;
    [ObservableProperty] private int _keySizeInBits = 2048;

    public Lab4ViewModel(IRsaCipher rsaCipher, INavigator navigator)
    {
        _rsaCipher = rsaCipher;
        _navigator = navigator;
    }

    private async Task<string?> SelectFileAsync(string title, FilePickerFileType[]? fileTypes = null)
    {
        try
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return null;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            Message = Lab4Messages.FilePickerError(ex.Message);
            return null;
        }
    }

    private async Task<string?> SaveFileAsync(string title, string? suggestedFileName = null)
    {
        try
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return null;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedFileName
            });

            return file?.Path.LocalPath;
        }
        catch (Exception ex)
        {
            Message = Lab4Messages.FilePickerError(ex.Message);
            return null;
        }
    }

    [RelayCommand]
    private async Task SelectKeyLocationAsync()
    {
        var path = await SaveFileAsync("Виберіть розташування для ключів");
        if (path != null)
        {
            KeyFilePath = ExtractDirectory(path);
        }
    }

    [RelayCommand]
    private async Task SelectPublicKeyAsync()
    {
        var fileTypes = new FilePickerFileType[]
        {
            new("Публічний ключ") { Patterns = new[] { "public_rsa_key" } },
            new("Всі файли") { Patterns = new[] { "*.*" } }
        };
        var path = await SelectFileAsync("Виберіть публічний ключ", fileTypes);
        if (path != null)
        {
            KeyFilePath = ExtractDirectory(path);
        }
    }

    [RelayCommand]
    private async Task SelectPrivateKeyAsync()
    {
        var fileTypes = new FilePickerFileType[]
        {
            new("Приватний ключ") { Patterns = new[] { "private_rsa_key" } },
            new("Всі файли") { Patterns = new[] { "*.*" } }
        };
        var path = await SelectFileAsync("Виберіть приватний ключ", fileTypes);
        if (path != null)
        {
            KeyFilePath = ExtractDirectory(path);
        }
    }

    [RelayCommand]
    private async Task SelectInputFileAsync()
    {
        var path = await SelectFileAsync("Виберіть файл для обробки");
        if (path != null)
        {
            InputFilePath = path;
        }
    }

    [RelayCommand]
    private async Task SelectOutputFileAsync()
    {
        var path = await SaveFileAsync("Виберіть розташування для збереження результату");
        if (path != null)
        {
            OutputFilePath = path;
        }
    }

    [RelayCommand]
    private async Task GenerateKeysAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(KeyFilePath))
            {
                Message = Lab4Messages.KeyPathNotSpecified;
                return;
            }

            var directory = NormalizeKeyDirectory(KeyFilePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                Message = Lab4Messages.KeyPathNotSpecified;
                return;
            }

            Directory.CreateDirectory(directory);

            var publicKeyPath = GetPublicKeyPath(directory);
            var privateKeyPath = GetPrivateKeyPath(directory);

            _rsaCipher.GenerateKeys(KeySizeInBits);
            _rsaCipher.SavePublicKey(publicKeyPath);
            _rsaCipher.SavePrivateKey(privateKeyPath);

            KeyFilePath = directory;

            Message = Lab4Messages.GetKeysGeneratedMessage(publicKeyPath, privateKeyPath);
        }
        catch (Exception ex)
        {
            Message = Lab4Messages.KeyGenerationError(ex.Message);
        }
    }

    [RelayCommand]
    private async Task EncryptFileAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputFilePath))
            {
                Message = Lab4Messages.InputFileRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputFilePath))
            {
                Message = Lab4Messages.OutputFileRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(KeyFilePath))
            {
                Message = Lab4Messages.PublicKeyRequired;
                return;
            }

            if (!File.Exists(InputFilePath))
            {
                Message = Lab4Messages.InputFileMissing;
                return;
            }

            var publicKeyPath = GetPublicKeyPath(KeyFilePath);
            if (string.IsNullOrWhiteSpace(publicKeyPath))
            {
                Message = Lab4Messages.PublicKeyRequired;
                return;
            }

            if (!File.Exists(publicKeyPath))
            {
                Message = Lab4Messages.PublicKeyNotFound(publicKeyPath);
                return;
            }

            var fileInfo = new FileInfo(InputFilePath);
            Message = Lab4Messages.EncryptionInProgress(fileInfo.Length);

            var stopwatch = Stopwatch.StartNew();

            _rsaCipher.LoadPublicKey(publicKeyPath);
            _rsaCipher.EncryptFile(InputFilePath, OutputFilePath);

            stopwatch.Stop();

            Message = Lab4Messages.GetEncryptionSuccessMessage(OutputFilePath, stopwatch.Elapsed);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            Message = Lab4Messages.CryptographyError(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Message = Lab4Messages.UnauthorizedAccess(ex.Message);
        }
        catch (IOException ex)
        {
            Message = Lab4Messages.IoError(ex.Message);
        }
        catch (Exception ex)
        {
            Message = Lab4Messages.EncryptionError(ex);
        }
    }

    [RelayCommand]
    private async Task DecryptFileAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputFilePath))
            {
                Message = Lab4Messages.InputFileRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputFilePath))
            {
                Message = Lab4Messages.OutputFileRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(KeyFilePath))
            {
                Message = Lab4Messages.PrivateKeyRequired;
                return;
            }

            if (!File.Exists(InputFilePath))
            {
                Message = Lab4Messages.InputFileMissing;
                return;
            }

            var privateKeyPath = GetPrivateKeyPath(KeyFilePath);
            if (string.IsNullOrWhiteSpace(privateKeyPath))
            {
                Message = Lab4Messages.PrivateKeyRequired;
                return;
            }

            if (!File.Exists(privateKeyPath))
            {
                Message = Lab4Messages.PrivateKeyNotFound(privateKeyPath);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            _rsaCipher.LoadPrivateKey(privateKeyPath);
            _rsaCipher.DecryptFile(InputFilePath, OutputFilePath);

            stopwatch.Stop();

            Message = Lab4Messages.GetDecryptionSuccessMessage(OutputFilePath, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            Message = Lab4Messages.DecryptionError(ex.Message);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigator.NavigateToMenu();
    }

    private static string? ExtractDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        var normalized = Path.TrimEndingDirectorySeparator(path);
        var directory = Path.GetDirectoryName(normalized);
        return string.IsNullOrEmpty(directory) ? normalized : directory;
    }

    private static string NormalizeKeyDirectory(string? basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath)) return string.Empty;
        return Path.TrimEndingDirectorySeparator(basePath);
    }

    private static string GetPublicKeyPath(string? basePath)
    {
        var directory = NormalizeKeyDirectory(basePath);
        return string.IsNullOrWhiteSpace(directory)
            ? string.Empty
            : Path.Combine(directory, "public_rsa_key");
    }

    private static string GetPrivateKeyPath(string? basePath)
    {
        var directory = NormalizeKeyDirectory(basePath);
        return string.IsNullOrWhiteSpace(directory)
            ? string.Empty
            : Path.Combine(directory, "private_rsa_key");
    }
}