using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using static ChildAssessmentApi.Models.Models;

namespace ChildAssessmentApi.Models
{
    //public class AssessmentDbContext : DbContext
        public class AssessmentDbContext : DbContext
    {


       // using Microsoft.EntityFrameworkCore;

//public class AssessmentDbContext : DbContext
   // {
        public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options) : base(options) { }

        public DbSet<Section> Sections { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Section>().HasMany(s => s.Questions).WithOne(q => q.Section).HasForeignKey(q => q.SectionId);
            modelBuilder.Entity<Assessment>().HasMany(a => a.Answers).WithOne(ans => ans.Assessment).HasForeignKey(ans => ans.AssessmentId);
            modelBuilder.Entity<Answer>().HasOne(ans => ans.Question).WithMany().HasForeignKey(ans => ans.QuestionId);
            modelBuilder.Entity<Assessment>().HasOne(a => a.Child).WithMany().HasForeignKey(a => a.ChildId);

            // Seed sections
            modelBuilder.Entity<Section>().HasData(
                new Section { Id = 1, Name = "التفاعل الاجتماعي" },
                new Section { Id = 2, Name = "التواصل" },
                new Section { Id = 3, Name = "السلوك" },
                new Section { Id = 4, Name = "المعالجة الحسية" },
                new Section { Id = 5, Name = "المهارات الحركية" },
                new Section { Id = 6, Name = "التطور الإدراكي" }
            );
        }
    }





}

