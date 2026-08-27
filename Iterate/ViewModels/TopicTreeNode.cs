using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Iterate.Models;

namespace Iterate.ViewModels;

public partial class TopicTreeNode : ObservableObject
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = null!;
    public Status TopicStatus { get; set; }
    public List<TopicTreeNode> Children { get; set; } = [];
    
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }

    [ObservableProperty] 
    private bool _isExpanded;

    [ObservableProperty] 
    private bool _isSelected;
    
    
}