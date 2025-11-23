//using ChildAssessmentApi.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace ChildAssessmentApi.Controllers
//{
//    [Route("api/students/{studentId}/assessment")]
//    [ApiController]
//    public class AssesmentquesController : ControllerBase
//    {

//        private readonly AssessmentDbContext _context;

//        public AssesmentquesController(AssessmentDbContext context) => _context = context;

//        [HttpGet("questions")]
//        public async Task<ActionResult> GetQuestions(int studentId, int level = 0)
//        {
//            var query = _context.VbmappQuestions.AsQueryable();
//            if (level > 0) query = query.Where(q => q.Level == level);

//            var questions = await query.OrderBy(q => q.Order).ToListAsync();

//            // جلب إجابات الطفل لو موجودة
//            var answers = await _context.StudentAnswers
//                .Where(a => a.StudentId == studentId && a.Id == 1)
//                .ToDictionaryAsync(a => a.QuestionId);

//            var result = questions.Select(q => new
//            {
//                q.Id,
//                q.Level,
//                q.Area,
//                q.Milestone,
//                q.QuestionText,
//                q.Example,
//                Score = answers.ContainsKey(q.Id) ? answers[q.Id].Score : (int?)null,
//                Notes = answers.ContainsKey(q.Id) ? answers[q.Id].Notes : null
//            });

//            return Ok(result);
//        }

//        [HttpPost("answer")]
//        public async Task<ActionResult> SaveAnswer(int studentId, [FromBody] SaveAnswerDto dto)
//        {
//            var answer = await _context.StudentAnswers
//                .FirstOrDefaultAsync(a => a.StudentId == studentId &&
//                                        a.QuestionId == dto.QuestionId &&
//                                        a.AssessmentNumber == 1);

//            if (answer == null)
//            {
//                answer = new StudentAnswers
//                {
//                    StudentId = studentId,
//                    QuestionId = dto.QuestionId,
//                    AssessmentNumber = 1
//                };
//                _context.StudentAnswers.Add(answer);
//            }

//            answer.Score = dto.Score;
//            answer.Notes = dto.Notes;
//            answer.AnsweredAt = DateTime.Now;

//            await _context.SaveChangesAsync();
//            return Ok();
//        }
//    }

//    public class SaveAnswerDto
//    {
//        public int QuestionId { get; set; }
//        public int Score { get; set; } // 0, 1, 2
//        public string Notes { get; set; } = string.Empty;



//    }
//}
