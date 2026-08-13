using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Iterate.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Iterate.Services;

public partial class Router : ObservableObject
{
    private readonly IServiceProvider _provider;
    
    [ObservableProperty] 
    private ViewModelBase currentViewModel;
    
    public Router(IServiceProvider provider)
    {
        this._provider = provider;
    }
    
    public void NavigateTo<T>() where T : ViewModelBase
        => CurrentViewModel = _provider.GetRequiredService<T>();
}