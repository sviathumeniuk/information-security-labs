using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Chrome;
using code.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace code.ViewModels;

public partial class Lab2ViewModel : ViewModelBase
{
    private readonly IMD5Hasher _md5Hasher;
    private readonly INavigator _navigator;

    [ObservableProperty] private string? _inputText = null;
    [ObservableProperty] private string? _filePath = null;
    [ObservableProperty] private string? _computedHash = null;
    [ObservableProperty] private string? _expectedHash = null;
    [ObservableProperty] private string? _message = null;

    private const string RESULTS_FOLDER = "Results";
    private const string FILE_PATH = "Results/Hash.txt";

    public Lab2ViewModel(
        IMD5Hasher md5Hasher,
        INavigator navigator)
    {
        _md5Hasher = md5Hasher;
        _navigator = navigator;
    }

    [RelayCommand]
    private void ComputeHashForText()
    {
        ResetState();

        if (string.IsNullOrWhiteSpace(InputText))
        {
            Message = Lab2Messages.EmptyInput;
            return;
        }

        try
        {
            ComputedHash = _md5Hasher.ComputeHash(InputText);
            Message = Lab2Messages.HashComputed;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void ComputeHashForFile()
    {
        ResetState();

        if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
        {
            Message = Lab2Messages.InvalidFilePath;
            return;
        }

        try
        {
            ComputedHash = _md5Hasher.ComputeFileHash(FilePath);
            Message = Lab2Messages.HashComputed;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void VerifyFileIntegrity()
    {
        if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
        {
            Message = Lab2Messages.InvalidFilePath;
            return;
        }

        if (string.IsNullOrWhiteSpace(ExpectedHash))
        {
            Message = Lab2Messages.EmptyExpectedHash;
            return;
        }

        try
        {
            bool isValid = _md5Hasher.VerifyFileHash(FilePath, ExpectedHash);

            if (isValid)
            {
                Message = Lab2Messages.FileIntact;
            }
            else
            {
                Message = Lab2Messages.FileCorrupted;
            }

            ComputedHash = _md5Hasher.ComputeFileHash(FilePath);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void SaveHashToFile()
    {
        if (string.IsNullOrWhiteSpace(ComputedHash))
        {
            Message = Lab2Messages.NoHashToSave;
            return;
        }

        try
        {
            if (!Directory.Exists(RESULTS_FOLDER))
            {
                Directory.CreateDirectory(RESULTS_FOLDER);
            }

            File.WriteAllText(FILE_PATH, ComputedHash);
            Message = Lab2Messages.FileSavedMessage(FILE_PATH);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void RunTestSuite()
    {
        ResetState();
        StringBuilder results = new();

        results.AppendLine("MD5 Test Suite (RFC 1321):");

        string[] testStrings = {
            "",
            "a",
            "abc",
            "message digest",
            "abcdefghijklmnopqrstuvwxyz",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
            "12345678901234567890123456789012345678901234567890123456789012345678901234567890"
        };

        string[] expectedHashes = {
            "D41D8CD98F00B204E9800998ECF8427E",
            "0CC175B9C0F1B6A831C399E269772661",
            "900150983CD24FB0D6963F7D28E17F72",
            "F96B697D7CB7938D525A2F31AAF161D0",
            "C3FCD3D76192E4007DFB496CCA67E13B",
            "D174AB98D277D9F5A5611C2C9F419D9F",
            "57EDF4A22BE3C955AC49DA2E2107B67A"
        };

        try
        {
            bool allTestsPassed = true;

            for (int i = 0; i < testStrings.Length; ++i)
            {
                string input = testStrings[i];
                string expected = expectedHashes[i];
                string actual = _md5Hasher.ComputeHash(input);

                bool passed = string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
                allTestsPassed &= passed;

                results.AppendLine($"Тест {i + 1}: {(input.Length == 0 ? "«пустий рядок»" : input.Length <= 10 ? $"«{input}»" : $"«{input.Substring(0, 10)}...»")}");
                results.AppendLine($"  Очікуваний: {expected}");
                results.AppendLine($"  Отриманий:  {actual}");
                results.AppendLine($"  Статус: {(passed ? "Успішно" : "Помилка")}");
                results.AppendLine();
            }

            results.AppendLine($"Загальний результат: {(allTestsPassed ? "Всі тести пройдені" : "Деякі тести не пройдені")}");

            ComputedHash = results.ToString();
            Message = Lab2Messages.TestSuiteCompleted;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void ReturnToMenu()
    {
        _navigator.NavigateToMenu();
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Виберіть файл для хешування",
                AllowMultiple = false
            };

            var result = await dialog.ShowAsync(desktop.MainWindow);

            if (result != null && result.Length > 0)
            {
                FilePath = result[0];
            }
        }
    }

    private void ResetState()
    {
        Message = null;
        ComputedHash = null;
    }
}