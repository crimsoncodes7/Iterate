using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iterate.Models;
using Iterate.Services;

namespace Iterate.ViewModels;

public partial class SubjectsViewModel : ViewModelBase
{
    private readonly Router _router;
    private readonly TopicHierarchyService _topicHierarchyService;
    private readonly SubjectService _subjectService;
    private readonly TopicService _topicService;
    private TopicTreeNode? _nodeBeingEdited;
    private Subject? _subjectBeingEdited;
    private TopicTreeNode? _topicPendingDelete;
    private Subject? _subjectPendingDelete;

    [ObservableProperty]
    private ObservableCollection<Subject> _subjects = [];
    
    [ObservableProperty]
    private ObservableCollection<TopicTreeNode> _topicTree = [];

    [ObservableProperty] 
    private Subject? _selectedSubject;

    [ObservableProperty] 
    private TopicTreeNode? _selectedTopicNode;

    [ObservableProperty] 
    private TopicViewModel? _selectedTopicViewModel;
    
    [ObservableProperty]
    private bool _isSidebarExpanded = true;

    [ObservableProperty]
    private double _sidebarWidth = 250;

    [ObservableProperty] 
    private string? _newSubjectName;
    
    [ObservableProperty]
    private string? _addSubjectErrorMessage;
    
    [ObservableProperty]
    private string? _newTopicName;

    [ObservableProperty]
    private string? _addTopicErrorMessage;
    
    [ObservableProperty]
    private bool _isEditTopicDialogOpen;

    [ObservableProperty]
    private string? _editTopicName;

    [ObservableProperty]
    private string? _editTopicSpecReference;

    [ObservableProperty]
    private string? _editTopicErrorMessage;
    
    [ObservableProperty]
    private bool _isEditSubjectDialogOpen;

    [ObservableProperty]
    private string? _editSubjectName;

    [ObservableProperty]
    private string? _editSubjectExamBoard;
    
    [ObservableProperty]
    private string? _editSubjectErrorMessage;
    
    [ObservableProperty]
    private bool _isConfirmDeleteOpen;

    [ObservableProperty]
    private string? _confirmDeleteTitle;

    [ObservableProperty]
    private string? _confirmDeleteMessage;
    
    public SubjectsViewModel(Router router, TopicHierarchyService topicHierarchyService, 
                             SubjectService subjectService, TopicService topicService)
    {
        _router = router;
        _topicHierarchyService = topicHierarchyService;
        _subjectService = subjectService;
        _topicService = topicService;
        
        _ = LoadSubjectsAsync();
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
        SidebarWidth = IsSidebarExpanded ? 250 : 0;
    }
    
    private async Task LoadSubjectsAsync()
    {
        var subjects = await _subjectService.GetAllSubjectsWithTopicsAsync();
        Subjects = new ObservableCollection<Subject>(subjects);
        SelectedSubject = Subjects.FirstOrDefault();
    }
    
    partial void OnSelectedSubjectChanged(Subject? value)
    {
        RebuildTopicTree();
    }
    
    private void RebuildTopicTree()
    {
        if (SelectedSubject is null)
        {
            TopicTree = [];
            return;
        }

        var nodes = _topicHierarchyService.BuildTopicTree(SelectedSubject.Topics);
        TopicTree = new ObservableCollection<TopicTreeNode>(nodes);
    }
    
    partial void OnSelectedTopicNodeChanged(TopicTreeNode? value)
    {
        if (value is null || value.Children.Any())
        {
            SelectedTopicViewModel = null;
            return;
        }
        
        _ = LoadTopicDetailsAsync(value.TopicId);
    }
  
    private async Task LoadTopicDetailsAsync(int requestedTopicId)
    {
        var topic = await _topicService.GetTopicByIdAsync(requestedTopicId);
       
        if (SelectedTopicNode?.TopicId != requestedTopicId) return;
        
        if (topic is null)
        {
            SelectedTopicViewModel = null;
            SelectedTopicNode = null;
            return;
        }
        
        SelectedTopicViewModel = new TopicViewModel(topic, _topicService, OnTopicProgressUpdated);
    }
    
    private void OnTopicProgressUpdated(TopicProgress dto)
    {
        if (SelectedSubject is null) return;
        
        var cachedTopic = SelectedSubject.Topics.FirstOrDefault(t => t.TopicId == dto.TopicId);
        if (cachedTopic is not null)
        {
            cachedTopic.ConfidenceStatus = dto.ConfidenceStatus;
            cachedTopic.Status = dto.Status;
            cachedTopic.CoveredInClass = dto.CoveredInClass;
            cachedTopic.StudiedIt = dto.StudiedIt;
            cachedTopic.DoneExamQs = dto.DoneExamQs;
            cachedTopic.FullyUnderstood = dto.FullyUnderstood;
        }
        
        foreach (var rootNode in TopicTree)
        {
            if (UpdateTreeNodeAndParents(rootNode, dto))
            {
                break;
            }
        }
    }
    
    private static bool UpdateTreeNodeAndParents(TopicTreeNode node, TopicProgress dto)
    {
        if (node.TopicId == dto.TopicId)
        {
            node.TopicStatus = dto.Status;
            node.RecalculateCounts();
            return true;
        }

        foreach (var child in node.Children)
        {
            if (UpdateTreeNodeAndParents(child, dto))
            {
                node.RecalculateCounts();
                return true;
            }
        }
        return false;
    }
    
    [RelayCommand]
    private async Task AddSubjectAsync()
    {
        (bool isValid, string normalisedName, string? error) = Validation.ValidateName(NewSubjectName, "Subject");
        if (!isValid)
        {
            AddSubjectErrorMessage = error;
            return;
        }

        AddSubjectErrorMessage = null;
        var newSubject = await _subjectService.CreateSubjectAsync(normalisedName);

        if (newSubject is null)
        {
            AddSubjectErrorMessage = "A subject with this name already exists.";
            return;
        }

        Subjects.Add(newSubject);
        SelectedSubject = newSubject;
        NewSubjectName = string.Empty;
    }
    
    [RelayCommand]
    private void OpenEditSubjectDialog(Subject subject)
    {
        _subjectBeingEdited = subject;
        EditSubjectName = subject.Name;
        EditSubjectExamBoard = subject.ExamBoard;
        EditSubjectErrorMessage = null;
        IsEditSubjectDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEditSubject()
    {
        IsEditSubjectDialogOpen = false;
        _subjectBeingEdited = null;
        EditSubjectName = null;
        EditSubjectExamBoard = null;
        EditSubjectErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveEditSubjectAsync()
    {
        if (_subjectBeingEdited is null) return;

        (bool isNameValid, string normalisedName, string? nameError) = Validation.ValidateName(EditSubjectName, "Subject");
        if (!isNameValid)
        {
            EditSubjectErrorMessage = nameError;
            return;
        }

        (bool isBoardValid, string? normalisedBoard, string? boardError) = Validation.ValidateExamBoard(EditSubjectExamBoard);
        if (!isBoardValid)
        {
            EditSubjectErrorMessage = boardError;
            return;
        }

        var targetSubject = _subjectBeingEdited;
        var result = await _subjectService.RenameSubjectAsync(targetSubject.SubjectId, normalisedName, normalisedBoard);

        switch (result)
        {
            case EditResult.DuplicateName:
                EditSubjectErrorMessage = "A subject with this name already exists.";
                return;

            case EditResult.NotFound:
                IsEditSubjectDialogOpen = false;
                _subjectBeingEdited = null;
                Subjects.Remove(targetSubject);
                if (SelectedSubject?.SubjectId == targetSubject.SubjectId)
                {
                    SelectedSubject = Subjects.FirstOrDefault();
                }
                return;

            case EditResult.Success:
                break;
        }
        
        int index = Subjects.IndexOf(targetSubject);
        if (index != -1)
        {
            bool wasSelected = SelectedSubject?.SubjectId == targetSubject.SubjectId;

            var updatedSubject = new Subject
            {
                SubjectId = targetSubject.SubjectId,
                Name = normalisedName,
                ExamBoard = normalisedBoard,
                Topics = targetSubject.Topics
            };

            Subjects[index] = updatedSubject;

            if (wasSelected)
            {
                SelectedSubject = updatedSubject;
            }
        }

        IsEditSubjectDialogOpen = false;
        _subjectBeingEdited = null;
    }

    partial void OnEditSubjectNameChanged(string? value)
    {
        if (!string.IsNullOrEmpty(EditSubjectErrorMessage))
        {
            EditSubjectErrorMessage = null;
        }
    }
    
    partial void OnEditSubjectExamBoardChanged(string? value)
    {
        if (!string.IsNullOrEmpty(EditSubjectErrorMessage))
        {
            EditSubjectErrorMessage = null;
        }
    }

    [RelayCommand]
    private async Task AddTopicAsync()
    {
        if (SelectedSubject is null) return;

        (bool isValid, string normalisedName, string? error) = Validation.ValidateName(NewTopicName, "Topic");
        if (!isValid)
        {
            AddTopicErrorMessage = error;
            return;
        }

        AddTopicErrorMessage = null;
        int? parentId = SelectedTopicNode?.TopicId;
        
        var newTopic = await _topicService.CreateTopicAsync(normalisedName, SelectedSubject.SubjectId, parentId);

        if (newTopic is null)
        {
            AddTopicErrorMessage = "A subtopic with this name already exists with this topic.";
            return;
        }

        NewTopicName = string.Empty;
        SelectedTopicNode = null;
        SelectedTopicViewModel = null;
        
        await RefreshTreeAsync(parentId);
    }
    
    [RelayCommand]
    private void OpenEditTopicDialog(TopicTreeNode node)
    {
        _nodeBeingEdited = node;
        EditTopicName = node.TopicName;
        EditTopicSpecReference = node.SpecReference;
        EditTopicErrorMessage = null;
        IsEditTopicDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEditTopic()
    {
        IsEditTopicDialogOpen = false;
        _nodeBeingEdited = null;
        EditTopicErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveEditTopicAsync()
    {
        if (_nodeBeingEdited is null || SelectedSubject is null) return;

        (bool isNameValid, string normalisedName, string? nameError) = Validation.ValidateName(EditTopicName, "Topic");
        if (!isNameValid)
        {
            EditTopicErrorMessage = nameError;
            return;
        }
        
        (bool isSpecValid, string? normalisedSpec, string? specError) = Validation.ValidateSpecReference(EditTopicSpecReference);
        if (!isSpecValid)
        {
            EditTopicErrorMessage = specError;
            return;
        }
        
        var targetNode = _nodeBeingEdited;
        var targetSubject = SelectedSubject;

        var result = await _topicService.UpdateTopicDetailsAsync(targetNode.TopicId, normalisedName, normalisedSpec);

        switch (result)
        {
            case EditResult.DuplicateName:
                EditTopicErrorMessage = "A topic with this name already exists in this folder.";
                return;

            case EditResult.NotFound:
                IsEditTopicDialogOpen = false;
                _nodeBeingEdited = null;
                if (SelectedTopicNode?.TopicId == targetNode.TopicId)
                {
                    SelectedTopicNode = null;
                    SelectedTopicViewModel = null;
                }
                await RefreshTreeAsync(null);
                return;

            case EditResult.Success:
                break;
        }
        
        var cached = targetSubject.Topics.FirstOrDefault(t => t.TopicId == targetNode.TopicId);
        if (cached is not null)
        {
            cached.Name = normalisedName;
            cached.SpecReference = normalisedSpec;
        }
        
        targetNode.TopicName = normalisedName;
        targetNode.SpecReference = normalisedSpec;
        
        if (SelectedTopicViewModel?.TopicId == targetNode.TopicId)
        {
            SelectedTopicViewModel.TopicTitle = normalisedName;
        }

        IsEditTopicDialogOpen = false;
        _nodeBeingEdited = null;
    }

    partial void OnNewSubjectNameChanged(string? value)
    {
        if (!string.IsNullOrEmpty(AddSubjectErrorMessage))
        {
            AddSubjectErrorMessage = null;
        }
    }
    
    partial void OnNewTopicNameChanged(string? value)
    {
        if (!string.IsNullOrEmpty(AddTopicErrorMessage))
        {
            AddTopicErrorMessage = null;
        }
    }

    partial void OnEditTopicNameChanged(string? value)
    {
        if (!string.IsNullOrEmpty(EditTopicErrorMessage))
        {
            EditTopicErrorMessage = null;
        }
    }

    partial void OnEditTopicSpecReferenceChanged(string? value)
    {
        if (!string.IsNullOrEmpty(EditTopicErrorMessage))
        {
            EditTopicErrorMessage = null;
        }
    }
    
    private async Task RefreshTreeAsync(int? expandedParentId)
    {
        if (SelectedSubject is null) return;

        var expandedIds = new HashSet<int>();
        CaptureExpandedTopicIds(TopicTree, expandedIds);

        if (expandedParentId.HasValue)
        {
            expandedIds.Add(expandedParentId.Value);
        }
        
        SelectedSubject.Topics = await _topicService.GetTopicsBySubjectIdAsync(SelectedSubject.SubjectId);
        RebuildTopicTree();
        
        RestoreExpandedTopicIds(TopicTree, expandedIds);
    }
    
    private static void CaptureExpandedTopicIds(IEnumerable<TopicTreeNode> nodes, HashSet<int> expandedIds)
    {
        foreach (var node in nodes)
        {
            if (node.IsExpanded)
            {
                expandedIds.Add(node.TopicId);
            }
            CaptureExpandedTopicIds(node.Children, expandedIds);
        }
    }

    private static void RestoreExpandedTopicIds(IEnumerable<TopicTreeNode> nodes, HashSet<int> expandedIds)
    {
        foreach (var node in nodes)
        {
            if (expandedIds.Contains(node.TopicId))
            {
                node.IsExpanded = true;
            }
            RestoreExpandedTopicIds(node.Children, expandedIds);
        }
    }
    
    [RelayCommand]
    private void RequestDeleteTopic(TopicTreeNode node)
    {
        _topicPendingDelete = node;
        _subjectPendingDelete = null;

        int descendantCount = CountDescendants(node);
        ConfirmDeleteTitle = "Delete Topic";
        ConfirmDeleteMessage = descendantCount > 0
            ? $"Are you sure you want to delete '{node.TopicName}' and its {descendantCount} subtopic(s)? This cannot be undone."
            : $"Are you sure you want to delete '{node.TopicName}'? This cannot be undone.";

        IsConfirmDeleteOpen = true;
    }

    [RelayCommand]
    private void RequestDeleteSubject(Subject subject)
    {
        _subjectPendingDelete = subject;
        _topicPendingDelete = null;

        int topicCount = subject.Topics.Count;
        ConfirmDeleteTitle = "Delete Subject";
        ConfirmDeleteMessage = topicCount > 0
            ? $"Are you sure you want to delete '{subject.Name}' and all {topicCount} of its topic(s)? This cannot be undone."
            : $"Are you sure you want to delete '{subject.Name}'? This cannot be undone.";

        IsConfirmDeleteOpen = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsConfirmDeleteOpen = false;
        _topicPendingDelete = null;
        _subjectPendingDelete = null;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        if (_topicPendingDelete is not null)
        {
            var targetNode = _topicPendingDelete;
            IsConfirmDeleteOpen = false;
            _topicPendingDelete = null;

            await _topicService.DeleteTopicSubtreeAsync(targetNode.TopicId);
            
            if (SelectedTopicNode is not null && IsNodeInSubtree(targetNode, SelectedTopicNode.TopicId))
            {
                SelectedTopicNode = null;
                SelectedTopicViewModel = null;
            }

            await RefreshTreeAsync(expandedParentId: null);
        }
        else if (_subjectPendingDelete is not null)
        {
            var targetSubject = _subjectPendingDelete;
            IsConfirmDeleteOpen = false;
            _subjectPendingDelete = null;

            await _subjectService.DeleteSubjectAsync(targetSubject.SubjectId);

            bool wasSelected = SelectedSubject?.SubjectId == targetSubject.SubjectId;
            Subjects.Remove(targetSubject);

            if (wasSelected)
            {
                SelectedTopicNode = null;
                SelectedTopicViewModel = null;
                SelectedSubject = Subjects.FirstOrDefault();
            }
        }
    }

    private static int CountDescendants(TopicTreeNode node)
    {
        int count = node.Children.Count;
        foreach (var child in node.Children)
        {
            count += CountDescendants(child);
        }
        return count;
    }

    private static bool IsNodeInSubtree(TopicTreeNode root, int targetTopicId)
    {
        if (root.TopicId == targetTopicId) return true;

        foreach (var child in root.Children)
        {
            if (IsNodeInSubtree(child, targetTopicId)) return true;
        }

        return false;
    }
    
    [RelayCommand]
    private void GoToHome() => _router.NavigateTo<HomeViewModel>();
}