using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iterate.Data;
using Iterate.Models;
using Iterate.Services;
using Microsoft.EntityFrameworkCore;

namespace Iterate.ViewModels;

public partial class SubjectsViewModel : ViewModelBase
{
    private readonly Router _router;
    private readonly IDbContextFactory<IterateContext> _contextFactory;
    private readonly TopicHierarchyService _topicHierarchyService;

    [ObservableProperty]
    private ObservableCollection<Subject> _subjects = [];
    
    [ObservableProperty]
    private ObservableCollection<TopicTreeNode> _topicTree = [];

    [ObservableProperty] 
    private Subject? _selectedSubject;
    
    
    public SubjectsViewModel(Router router, IDbContextFactory<IterateContext> contextFactory
                            ,TopicHierarchyService topicHierarchyService)
    {
        _router = router;
        _contextFactory = contextFactory;
        _topicHierarchyService = topicHierarchyService;
        
        _ = LoadSubjectsAsync();
    }
    
    private async Task LoadSubjectsAsync()
    {
        var context = await _contextFactory.CreateDbContextAsync();
        var subjects = await context.Subjects
            .AsNoTracking()
            .Include(s => s.Topics)
            .ToListAsync();
        
        await Dispatcher.UIThread.InvokeAsync(() => Subjects = new ObservableCollection<Subject>(subjects));
    }
    
    partial void OnSelectedSubjectChanged(Subject? value)
    {
        if (value is null)
        {
            TopicTree = [];
            return;
        }

        var nodes = _topicHierarchyService.BuildTopicTree(value.Topics);
        TopicTree = new ObservableCollection<TopicTreeNode>(nodes);
        foreach (var node in nodes)
            PrintTree(node);
    }
    
    private void PrintTree(TopicTreeNode node, int depth = 0)
    {
        Console.WriteLine($"{new string(' ', depth * 2)}{node.TopicName} [{node.TopicStatus}] {node.CompletedCount}/{node.TotalCount}");
        foreach (var child in node.Children)
            PrintTree(child, depth + 1);
    }

    [RelayCommand]
    private void GoToHome() => _router.NavigateTo<HomeViewModel>();
}