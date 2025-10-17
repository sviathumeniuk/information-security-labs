using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using code.Models;

namespace code.ViewModels;

public partial class Lab3ViewModel : ViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IMD5Hasher _md5Hasher;
    private readonly IRandomGenerator _randomGenerator;

    private const int WordSize = 16;
    private const int Rounds = 20;

    [ObservableProperty] private string? _password = null;
    [ObservableProperty] private string? _inputFilePath = null;
    [ObservableProperty] private string? _statusMessage = null;
    [ObservableProperty] private bool _isEncryptMode = true;
    [ObservableProperty] private bool _isProcessing = false;

    private const string RESULTS_FOLDER = "Results";

    public Lab3ViewModel(INavigator navigator, IMD5Hasher md5Hasher, IRandomGenerator randomGenerator)
    {
        _navigator = navigator;
        _md5Hasher = md5Hasher;
        _randomGenerator = randomGenerator;
    }

    [RelayCommand]
    private async Task SelectInputFileAsync()
    {
        try
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is 
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Lab3Messages.SelectFileTitle,
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                InputFilePath = files[0].Path.LocalPath;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = Lab3Messages.ErrorSelectingFile(ex.Message);
        }
    }

    [RelayCommand]
    private async Task ProcessFileAsync()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = Lab3Messages.EmptyPassword;
            return;
        }

        if (string.IsNullOrWhiteSpace(InputFilePath) || !File.Exists(InputFilePath))
        {
            StatusMessage = Lab3Messages.EmptyInputFile;
            return;
        }

        IsProcessing = true;
        StatusMessage = IsEncryptMode ? Lab3Messages.Encrypting : Lab3Messages.Decrypting;

        try
        {
            if (!Directory.Exists(RESULTS_FOLDER))
            {
                Directory.CreateDirectory(RESULTS_FOLDER);
            }

            string inputFileName = Path.GetFileName(InputFilePath);
            string outputFileName;
            
            if (IsEncryptMode)
            {
                outputFileName = inputFileName + Lab3Messages.EncryptedExtension;
            }
            else
            {
                if (inputFileName.EndsWith(Lab3Messages.EncryptedExtension))
                {
                    outputFileName = inputFileName.Substring(0, inputFileName.Length - Lab3Messages.EncryptedExtension.Length);
                }
                else
                {
                    outputFileName = inputFileName + Lab3Messages.DecryptedExtension;
                }
            }

            string outputPath = Path.Combine(RESULTS_FOLDER, outputFileName);

            await Task.Run(() =>
            {
                if (IsEncryptMode)
                {
                    EncryptFile(InputFilePath, outputPath, Password);
                }
                else
                {
                    DecryptFile(InputFilePath, outputPath, Password);
                }
            });

            StatusMessage = IsEncryptMode 
                ? Lab3Messages.SuccessEncryption(outputFileName)
                : Lab3Messages.SuccessDecryption(outputFileName);
        }
        catch (Exception ex)
        {
            StatusMessage = Lab3Messages.ErrorProcessing(ex.Message);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void SwitchMode()
    {
        IsEncryptMode = !IsEncryptMode;
        StatusMessage = string.Empty;
    }

    private byte[] GenerateKey(string password)
    {
        byte[] hash = _md5Hasher.ComputeHashBytes(password);
        return hash;
    }

    private static RC5Cipher CreateCipher(byte[] key)
    {
        return new RC5Cipher(key, Rounds, WordSize);
    }

    private byte[] GenerateIV()
    {
        var seed = Environment.TickCount;
        var random = _randomGenerator.Generate(4, seed, 1103515245, 12345, 2147483647).ToArray();
        
        byte[] iv = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            iv[i] = (byte)(random[i] & 0xFF);
        }
        
        return iv;
    }

    private void EncryptFile(string inputPath, string outputPath, string password)
    {
        byte[] key = GenerateKey(password);
        var rc5 = CreateCipher(key);
        byte[] iv = GenerateIV();
        
        byte[] plainData = File.ReadAllBytes(inputPath);
        
        byte[] encryptedIV = new byte[4];
        rc5.EncryptBlock(iv, encryptedIV);
        
        byte[] encryptedData = rc5.EncryptCBC(plainData, iv);
        
        using (var fs = new FileStream(outputPath, FileMode.Create))
        {
            fs.Write(encryptedIV, 0, encryptedIV.Length);
            fs.Write(encryptedData, 0, encryptedData.Length);
        }
    }

    private void DecryptFile(string inputPath, string outputPath, string password)
    {
        byte[] key = GenerateKey(password);
        var rc5 = CreateCipher(key);
        byte[] encryptedFile = File.ReadAllBytes(inputPath);
        
        if (encryptedFile.Length < 4)
        {
            throw new InvalidDataException(Lab3Messages.FileTooSmall);
        }
        
        byte[] encryptedIV = new byte[4];
        Array.Copy(encryptedFile, 0, encryptedIV, 0, 4);
        
        byte[] iv = new byte[4];
        rc5.DecryptBlock(encryptedIV, iv);
        
        byte[] encryptedData = new byte[encryptedFile.Length - 4];
        Array.Copy(encryptedFile, 4, encryptedData, 0, encryptedData.Length);
        
        byte[] plainData = rc5.DecryptCBC(encryptedData, iv);
        
        File.WriteAllBytes(outputPath, plainData);
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigator.NavigateToMenu();
    }
}