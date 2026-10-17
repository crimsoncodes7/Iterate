using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Iterate.Models;

namespace Iterate.ViewModels;

public partial class TopicTreeNode : ObservableObject
{
    public int TopicId { get; set; }
    public List<TopicTreeNode> Children { get; set; } = [];
    
    [ObservableProperty]
    private string _topicName = null!;

    [ObservableProperty]
    private string? _specReference;
    
    [ObservableProperty]
    private Status _topicStatus;

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty] 
    private bool _isExpanded;

    [ObservableProperty] 
    private bool _isSelected;
    
    public void RecalculateCounts()
    {
        if (Children.Count == 0)
        {
            TotalCount = 1;
            CompletedCount = TopicStatus == Status.Completed ? 1 : 0;
            return;
        }

        int total = 0;
        int completed = 0;

        foreach (var child in Children)
        {
            total += child.TotalCount;
            completed += child.CompletedCount;
        }

        TotalCount = total;
        CompletedCount = completed;

        TopicStatus = completed == 0 ? Status.NotStarted :
            completed == total ? Status.Completed :
            Status.Started;
    }
    
}