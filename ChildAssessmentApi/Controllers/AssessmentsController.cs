using ChildAssessmentApi.Models;
using ChildAssessmentApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
    private readonly QuestionImportService _importService;

    public AssessmentsController(AssessmentDbContext context, QuestionImportService importService)
    {
        _context = context;
        _importService = importService;
        ExcelPackage.License.SetNonCommercialPersonal("Osama");
    }

    [HttpGet("sections")]
    public async Task<ActionResult<IEnumerable<SectionDto>>> GetSections()
    {
        var sections = await _context.Sections
            .Include(s => s.Questions.Where(q => q.IsActive))
            .Select(s => new SectionDto
            {
                Id = s.Id,
                Name = s.Name,
                Questions = s.Questions.Where(q => q.IsActive).OrderBy(q => q.Order).Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Description = q.Description,
                    SectionId = q.SectionId,
                    MilestoneLevel = q.MilestoneLevel,
                    Category = q.Category,
                    Order = q.Order,
                    IsActive = q.IsActive
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
            Description = questionDto.Description,
            SectionId = questionDto.SectionId,
            MilestoneLevel = questionDto.MilestoneLevel,
            Category = questionDto.Category,
            Order = questionDto.Order,
            IsActive = questionDto.IsActive
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

    // New VBMAPP endpoints
    [HttpGet("questions")]
    public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions([FromQuery] QuestionFilterDto filter)
    {
        var query = _context.Questions.AsQueryable();

        if (filter.SectionId.HasValue)
            query = query.Where(q => q.SectionId == filter.SectionId.Value);

        if (filter.MilestoneLevel.HasValue)
            query = query.Where(q => q.MilestoneLevel == filter.MilestoneLevel.Value);

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(q => q.Category == filter.Category);

        if (filter.IsActive.HasValue)
            query = query.Where(q => q.IsActive == filter.IsActive.Value);

        var questions = await query
            .OrderBy(q => q.SectionId)
            .ThenBy(q => q.Order)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                Description = q.Description,
                SectionId = q.SectionId,
                MilestoneLevel = q.MilestoneLevel,
                Category = q.Category,
                Order = q.Order,
                IsActive = q.IsActive
            })
            .ToListAsync();

        return Ok(questions);
    }

    [HttpPost("questions/bulk-import")]
    public async Task<ActionResult<BulkQuestionImportDto>> BulkImportQuestions([FromBody] BulkQuestionImportDto importDto)
    {
        var questions = new List<Question>();
        var errors = new List<string>();

        foreach (var questionDto in importDto.Questions)
        {
            if (!new[] { 1, 2, 3, 4, 5, 6 }.Contains(questionDto.SectionId))
            {
                errors.Add($"معرف القسم غير صالح للسؤال: {questionDto.Text}");
                continue;
            }

            var question = new Question
            {
                Text = questionDto.Text,
                Description = questionDto.Description,
                SectionId = questionDto.SectionId,
                MilestoneLevel = questionDto.MilestoneLevel,
                Category = questionDto.Category,
                Order = questionDto.Order,
                Guide = questionDto.Guide,
                IsActive = true
            };

            // إضافة الخيارات إذا كانت موجودة
            if (questionDto.Options != null && questionDto.Options.Any())
            {
                question.Options = questionDto.Options.Select(opt => new QuestionOption
                {
                    Text = opt.Text,
                    Order = opt.Order,
                    Score = opt.Score
                }).ToList();
            }

            questions.Add(question);
        }

        if (errors.Any())
        {
            return BadRequest(new { errors });
        }

        _context.Questions.AddRange(questions);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"تم استيراد {questions.Count} سؤال بنجاح" });
    }

    [HttpPut("questions/{id}")]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(int id, [FromBody] QuestionDto questionDto)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound();

        question.Text = questionDto.Text;
        question.Description = questionDto.Description;
        question.MilestoneLevel = questionDto.MilestoneLevel;
        question.Category = questionDto.Category;
        question.Order = questionDto.Order;
        question.IsActive = questionDto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(questionDto);
    }

    [HttpDelete("questions/{id}")]
    public async Task<ActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound();

        question.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("questions/categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetQuestionCategories()
    {
        var categories = await _context.Questions
            .Where(q => !string.IsNullOrEmpty(q.Category))
            .Select(q => q.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("questions/milestone-levels")]
    public async Task<ActionResult<IEnumerable<int>>> GetMilestoneLevels()
    {
        var levels = await _context.Questions
            .Where(q => q.MilestoneLevel.HasValue)
            .Select(q => q.MilestoneLevel.Value)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();

        return Ok(levels);
    }

    [HttpPost("questions/import-from-file")]
    public async Task<ActionResult<ImportResult>> ImportQuestionsFromFile(string filePath)
    {
        var result = new ImportResult
        {
            Success = false,
            Message = "",
            ImportedCount = 0,
            Errors = new List<string>()
        };

        if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
        {
            result.Message = "File path is invalid or file does not exist.";
            result.Errors.Add(result.Message);
            return BadRequest(result);
        }

        List<Models.Question> questions = new List<Models.Question>();
        try
        {
            await Task.Run(() =>
            {
                using (var excelPackage = new OfficeOpenXml.ExcelPackage(new System.IO.FileInfo(filePath)))
                {
                    var worksheet = excelPackage.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var text = worksheet.Cells[row, 1].Text;
                            var description = worksheet.Cells[row, 2].Text;
                            var sectionId = int.Parse(worksheet.Cells[row, 3].Text);
                            var milestoneLevelText = worksheet.Cells[row, 4].Text;
                            int? milestoneLevel = string.IsNullOrEmpty(milestoneLevelText) ? (int?)null : int.Parse(milestoneLevelText);
                            var category = worksheet.Cells[row, 5].Text;
                            var order = int.Parse(worksheet.Cells[row, 6].Text);

                            if (!new[] { 1, 2, 3, 4, 5, 6 }.Contains(sectionId))
                            {
                                result.Errors.Add($"Invalid section ID at row {row}: {sectionId}");
                                continue;
                            }

                            questions.Add(new Models.Question
                            {
                                Text = text,
                                Description = description,
                                SectionId = sectionId,
                                MilestoneLevel = milestoneLevel,
                                Category = category,
                                Order = order,
                                IsActive = true
                            });
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Error parsing row {row}: {ex.Message}");
                        }
                    }
                }
            });

            if (result.Errors.Any())
            {
                result.Message = "Some rows had errors and were not imported.";
            }

            if (questions.Any())
            {
                _context.Questions.AddRange(questions);
                await _context.SaveChangesAsync();
                result.Success = true;
                result.ImportedCount = questions.Count;
                result.Message = $"Successfully imported {questions.Count} questions.";
                return Ok(result);
            }
            else
            {
                result.Message = "No valid questions to import.";
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(result.Message);
            return BadRequest(result);
        }
    }

    [HttpPost("questions/import-sample")]
    public async Task<ActionResult<ImportResult>> ImportSampleQuestions()
    {
        var sampleFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "vbmapp_questions.json");
        var result = await _importService.ImportQuestionsFromJsonAsync(sampleFilePath);
        
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}