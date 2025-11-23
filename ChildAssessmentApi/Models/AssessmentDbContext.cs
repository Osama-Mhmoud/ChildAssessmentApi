using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using static ChildAssessmentApi.Models.Models;

namespace ChildAssessmentApi.Models
{
    //public class AssessmentDbContext : DbContext
        public class AssessmentDbContext : DbContext
    {

        public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options) { }


        public DbSet<MilestoneScore> MilestoneScores { get; set; }
        public DbSet<BarrierScore> BarrierScores { get; set; }
        public DbSet<TransitionScore> TransitionScores { get; set; }
        public DbSet<TaskAnalysisScore> TaskAnalysisScores { get; set; }

        // Data/ApplicationDbContext.cs
        public DbSet<VbmappQuestion> VbmappQuestions { get; set; } = null!;
        public DbSet<StudentAnswer> StudentAnswers { get; set; } = null!;

        public DbSet<Section> Sections { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Answer> Answers { get; set; }


        // ApplicationDbContext.cs
        public DbSet<AssessmentScore> AssessmentScores { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Section>().HasMany(s => s.Questions).WithOne(q => q.Section).HasForeignKey(q => q.SectionId);
            modelBuilder.Entity<Question>().HasMany(q => q.Options).WithOne(o => o.Question).HasForeignKey(o => o.QuestionId);
            modelBuilder.Entity<Assessment>().HasMany(a => a.Answers).WithOne(ans => ans.Assessment).HasForeignKey(ans => ans.AssessmentId);
            modelBuilder.Entity<Answer>().HasOne(ans => ans.Question).WithMany().HasForeignKey(ans => ans.QuestionId);
            modelBuilder.Entity<Assessment>().HasOne(a => a.Child).WithMany().HasForeignKey(a => a.ChildId);

            // Configure Question entity
            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.HasIndex(e => new { e.SectionId, e.Order });
                entity.HasIndex(e => e.MilestoneLevel);
                entity.HasIndex(e => e.Category);
            });

            // Seed sections
            modelBuilder.Entity<Section>().HasData(
                new Section { Id = 1, Name = "VB-MAPP Overview" },
                new Section { Id = 2, Name = "Milestones" },
                new Section { Id = 3, Name = "Barriers" },
                new Section { Id = 4, Name = "Transitions" },
                new Section { Id = 5, Name = "EESA" },
                new Section { Id = 6, Name = "Task Analysis" }
            );
            
            // Configure QuestionOption entity
            modelBuilder.Entity<QuestionOption>(entity =>
            {
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => new { e.QuestionId, e.Order });
            });
        }
    }





}

