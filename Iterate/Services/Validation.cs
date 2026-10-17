namespace Iterate.Services;

public static class Validation
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 20;
    private const int MaxSpecLength = 10;
    private const int MaxExamBoardLength = 15;

    public static (bool IsValid, string NormalisedName, string? ErrorMessage) ValidateName(string? name, string entityName)
    {
        var normalised = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalised) || normalised.Length < MinNameLength)
        {
            return (false, normalised, $"{entityName} name must be at least {MinNameLength} characters.");
        }
        
        return normalised.Length > MaxNameLength 
            ? (false, normalised, $"{entityName} name cannot exceed {MaxNameLength} characters.") 
            : (true, normalised, null);
    }
    
    public static (bool IsValid, string? NormalisedSpec, string? ErrorMessage) ValidateSpecReference(string? specRef)
    {
        var normalised = string.IsNullOrWhiteSpace(specRef) ? null : specRef.Trim();

        if (normalised is not null && normalised.Length > MaxSpecLength)
        {
            return (false, normalised, $"Spec reference cannot exceed {MaxSpecLength} characters.");
        }
        
        return (true, normalised, null);
    }
    
    public static (bool IsValid, string? NormalisedExamBoard, string? ErrorMessage) ValidateExamBoard(string? examBoard)
    {
        var normalised = string.IsNullOrWhiteSpace(examBoard) ? null : examBoard.Trim();

        if (normalised is not null && normalised.Length > MaxExamBoardLength)
        {
            return (false, normalised, $"Exam board cannot exceed {MaxExamBoardLength} characters.");
        }

        return (true, normalised, null);
    }
}