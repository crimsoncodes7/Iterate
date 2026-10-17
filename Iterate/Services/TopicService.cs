using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iterate.Data;
using Iterate.Models;
using Microsoft.EntityFrameworkCore;

namespace Iterate.Services;

public class TopicService
{
    private readonly IDbContextFactory<IterateContext> _contextFactory;

    public TopicService(IDbContextFactory<IterateContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Topic?> GetTopicByIdAsync(int topicId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Topics
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TopicId == topicId);
    }

    public async Task<List<Topic>> GetTopicsBySubjectIdAsync(int subjectId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Topics
            .AsNoTracking()
            .Where(t => t.SubjectId == subjectId)
            .ToListAsync();
    }
    
    public async Task<Topic?> CreateTopicAsync(string name, int subjectId, int? parentTopicId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var exists = await context.Topics.AnyAsync(t =>
            t.SubjectId == subjectId &&
            t.ParentTopicId == parentTopicId &&
            t.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return null; 
        }
        
        var topic = new Topic
        {
            Name = name,
            SubjectId = subjectId,
            ParentTopicId = parentTopicId
        };

        context.Topics.Add(topic);
        await context.SaveChangesAsync();
        
        return topic;
    }
    
    public async Task<bool> UpdateProgressAsync(TopicProgress dto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var topic = await context.Topics.FindAsync(dto.TopicId);

        if (topic is null) return false;

        topic.ConfidenceStatus = dto.ConfidenceStatus;
        topic.Status = dto.Status;
        topic.CoveredInClass = dto.CoveredInClass;
        topic.StudiedIt = dto.StudiedIt;
        topic.DoneExamQs = dto.DoneExamQs;
        topic.FullyUnderstood = dto.FullyUnderstood;

        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<EditResult> UpdateTopicDetailsAsync(int topicId, string normalisedName, string? normalisedSpec)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var topic = await context.Topics.FindAsync(topicId);

        if (topic is null)
        {
            return EditResult.NotFound;
        }

        bool isDuplicate = await context.Topics.AnyAsync(t =>
            t.TopicId != topicId &&
            t.SubjectId == topic.SubjectId &&
            t.ParentTopicId == topic.ParentTopicId &&
            t.Name.ToLower() == normalisedName.ToLower());

        if (isDuplicate)
        {
            return EditResult.DuplicateName;
        }

        topic.Name = normalisedName;
        topic.SpecReference = normalisedSpec;

        await context.SaveChangesAsync();
        return EditResult.Success;
    }
    
    public async Task<bool> DeleteTopicSubtreeAsync(int topicId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var target = await context.Topics.FindAsync(topicId);
            if (target is null)
            {
                return false;
            }
            
            var subjectTopics = await context.Topics
                .Where(t => t.SubjectId == target.SubjectId)
                .ToListAsync();

            var childLookup = subjectTopics.ToLookup(t => t.ParentTopicId);
            var deletionOrder = new List<Topic>();
            
            AppendSubtree(target, childLookup, deletionOrder);

            foreach (var topic in deletionOrder)
            {
                context.Topics.Remove(topic);
                await context.SaveChangesAsync();
            }

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