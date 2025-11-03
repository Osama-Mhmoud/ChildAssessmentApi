using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;
using System.IO;
using static ChildAssessmentApi.Models.Models;

namespace ChildAssessmentApi.Migrations
{
    public partial class SeedAllAssessmentData : Migration
    {
        // تعريف الأقسام مع الملفات و SectionId
        private readonly (string FileName, int SectionId)[] Sections =
        {
            ("barriers_questions.json", 1),
            ("milestones_questions.json", 2),
            ("transitions_questions.json", 3),
            ("task_analysis_questions.json", 6),
            ("eesa_questions.json", 5)
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (fileName, sectionId) in Sections)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", fileName);

                if (!File.Exists(filePath))
                {
                    // لو الملف مش موجود، نطبع تحذير (مش خطأ)
                    migrationBuilder.Sql($"-- WARNING: File not found: {filePath}");
                    continue;
                }

                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // هيكل الـ JSON: { "questions": [ ... ] }
                var wrapper = JsonSerializer.Deserialize<JsonWrapper>(json, options);
                if (wrapper?.Questions == null || !wrapper.Questions.Any()) continue;

                foreach (var q in wrapper.Questions)
                {
                    migrationBuilder.InsertData(
                        table: "Questions",
                        columns: new[]
                        {
                            "Text", "Description", "SectionId", "MilestoneLevel",
                            "Category", "Order", "Guide"
                        },
                        values: new object[]
                        {
                            q.Text,
                            q.Description ?? "",
                            sectionId, // نستخدم SectionId المحدد
                            q.MilestoneLevel,
                            q.Category,
                          
                            q.Order,
                            q.Guide ?? ""
                        });
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // احذف كل الأسئلة من الأقسام دي
            var sectionIds = Sections.Select(s => s.SectionId).ToArray();
            migrationBuilder.DeleteData("Questions", "SectionId", sectionIds);
        }
    }

    // DTO لتحويل JSON
    public class JsonWrapper
    {
        public List<Question> Questions { get; set; } = new();
    }

    //public class Question
    //{
    //    public string Text { get; set; } = string.Empty;
    //    public string? Description { get; set; }
    //    public int MilestoneLevel { get; set; }
    //    public string Category { get; set; } = string.Empty;
    //    public string SkillType { get; set; } = string.Empty;
    //    public bool IsCritical { get; set; }
    //    public int Order { get; set; }
    //    public string? Guide { get; set; }
    //}
}