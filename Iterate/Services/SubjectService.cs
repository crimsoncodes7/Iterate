using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iterate.Data;
using Iterate.Models;
using Microsoft.EntityFrameworkCore;

namespace Iterate.Services;

public class SubjectService
{
    private readonly IDbContextFactory<IterateContext> _contextFactory;

    public SubjectService(IDbContextFactory<IterateContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Subject>> GetAllSubjectsWithTopicsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Subjects
            .AsNoTracking()
            .Include(s => s.Topics)
            .ToListAsync();
    }

    public async Task<Subject?> CreateSubjectAsync(string name)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        bool exists = await context.Subjects
            .AnyAsync(s => s.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return null;
        }

        var subject = new Subject
        {
            Name = name,
            Topics = []
        };

        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
        return subject;
    }
    
    public async Task<EditResult> RenameSubjectAsync(int subjectId, string normalisedName, string? normalisedExamBoard)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var subject = await context.Subjects.FindAsync(subjectId);

        if (subject is null)
        {
            return EditResult.NotFound;
        }

        bool isDuplicate = await context.Subjects.AnyAsync(s =>
            s.SubjectId != subjectId &&
            s.Name.ToLower() == normalisedName.ToLower());

        if (isDuplicate)
        {
            return EditResult.DuplicateName;
        }

        subject.Name = normalisedName;
        subject.ExamBoard = normalisedExamBoard;

        await context.SaveChangesAsync();
        return EditResult.Success;
    }
    
    public async Task<bool> DeleteSubjectAsync(int subjectId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var subject = await context.Subjects.FindAsync(subjectId);
            if (subject is null)
            {
                return false;
            }

            var topics = await context.Topics
                .Where(t => t.SubjectId == subjectId)
                .ToListAsync();

            var childLookup = topics.ToLookup(t => t.ParentTopicId);
            var deletionOrder = new List<Topic>();
            
            foreach (var root in topics.Where(t => t.ParentTopicId is null))
            {
                AppendSubtree(root, childLookup, deletionOrder);
            }

            foreach (var topic in deletionOrder)
            {
                context.Topics.Remove(topic);
                await context.SaveChangesAsync();
            }

            context.Subjects.Remove(subject);
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void AppendSubtree(Topic current, ILookup<int?, Topic> lookup, List<Topic> topics)
    {
        foreach (var child in lookup[current.TopicId])
        {
            AppendSubtree(child, lookup, topics);
        }
        topics.Add(current);
    }
}