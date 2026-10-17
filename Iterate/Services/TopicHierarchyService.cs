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
        rootNodes.ForEach(ComputeRecursively);
        return rootNodes;
    }

    private TopicTreeNode BuildTree(Topic topic, ILookup<int?, Topic> lookup)
    {
        var node = new TopicTreeNode { TopicId = topic.TopicId, TopicName = topic.Name, SpecReference = topic.SpecReference, TopicStatus = topic.Status};

        foreach (var child in lookup[topic.TopicId])
        {
            node.Children.Add(BuildTree(child, lookup));
        }
        return node;
    }

    private void ComputeRecursively(TopicTreeNode node)
    {
        foreach (var child in node.Children)
        {
            ComputeRecursively(child);
        }
        node.RecalculateCounts();
    }
}