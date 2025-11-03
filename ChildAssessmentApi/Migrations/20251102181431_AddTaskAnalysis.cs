using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using System.Text.Json;
using static ChildAssessmentApi.Models.Models;

namespace ChildAssessmentApi.Migrations
{
    public partial class AddTaskAnalysis : Migration
    {
        private const int SectionId = 6;
        private const string FileName = "task_analysis_questions.json";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", FileName);

            if (!File.Exists(filePath))
            {
                migrationBuilder.Sql($"-- WARNING: File not found: {filePath}");
                return;
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var wrapper = JsonSerializer.Deserialize<JsonWrapper>(json, options);

            if (wrapper?.Questions == null || !wrapper.Questions.Any()) return;

            foreach (var q in wrapper.Questions)
            {
                migrationBuilder.InsertData(
                    table: "Questions",
                    columns: new[]
                    {
                        "Text",
                        "Description",
                        "SectionId",
                        "MilestoneLevel",
                        "Category",
                        "Order",
                        "Guide"
                    },
                    values: new object[]
                    {
                        q.Text,
                        q.Description ?? string.Empty,
                        SectionId,
                        q.MilestoneLevel,
                        q.Category,
                        q.Order,
                        q.Guide ?? string.Empty
                    });
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // الحل الصحيح: استخدم SQL مباشرة
            migrationBuilder.Sql("DELETE FROM Questions WHERE SectionId = 6");
        }

        private class JsonWrapper
        {
            public List<Question> Questions { get; set; } = new();
        }

        //private class Question
        //{
        //    public string Text { get; set; } = string.Empty;
        //    public string? Description { get; set; }
        //    public int MilestoneLevel { get; set; }
        //    public string Category { get; set; } = string.Empty;
        //    public int Order { get; set; }
        //    public string? Guide { get; set; }
        //}
    }
}