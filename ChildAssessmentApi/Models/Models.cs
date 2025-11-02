

using System;
using System.Collections.Generic;



namespace ChildAssessmentApi.Models
{
    public class Models
    {


public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string? Description { get; set; }
        public int SectionId { get; set; }
        public Section Section { get; set; }
        public int? MilestoneLevel { get; set; } // VBMAPP milestone level (0-48 months)
        public string? Category { get; set; } // VBMAPP category
        public int Order { get; set; } // Order within section
        public bool IsActive { get; set; } = true;
        public string? Guide { get; set; } // Guide text for Barriers questions
        public List<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }

    public class QuestionOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public string Text { get; set; }
        public int Order { get; set; } // Order of the option (0-4 for Barriers questions)
        public int Score { get; set; } // Score value (0-4 for Barriers questions)
    }

    public class Child
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class Assessment
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public Child Child { get; set; }
        public DateTime Date { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }

    public class Answer
    {
        public int Id { get; set; }
        public int AssessmentId { get; set; }
        public Assessment Assessment { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public double Score { get; set; } // 0, 0.5, or 1
    }

    public class SectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string? Description { get; set; }
        public int SectionId { get; set; }
        public int? MilestoneLevel { get; set; }
        public string? Category { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public string? Guide { get; set; }
        public List<QuestionOptionDto> Options { get; set; } = new List<QuestionOptionDto>();
    }

    public class QuestionOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
        public int Score { get; set; }
    }

    public class ChildDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class AssessmentDto
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public DateTime Date { get; set; }
        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public double Score { get; set; }
    }

    public class AssessmentResultDto
    {
        public int AssessmentId { get; set; }
        public double TotalScore { get; set; }
        public Dictionary<string, double> SectionScores { get; set; } = new Dictionary<string, double>();
    }

    // VBMAPP specific models
    public class VbmappQuestionImportDto
    {
        public string Text { get; set; }
        public string? Description { get; set; }
        public int SectionId { get; set; }
        public int? MilestoneLevel { get; set; }
        public string? Category { get; set; }
        public int Order { get; set; }
        public string? Guide { get; set; }
        public List<VbmappQuestionOptionDto> Options { get; set; } = new List<VbmappQuestionOptionDto>();
    }

    public class VbmappQuestionOptionDto
    {
        public string Text { get; set; }
        public int Order { get; set; }
        public int Score { get; set; }
    }

    public class BulkQuestionImportDto
    {
        public List<VbmappQuestionImportDto> Questions { get; set; } = new List<VbmappQuestionImportDto>();
    }

    public class BarriersQuestionImportDto
    {
        public string Text { get; set; }
        public int SectionId { get; set; }
        public int Order { get; set; }
        public string? Guide { get; set; }
        public List<string> Options { get; set; } = new List<string>(); // 5 options in order (0-4)
    }

    public class QuestionFilterDto
    {
        public int? SectionId { get; set; }
        public int? MilestoneLevel { get; set; }
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
    }

}
}
