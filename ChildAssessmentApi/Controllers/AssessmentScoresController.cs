// Controllers/AssessmentScoresController.cs
using ChildAssessmentApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Route("api/students/{studentId}/milestones")]
public class MilestonesController : ControllerBase
{
    private readonly AssessmentDbContext _context;
    public MilestonesController(AssessmentDbContext context) => _context = context;

    [HttpGet("Milestone")]
    public async Task<ActionResult<List<MilestoneScore>>> GetMilestone(int studentId)
        => await _context.MilestoneScores.Where(s => s.StudentId == studentId).ToListAsync();
    [HttpGet("Barriers")]
    public async Task<ActionResult<List<BarrierScore>>> GetBarriers(int studentId)
        => await _context.BarrierScores.Where(s => s.StudentId == studentId).ToListAsync();
    [HttpGet("Transitions")]
    public async Task<ActionResult<List<TransitionScore>>> GetTransitions(int studentId)
        => await _context.TransitionScores.Where(s => s.StudentId == studentId).ToListAsync();
    [HttpGet("TaskAnalysis")]
    public async Task<ActionResult<List<TaskAnalysisScore>>> GetTaskAnalysis(int studentId)
        => await _context.TaskAnalysisScores.Where(s => s.StudentId == studentId).ToListAsync();

    [HttpPost]
    public async Task<ActionResult> Save(int studentId, [FromBody] MilestoneScore score)
    {
        score.StudentId = studentId;
        score.CreatedAt = DateTime.Now;
        _context.MilestoneScores.Add(score);
        await _context.SaveChangesAsync();
        return Ok();
    }
}