using System;
using code.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace code.Models;

public class Navigator : INavigator
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase? _currentView;
    
    public ViewModelBase? CurrentView => _currentView;
    
    public bool IsMainMenuVisible => _currentView == null;
    
    public event EventHandler<ViewModelBase?>? CurrentViewChanged;
    
    public Navigator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void NavigateToMenu()
    {
        SetCurrentView(null);
    }
    
    public void NavigateTo<T>() where T : ViewModelBase
    {
        var view = _serviceProvider.GetRequiredService<T>();
        SetCurrentView(view);
    }
    
    private void SetCurrentView(ViewModelBase? view)
    {
        _currentView = view;
        CurrentViewChanged?.Invoke(this, view);
    }
}