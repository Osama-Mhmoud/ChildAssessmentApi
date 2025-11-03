using Microsoft.EntityFrameworkCore.Migrations;
using System.Text.Json;
using System.IO;
using static ChildAssessmentApi.Models.Models;

namespace ChildAssessmentApi.Migrations
{
    public partial class AddTransitionsSectionAndQuestions : Migration
    {
        private const int SectionId = 4;
        private const string FileName = "transitions_questions.json";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", FileName);

            if (!File.Exists(filePath))
            {

                migrationBuilder.Sql($"-- WARNING: File not found: {filePath}");
                return;
            }
            try
            {

                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var wrapper = JsonSerializer.Deserialize<JsonWrapper>(json, options);
                if (wrapper?.Questions == null || !wrapper.Questions.Any()) return;

                foreach (var q in wrapper.Questions)
                {
                    // 1. أضف السؤال
                    migrationBuilder.InsertData(
                        table: "Questions",
                        columns: new[]
                        {
                        "Text", "Description", "SectionId", "Order", "Guide"
                        },
                        values: new object[]
                        {
                        q.Text,
                        string.Empty, // Description مش موجود
                        SectionId,
                        q.Order,
                        q.Guide ?? string.Empty,
                        
                        });
                    // 2. أضف الخيارات (Options)
                    if (q.Options != null && q.Options.Any())
                    {
                        foreach (var opt in q.Options)
                        {
                            migrationBuilder.InsertData(
                                table: "QuestionOptions",
                                columns: new[]
                                {
                                "QuestionId", "Text", "Order", "Score"
                                },
                                values: new object[]
                                {
                                q.Order,
                                opt.Text,
                                opt.Order,
                                opt.Score
                                });
                        }
                    }

                }
            }

            catch (System.Exception ex)
            {
                migrationBuilder.Sql($"-- ERROR: Exception occurred while reading file: {ex.Message}");
            }

            
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // احذف الخيارات أولاً
            migrationBuilder.Sql(@"
                DELETE FROM QuestionOptions 
                WHERE QuestionText IN (
                    SELECT Text FROM Questions WHERE SectionId = 4
                );
            ");

            // ثم احذف الأسئلة
            migrationBuilder.Sql("DELETE FROM Questions WHERE SectionId = 4");
        }

        // DTO لتحويل JSON
        private class JsonWrapper
        {
            public List<Question> Questions { get; set; } = new();
        }

        //private class Question
        //{
        //    public string Text { get; set; } = string.Empty;
        //    public int Order { get; set; }
        //    public string? Guide { get; set; }
        //    public List<Option>? Options { get; set; }
        //}

        private class Option
        {
            public string Text { get; set; } = string.Empty;
            public int Order { get; set; }
            public int Score { get; set; }
        }
    }
}