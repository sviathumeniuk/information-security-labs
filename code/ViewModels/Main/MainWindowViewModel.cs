using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace code.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    public bool IsGatewayVisible => CurrentView is null;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        CurrentView = null;
    }

    partial void OnCurrentViewChanged(ViewModelBase? value)
    {
        OnPropertyChanged(nameof(IsGatewayVisible));
    }

    [RelayCommand]
    private void OpenLab1()
    {
        CurrentView = _serviceProvider.GetRequiredService<Lab1ViewModel>();
    }

    [RelayCommand]
    private void OpenLab2()
    {
        throw new NotImplementedException();
    }
}
