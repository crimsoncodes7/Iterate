using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iterate.Models;
using Iterate.Services;

namespace Iterate.ViewModels;

public partial class TopicViewModel : ViewModelBase
{
    private readonly TopicService _topicService;
    private readonly Action<TopicProgress>? _onTopicUpdated;
    
    public int TopicId { get; }
    
    public Status Status => GetStatus();
    public ConfidenceStatus? ConfidenceStatus => Struggling ? Models.ConfidenceStatus.Red : 
        Developing ? Models.ConfidenceStatus.Yellow : Mastered ? Models.ConfidenceStatus.Green : null;
    
    private int CompletedCount => (CoveredInClass ? 1 : 0) + (StudiedIt ? 1 : 0) 
                                + (DoneExamQs ? 1 : 0) + (FullyUnderstood ? 1 : 0);
    
    [ObservableProperty]
    private string _topicTitle;
        
    [NotifyPropertyChangedFor(nameof(Status))]
    [ObservableProperty]
    private bool _coveredInClass;
    
    [NotifyPropertyChangedFor(nameof(Status))]
    [ObservableProperty]
    private bool _studiedIt;
    
    [NotifyPropertyChangedFor(nameof(Status))]
    [ObservableProperty]
    private bool _doneExamQs;
    
    [NotifyPropertyChangedFor(nameof(Status))]
    [ObservableProperty]
    private bool _fullyUnderstood;

    [NotifyPropertyChangedFor(nameof(ConfidenceStatus))]
    [ObservableProperty] 
    private bool _struggling;
    
    [NotifyPropertyChangedFor(nameof(ConfidenceStatus))]
    [ObservableProperty] 
    private bool _developing;
    
    [NotifyPropertyChangedFor(nameof(ConfidenceStatus))]
    [ObservableProperty] 
    private bool _mastered;
    
    public TopicViewModel(Topic topic, TopicService topicService, Action<TopicProgress>? onTopicUpdated = null)
    {
        TopicId = topic.TopicId;
        _topicTitle = topic.Name;
        _topicService = topicService;
        _onTopicUpdated = onTopicUpdated;

        _coveredInClass = topic.CoveredInClass;
        _studiedIt = topic.StudiedIt;
        _doneExamQs = topic.DoneExamQs;
        _fullyUnderstood = topic.FullyUnderstood;
        
        Struggling = topic.ConfidenceStatus == Models.ConfidenceStatus.Red;
        Developing = topic.ConfidenceStatus == Models.ConfidenceStatus.Yellow;
        Mastered = topic.ConfidenceStatus == Models.ConfidenceStatus.Green;
    }
    
    
    partial void OnCoveredInClassChanged(bool value) => _ = SaveAsync();
    partial void OnStudiedItChanged(bool value) => _ = SaveAsync();
    partial void OnDoneExamQsChanged(bool value) => _ = SaveAsync();
    partial void OnFullyUnderstoodChanged(bool value) => _ = SaveAsync();
    
    private Status GetStatus()
    {
        return CompletedCount switch
        {
            0 => Status.NotStarted,
            4 => Status.Completed,
            _ => Status.Started
        };
    }
    
    private async Task SaveAsync()
    {
        var dto = new TopicProgress(TopicId, ConfidenceStatus, Status, CoveredInClass, StudiedIt, DoneExamQs, FullyUnderstood);

        var isSaved = await _topicService.UpdateProgressAsync(dto);
        
        if (isSaved)
        {
            _onTopicUpdated?.Invoke(dto);
        }
    }

    [RelayCommand]
    private async Task SelectConfidenceStatus(string status)
    {
        Struggling = status == "Struggling";
        Developing = status == "Developing";
        Mastered = status == "Mastered";
        
        await SaveAsync();
    }
}