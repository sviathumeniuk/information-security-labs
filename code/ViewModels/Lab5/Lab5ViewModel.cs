using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using code.Models;

namespace code.ViewModels;

public partial class Lab5ViewModel : ViewModelBase
{
    private const string XmlFilePattern = "*.xml";
    private const string XmlFileTypeName = "XML файл";
    private const string SignatureFilePattern = "*.sig";
    private const string SignatureFileTypeName = "Signature файл";
    
    private readonly IDigitalSignature _digitalSignature;
    private readonly INavigator _navigator;

    [ObservableProperty] private string? _inputText;
    [ObservableProperty] private string? _verifyText;
    [ObservableProperty] private string? _inputFilePath;
    [ObservableProperty] private string? _publicKeyPath;
    [ObservableProperty] private string? _privateKeyPath;
    [ObservableProperty] private string? _signatureFilePath;
    [ObservableProperty] private string? _outputSignature;
    [ObservableProperty] private string? _verificationResult;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isTextMode = true;
    [ObservableProperty] private bool _isFileMode = false;
    [ObservableProperty] private bool _isVerifyMode = false;

    private byte[]? _publicKey;
    private byte[]? _privateKey;

    public Lab5ViewModel(IDigitalSignature digitalSignature, INavigator navigator)
    {
        _digitalSignature = digitalSignature;
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
            Message = $"{Lab5Messages.FileReadError}{ex.Message}";
            return null;
        }
    }

    private async Task<string?> SaveFileAsync(string title, string? suggestedFileName = null, FilePickerFileType[]? fileTypes = null)
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
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = fileTypes
            });

            return file?.Path.LocalPath;
        }
        catch (Exception ex)
        {
            Message = $"{Lab5Messages.FileSaveError}{ex.Message}";
            return null;
        }
    }

    [RelayCommand]
    private async Task GenerateKeysAsync()
    {
        try
        {
            Message = Lab5Messages.GeneratingKeys;

            var (publicKey, privateKey) = _digitalSignature.GenerateKeys();
            _publicKey = publicKey;
            _privateKey = privateKey;

            var publicKeyPath = await SaveFileAsync(
                Lab5Messages.SavePublicKeyTitle,
                "public_key.xml",
                new[] { new FilePickerFileType(XmlFileTypeName) { Patterns = new[] { XmlFilePattern } } }
            );

            if (string.IsNullOrEmpty(publicKeyPath))
            {
                Message = Lab5Messages.KeysCancelled;
                return;
            }

            var privateKeyPath = await SaveFileAsync(
                Lab5Messages.SavePrivateKeyTitle,
                "private_key.xml",
                new[] { new FilePickerFileType(XmlFileTypeName) { Patterns = new[] { XmlFilePattern } } }
            );

            if (string.IsNullOrEmpty(privateKeyPath))
            {
                Message = Lab5Messages.KeysCancelled;
                return;
            }

            var publicKeyXml = _digitalSignature.ExportPublicKey(publicKey);
            var privateKeyXml = _digitalSignature.ExportPrivateKey(privateKey);

            await File.WriteAllTextAsync(publicKeyPath, publicKeyXml);
            await File.WriteAllTextAsync(privateKeyPath, privateKeyXml);

            PublicKeyPath = publicKeyPath;
            PrivateKeyPath = privateKeyPath;

            Message = $"{Lab5Messages.KeysGenerated}\n{Lab5Messages.PublicKeyLoaded}{publicKeyPath}\n{Lab5Messages.PrivateKeyLoaded}{privateKeyPath}";
        }
        catch (Exception ex)
        {
            Message = $"{Lab5Messages.KeysGenerationError}{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SelectPublicKeyAsync()
    {
        var path = await SelectFileAsync(
            Lab5Messages.SelectPublicKeyTitle,
            [new FilePickerFileType(XmlFileTypeName) { Patterns = [XmlFilePattern] }]
        );

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                var keyXml = await File.ReadAllTextAsync(path);
                _publicKey = _digitalSignature.ImportPublicKey(keyXml);
                PublicKeyPath = path;
                Message = $"{Lab5Messages.PublicKeyLoaded}{path}";
            }
            catch (Exception ex)
            {
                Message = $"{Lab5Messages.FileReadError}{ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task SelectPrivateKeyAsync()
    {
        var path = await SelectFileAsync(
            Lab5Messages.SelectPrivateKeyTitle,
            new[] { new FilePickerFileType(XmlFileTypeName) { Patterns = new[] { XmlFilePattern } } }
        );

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                var keyXml = await File.ReadAllTextAsync(path);
                _privateKey = _digitalSignature.ImportPrivateKey(keyXml);
                PrivateKeyPath = path;
                Message = $"{Lab5Messages.PrivateKeyLoaded}{path}";
            }
            catch (Exception ex)
            {
                Message = $"{Lab5Messages.FileReadError}{ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task SelectInputFileAsync()
    {
        var path = await SelectFileAsync(Lab5Messages.SelectFileTitle);
        if (!string.IsNullOrEmpty(path))
        {
            InputFilePath = path;
            Message = $"{Lab5Messages.FileSelected}{path}";
        }
    }

    [RelayCommand]
    private async Task SelectSignatureFileAsync()
    {
        var path = await SelectFileAsync(
            Lab5Messages.SelectSignatureFileTitle,
            new[] { new FilePickerFileType(SignatureFileTypeName) { Patterns = new[] { SignatureFilePattern, "*.txt" } } }
        );

        if (!string.IsNullOrEmpty(path))
        {
            SignatureFilePath = path;
            Message = $"{Lab5Messages.SignatureFileSelected}{path}";
        }
    }

    [RelayCommand]
    private async Task SignTextAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                Message = Lab5Messages.NoInputText;
                return;
            }

            if (_privateKey == null)
            {
                Message = Lab5Messages.NoPrivateKeySelected;
                return;
            }

            Message = Lab5Messages.CreatingSignature;

            var data = Encoding.UTF8.GetBytes(InputText);
            var signature = _digitalSignature.SignData(data, _privateKey);
            var signatureHex = BitConverter.ToString(signature).Replace("-", "");

            OutputSignature = signatureHex;
            Message = Lab5Messages.SignatureCreated;
        }
        catch (Exception ex)
        {
            Message = $"{Lab5Messages.SignatureError}{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SignFileAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(InputFilePath))
            {
                Message = Lab5Messages.NoFileSelected;
                return;
            }

            if (_privateKey == null)
            {
                Message = Lab5Messages.NoPrivateKeySelected;
                return;
            }

            Message = Lab5Messages.CreatingFileSignature;

            var data = await File.ReadAllBytesAsync(InputFilePath);
            var signature = _digitalSignature.SignData(data, _privateKey);
            var signatureHex = BitConverter.ToString(signature).Replace("-", "");

            OutputSignature = signatureHex;

            var signaturePath = await SaveFileAsync(
                Lab5Messages.SaveSignatureTitle,
                Path.GetFileName(InputFilePath) + ".sig",
                new[] { new FilePickerFileType(SignatureFileTypeName) { Patterns = new[] { SignatureFilePattern } } }
            );

            if (!string.IsNullOrEmpty(signaturePath))
            {
                await File.WriteAllTextAsync(signaturePath, signatureHex);
                Message = $"{Lab5Messages.SignatureCreated}\n{Lab5Messages.SignatureSaved}{signaturePath}";
            }
            else
            {
                Message = Lab5Messages.SignatureCreated;
            }
        }
        catch (Exception ex)
        {
            Message = $"{Lab5Messages.SignatureError}{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSignatureAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(OutputSignature))
            {
                Message = Lab5Messages.NoSignatureToSave;
                return;
            }

            var path = await SaveFileAsync(
                Lab5Messages.SaveSignatureTitle,
                "signature.sig",
                new[] { new FilePickerFileType(SignatureFileTypeName) { Patterns = new[] { SignatureFilePattern, "*.txt" } } }
            );

            if (!string.IsNullOrEmpty(path))
            {
                await File.WriteAllTextAsync(path, OutputSignature);
                Message = $"{Lab5Messages.SignatureSaved}{path}";
            }
        }
        catch (Exception ex)
        {
            Message = $"{Lab5Messages.FileSaveError}{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VerifySignatureAsync()
    {
        try
        {
            var hasText = !string.IsNullOrEmpty(VerifyText);
            var hasFile = !string.IsNullOrEmpty(InputFilePath);

            if (!hasText && !hasFile)
            {
                Message = Lab5Messages.NoInputTextOrFile;
                return;
            }

            if (string.IsNullOrEmpty(SignatureFilePath))
            {
                Message = Lab5Messages.NoSignatureSelected;
                return;
            }

            if (_publicKey == null)
            {
                Message = Lab5Messages.NoPublicKeySelected;
                return;
            }

            Message = Lab5Messages.VerifyingSignature;

            byte[] data;
            if (hasText)
            {
                data = Encoding.UTF8.GetBytes(VerifyText!);
            }
            else
            {
                data = await File.ReadAllBytesAsync(InputFilePath!);
            }

            var signatureHex = await File.ReadAllTextAsync(SignatureFilePath);
            
            signatureHex = signatureHex.Replace("-", "").Replace(" ", "").Replace("\r", "").Replace("\n", "");
            var signature = new byte[signatureHex.Length / 2];
            for (int i = 0; i < signature.Length; i++)
            {
                signature[i] = Convert.ToByte(signatureHex.Substring(i * 2, 2), 16);
            }

            var isValid = _digitalSignature.VerifySignature(data, signature, _publicKey);

            VerificationResult = isValid ? Lab5Messages.SignatureValid : Lab5Messages.SignatureInvalid;
            Message = VerificationResult;
        }
        catch (Exception ex)
        {
            Message = $"{Lab5Messages.VerificationError}{ex.Message}";
            VerificationResult = $"{Lab5Messages.Error}{ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = string.Empty;
        VerifyText = string.Empty;
        InputFilePath = string.Empty;
        PublicKeyPath = string.Empty;
        PrivateKeyPath = string.Empty;
        SignatureFilePath = string.Empty;
        OutputSignature = string.Empty;
        VerificationResult = string.Empty;
        Message = Lab5Messages.DataCleared;
        _publicKey = null;
        _privateKey = null;
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigator.NavigateToMenu();
    }

}