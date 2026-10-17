using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Iterate.Services;

namespace Iterate.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Router _router;
    
    [ObservableProperty] 
    private bool _isSubjectsSelected;

    public HomeViewModel(Router router)
    {
        _router = router;
    }

    [RelayCommand]
    private void GoToSubjects(string page)
    {
        IsSubjectsSelected  = page == "Subjects";
        _router.NavigateTo<SubjectsViewModel>();
    }
}