using code.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace code.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private ViewModelBase? _currentView;
    
    public bool IsGatewayVisible => _navigator.IsMainMenuVisible;

    public MainWindowViewModel(INavigator navigator)
    {
        _navigator = navigator;
        _navigator.CurrentViewChanged += OnNavigatorCurrentViewChanged;
    }

    private void OnNavigatorCurrentViewChanged(object? sender, ViewModelBase? view)
    {
        CurrentView = view;
        OnPropertyChanged(nameof(IsGatewayVisible));
    }

    [RelayCommand]
    private void OpenLab1()
    {
        _navigator.NavigateTo<Lab1ViewModel>();
    }

    [RelayCommand]
    private void OpenLab2()
    {
        _navigator.NavigateTo<Lab2ViewModel>();
    }
}