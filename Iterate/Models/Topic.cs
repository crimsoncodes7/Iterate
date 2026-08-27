using System.Collections.Generic;

namespace Iterate.Models;

public class Topic
{
    public int TopicId { get; set; }
    public required string Name { get; set; }
    public Status Status { get; set; } = Status.NotStarted;
    public string? SpecReference { get; set; }
    public bool CoveredInClass { get; set; } = false;
    public bool StudiedIt { get; set; } = false;
    public bool DoneExamQs { get; set; } = false;
    public bool FullyUnderstood { get; set; } = false;
    public ConfidenceStatus? ConfidenceStatus { get; set; }
    
    public Topic? ParentTopic { get; set; }
    public int? ParentTopicId { get; set; }
    public List<Topic> SubTopics { get; set; } = [];

    public Subject? Subject { get; set; }
    public int SubjectId { get; set; }
}