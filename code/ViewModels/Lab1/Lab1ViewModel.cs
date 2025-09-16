using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using code.Models;
using System.IO;
using System.Text;

namespace code.ViewModels;

public partial class Lab1ViewModel : ViewModelBase
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IPiCalculator _piCalculator;

    [ObservableProperty] private string? _count = null;
    [ObservableProperty] private int _seed = 16;
    [ObservableProperty] private int _a = 3125;
    [ObservableProperty] private int _c = 3;
    [ObservableProperty] private int _m = 8191;
    [ObservableProperty] private ObservableCollection<int> _numbers = new();
    [ObservableProperty] private string? _message = null;
    [ObservableProperty] private string? _piValue = null;
    [ObservableProperty] private bool _isCustomGenerator = false;

    private const string RESULTS_FOLDER = "Results";
    private const string FILE_PATH = "Results/numbers.txt";
    

    public Lab1ViewModel(
        IRandomGenerator randomGenerator,
        IPiCalculator piCalculator)
    {
        _randomGenerator = randomGenerator;
        _piCalculator = piCalculator;
    }

    [RelayCommand]
    private void Generate()
    {
        ResetState();

        if (!ValidateInput(out int count))
            return;

        GenerateNumbers(count);
    }

    [RelayCommand]
    private void CalculatePeriod()
    {
        try
        {
            var period = _randomGenerator.CalculatePeriod(Seed, A, C, M);
            Message = (period == -1)
                ? Lab1Messages.InfinitePeriod
                : Lab1Messages.GetPeriodMessage(period);
        }
        catch (System.Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void TestPiCalculation()
    {
        try
        {
            if (Numbers.Count < 2)
            {
                Message = Lab1Messages.NotEnoughNumbers;
                return;
            }

            var (pi, error) = _piCalculator.CalculatePi(Numbers.ToArray(), A, C, M);
            PiValue = Lab1Messages.GetPiEstimationMessage(pi, error);
        }
        catch (System.Exception ex)
        {
            Message = ex.Message;
        }
    }

    private void ResetState()
    {
        Message = null;
        Numbers.Clear();
    }

    private bool ValidateInput(out int count)
    {
        count = 0;

        if (string.IsNullOrWhiteSpace(Count))
        {
            Message = Lab1Messages.EmptyCount;
            return false;
        }

        if (!int.TryParse(Count, out count))
        {
            Message = Lab1Messages.NotInteger;
            return false;
        }

        if (count <= 0)
        {
            Message = Lab1Messages.NotPositive;
            return false;
        }

        return true;
    }

    private void GenerateNumbers(int count)
    {
        try
        {
            var generatedNumbers = IsCustomGenerator
                ? _randomGenerator.Generate(count, Seed, A, C, M)
                : _randomGenerator.GenerateSystemRandom(count, Seed, M);

            foreach (var num in generatedNumbers)
                Numbers.Add(num);

            SaveNumbersToFile();

            Message = Lab1Messages.FileSavedMessage(FILE_PATH); 
        }
        catch (System.Exception ex)
        {
            Message = ex.Message;
        }
    }

    private void SaveNumbersToFile()
    {
        try
        {
            if (!Directory.Exists(RESULTS_FOLDER))
            {
                Directory.CreateDirectory(RESULTS_FOLDER);
            }

            var sb = new StringBuilder();
            foreach (var number in Numbers)
            {
                sb.Append(number);
                sb.Append(' ');
            }

            File.WriteAllText(FILE_PATH, sb.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }
}
