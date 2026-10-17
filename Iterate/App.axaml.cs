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


public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (Avalonia.Controls.Design.IsDesignMode) return;
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
        services.AddTransient<SubjectService>();
        services.AddTransient<TopicService>();
        
        services.AddDbContextFactory<IterateContext>(options => options.UseSqlite($"Data Source={databasePath}"));
        
        var provider = services.BuildServiceProvider();
        
        var contextFactory = provider.GetRequiredService<IDbContextFactory<IterateContext>>();
        using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();
        
        if (!context.Subjects.Any())
        {
            var maths = new Subject { Name = "Mathematics", ExamBoard = "AQA" };
            var compSci = new Subject { Name = "Computer Science", ExamBoard = "AQA" };
            var itData = new Subject { Name = "IT - Data Analytics", ExamBoard = "OCR" };

            var algebra = new Topic { Name = "Algebra", Subject = maths };
            var quadratics = new Topic { Name = "Quadratics", Subject = maths, ParentTopic = algebra };
            var geometry = new Topic { Name = "Geometry", Subject = maths };

            var dataStructures = new Topic { Name = "Data Structures", Subject = compSci };
            var trees = new Topic { Name = "Trees", Subject = compSci, ParentTopic = dataStructures };
            var queues = new Topic { Name = "Queues", Subject = compSci, ParentTopic = dataStructures };

            context.Subjects.AddRange(maths, compSci, itData);
            context.Topics.AddRange(algebra, quadratics, geometry, dataStructures, trees, queues);
            context.SaveChanges();
        }
        
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