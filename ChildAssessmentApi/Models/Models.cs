

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
        public int SectionId { get; set; }
        public Section Section { get; set; }
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
        public int SectionId { get; set; }
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

}
}
