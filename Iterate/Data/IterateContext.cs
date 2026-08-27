using Iterate.Models;
using Microsoft.EntityFrameworkCore;

namespace Iterate.Data;

public class IterateContext : DbContext
{
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;
    
    public IterateContext() { }
    public IterateContext(DbContextOptions<IterateContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topic>()
            .HasOne(t => t.Subject)
            .WithMany(s => s.Topics)
            .HasForeignKey(t => t.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Topic>()
            .HasOne(t => t.ParentTopic)
            .WithMany(p => p.SubTopics)
            .HasForeignKey(t => t.ParentTopicId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Topic>()
            .HasIndex(t => new { t.SubjectId, t.ParentTopicId, t.Name})
            .IsUnique();
    }
}