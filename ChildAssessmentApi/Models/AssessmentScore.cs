
namespace ChildAssessmentApi.Models
{
    public class AssessmentScore
    {
      
        
            public int Id { get; set; }
            public int StudentId { get; set; }
            public int AssessmentNumber { get; set; }
            public int Score { get; set; }
            public DateTime Date { get; set; }
            public string Color { get; set; } = "Green";
            public string Tester { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        
    }


    // Models/MilestoneScore.cs
    public class MilestoneScore : AssessmentScore { }

    public class BarrierScore : AssessmentScore { }


    public class TransitionScore : AssessmentScore { }



    public class TaskAnalysisScore : AssessmentScore { }




}