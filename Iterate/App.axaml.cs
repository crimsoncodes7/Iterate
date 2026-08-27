using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Iterate.Data;
using Iterate.Models;
using Iterate.Services;
using Iterate.ViewModels;
using Iterate.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Iterate;


public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Iterate");
        Directory.CreateDirectory(dataFolder);
        var databasePath = Path.Combine(dataFolder, "iterate.db");
        
        var services = new ServiceCollection();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<Router>();
        services.AddSingleton<TopicHierarchyService>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<SubjectsViewModel>();
        services.AddTransient<ReviewViewModel>();
        services.AddTransient<ProductivityViewModel>();
        services.AddTransient<AssistantViewModel>();
        services.AddTransient<HelpViewModel>();
        
        services.AddDbContextFactory<IterateContext>(options => options.UseSqlite($"Data Source={databasePath}"));
        
        var provider = services.BuildServiceProvider();
        
        var contextFactory = provider.GetRequiredService<IDbContextFactory<IterateContext>>();
        using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
}