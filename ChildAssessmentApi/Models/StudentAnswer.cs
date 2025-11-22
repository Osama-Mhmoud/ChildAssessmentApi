namespace ChildAssessmentApi.Models
{
    // Models/StudentAnswer.cs
    public class StudentAnswer
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int QuestionId { get; set; }
        public int Score { get; set; }     // 0, 0.5, 1, 2 حسب التقييم
        public string Notes { get; set; }
        public DateTime AnsweredAt { get; set; }
        public VbmappQuestion Question { get; set; }
    }
}
