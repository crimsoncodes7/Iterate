using System.Collections.Generic;
using System.Linq;
using Iterate.Models;
using Iterate.ViewModels;

namespace Iterate.Services;

public class TopicHierarchyService
{
    public List<TopicTreeNode> BuildTopicTree(List<Topic> topics)
    {
        var lookup = topics.ToLookup(t => t.ParentTopicId);
        var roots = topics.Where(t => t.ParentTopicId == null).ToList();
        var rootNodes = roots.Select(r => BuildTree(r, lookup)).ToList();
        rootNodes.ForEach(ComputeCompletedCount);
        return rootNodes;
    }

    private TopicTreeNode BuildTree(Topic topic, ILookup<int?, Topic> lookup)
    {
        TopicTreeNode node = new TopicTreeNode { TopicId = topic.TopicId, TopicName = topic.Name, TopicStatus = topic.Status};

        foreach (var child in lookup[topic.TopicId])
        {
            TopicTreeNode childNode = BuildTree(child, lookup);
            node.Children.Add(childNode);
        }
        return node;
    }

    private void ComputeCompletedCount(TopicTreeNode node)
    {
        if (node.Children.Count == 0)
        {
            node.TotalCount = 1;
            node.CompletedCount = node.TopicStatus == Status.Completed ? 1 : 0;
        }
        else
        {
            node.TotalCount = 0;
            node.CompletedCount = 0;
            foreach (var child in node.Children)
            {
                ComputeCompletedCount(child);
                node.TotalCount += child.TotalCount;
                node.CompletedCount += child.CompletedCount;
            }
        }
    }
}