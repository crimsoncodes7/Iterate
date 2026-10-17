namespace Iterate.Models;

public record TopicProgress(int TopicId, ConfidenceStatus? ConfidenceStatus, Status Status,
                             bool CoveredInClass, bool StudiedIt, bool DoneExamQs, bool FullyUnderstood);