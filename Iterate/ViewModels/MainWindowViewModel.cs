using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iterate.Services;

namespace Iterate.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public Router Router { get; }

    public MainWindowViewModel(Router router)
    {
        Router = router;
        Router.NavigateTo<HomeViewModel>();
    }

    [ObservableProperty] private bool _isHomeSelected = true;

    [ObservableProperty] private bool _isSubjectsSelected;

    [ObservableProperty] private bool _isReviewSelected;

    [ObservableProperty] private bool _isProductivitySelected;

    [ObservableProperty] private bool _isAssistantSelected;

    [ObservableProperty] private bool _isHelpSelected;
    
    [ObservableProperty]
    private bool _isSidebarExpanded = true;

    [ObservableProperty]
    private double _sidebarWidth = 196;

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
        SidebarWidth = IsSidebarExpanded ? 196 : 72;
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        SelectNavItem("Home");
        Router.NavigateTo<HomeViewModel>();
    }

    [RelayCommand]
    private void NavigateToSubjects()
    {
        SelectNavItem("Subjects");
        Router.NavigateTo<SubjectsViewModel>();
    }

    [RelayCommand]
    private void NavigateToReview()
    {
        SelectNavItem("Review");
        Router.NavigateTo<ReviewViewModel>();
    }

    [RelayCommand]
    private void NavigateToProductivity()
    {
        SelectNavItem("Productivity");
        Router.NavigateTo<ProductivityViewModel>();
    }

    [RelayCommand]
    private void NavigateToAssistant()
    {
        SelectNavItem("Assistant");
        Router.NavigateTo<AssistantViewModel>();
    }

    [RelayCommand]
    private void NavigateToHelp()
    {
        SelectNavItem("Help");
        Router.NavigateTo<HelpViewModel>();
    }

    [RelayCommand]
    private void SelectNavItem(string item)
    {
        IsHomeSelected = item == "Home";
        IsSubjectsSelected  = item == "Subjects";
        IsReviewSelected  = item == "Review";
        IsProductivitySelected  = item == "Productivity";
        IsAssistantSelected  = item == "Assistant";
        IsHelpSelected  = item == "Help";
    }
}