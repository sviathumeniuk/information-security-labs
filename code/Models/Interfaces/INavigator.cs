using System;
using code.ViewModels;

namespace code.Models;

public interface INavigator
{
    ViewModelBase? CurrentView { get; }
    bool IsMainMenuVisible { get; }
    void NavigateToMenu();
    void NavigateTo<T>() where T : ViewModelBase;
    event EventHandler<ViewModelBase?> CurrentViewChanged;
}