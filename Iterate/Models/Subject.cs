using System.Collections.Generic;

namespace Iterate.Models;

public class Subject
{
    public int SubjectId { get; set; }
    public required string Name { get; set; }
    public string? ExamBoard { get; set; }
    public List<Topic> Topics { get; set; } = [];
}