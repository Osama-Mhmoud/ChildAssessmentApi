namespace ChildAssessmentApi.Models
{
    // Models/VbmappQuestion.cs
    public class VbmappQuestion
    {
        public int Id { get; set; }
        public string Section { get; set; }      // "Milestones", "Barriers", "Transitions"
        public int Level { get; set; }           // 1, 2, 3
        public string Area { get; set; }         // "Mand", "Tact", ...
        public string Milestone { get; set; }    // "5-a", "10-M", "15-e"
        public string QuestionText { get; set; } // النص الكامل للسؤال
        public string Example { get; set; }      // مثال (اختياري)
        public int Order { get; set; }
    }
}
