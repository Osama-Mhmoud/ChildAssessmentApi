using ChildAssessmentApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ChildAssessmentApi.Models.Models;

[Route("api/[controller]")]
[ApiController]
public class AssessmentsController : ControllerBase
{
    private readonly AssessmentDbContext _context;

    public AssessmentsController(AssessmentDbContext context)
    {
        _context = context;
    }

    [HttpGet("sections")]
    public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections()
    {
        var sections = await _context.Sections
            .Include(s => s.Questions)
            .Select(s => new SectionDto
            {
                Id = s.Id,
                Name = s.Name,
                Questions = s.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    SectionId = q.SectionId
                }).ToList()
            })
            .ToListAsync();
        return Ok(sections);
    }

    [HttpPost("questions")]
    public async Task<ActionResult<QuestionDto>> AddQuestion([FromBody] QuestionDto questionDto)
    {
        if (!new[] { 1, 2, 3, 4, 5, 6 }.Contains(questionDto.SectionId))
            return BadRequest("معرف القسم غير صالح. يجب أن يكون بين 1 و6.");

        var question = new Question
        {
            Text = questionDto.Text,
            SectionId = questionDto.SectionId
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        questionDto.Id = question.Id;
        return CreatedAtAction(nameof(GetSections), new { id = question.Id }, questionDto);
    }

    [HttpPost("children")]
    public async Task<ActionResult<ChildDto>> CreateChild([FromBody] ChildDto childDto)
    {
        var child = new Child
        {
            Name = childDto.Name,
            DateOfBirth = childDto.DateOfBirth
        };

        _context.Children.Add(child);
        await _context.SaveChangesAsync();

        childDto.Id = child.Id;
        return CreatedAtAction(nameof(GetChild), new { id = child.Id }, childDto);
    }

    [HttpGet("children/{id}")]
    public async Task<ActionResult<ChildDto>> GetChild(int id)
    {
        var child = await _context.Children.FindAsync(id);
        if (child == null)
            return NotFound();

        return Ok(new ChildDto
        {
            Id = child.Id,
            Name = child.Name,
            DateOfBirth = child.DateOfBirth
        });
    }

    [HttpPost("assessments")]
    public async Task<ActionResult<AssessmentDto>> CreateAssessment([FromBody] AssessmentDto assessmentDto)
    {
        if (!await _context.Children.AnyAsync(c => c.Id == assessmentDto.ChildId))
            return BadRequest("معرف الطفل غير صالح.");

        foreach (var answer in assessmentDto.Answers)
        {
            if (!new[] { 0.0, 0.5, 1.0 }.Contains(answer.Score))
                return BadRequest("الدرجة غير صالحة. يجب أن تكون 0، 0.5، أو 1.");
            if (!await _context.Questions.AnyAsync(q => q.Id == answer.QuestionId))
                return BadRequest($"معرف السؤال غير صالح: {answer.QuestionId}.");
        }

        var assessment = new Assessment
        {
            ChildId = assessmentDto.ChildId,
            Date = assessmentDto.Date,
            Answers = assessmentDto.Answers.Select(a => new Answer
            {
                QuestionId = a.QuestionId,
                Score = a.Score
            }).ToList()
        };

        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        assessmentDto.Id = assessment.Id;
        return CreatedAtAction(nameof(GetAssessment), new { id = assessment.Id }, assessmentDto);
    }

    [HttpGet("assessments/{id}")]
    public async Task<ActionResult<AssessmentDto>> GetAssessment(int id)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment == null)
            return NotFound();

        return Ok(new AssessmentDto
        {
            Id = assessment.Id,
            ChildId = assessment.ChildId,
            Date = assessment.Date,
            Answers = assessment.Answers.Select(a => new AnswerDto
            {
                QuestionId = a.QuestionId,
                Score = a.Score
            }).ToList()
        });
    }

    [HttpGet("assessments/{id}/result")]
    public async Task<ActionResult<AssessmentResultDto>> GetAssessmentResult(int id)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .ThenInclude(q => q.Section)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment == null)
            return NotFound();

        var sectionScores = assessment.Answers
            .GroupBy(ans => ans.Question.Section.Name)
            .ToDictionary(g => g.Key, g => g.Sum(ans => ans.Score));

        var result = new AssessmentResultDto
        {
            AssessmentId = assessment.Id,
            TotalScore = assessment.Answers.Sum(ans => ans.Score),
            SectionScores = sectionScores
        };

        return Ok(result);
    }
}