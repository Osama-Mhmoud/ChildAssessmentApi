using ChildAssessmentApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using static ChildAssessmentApi.Models.Models;

namespace ChildAssessmentApi.Services
{
    public class QuestionImportService
    {
        private readonly AssessmentDbContext _context;
        private readonly ILogger<QuestionImportService> _logger;

        public QuestionImportService(AssessmentDbContext context, ILogger<QuestionImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ImportResult> ImportQuestionsFromJsonAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new ImportResult { Success = false, Message = "الملف غير موجود" };
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var importData = JsonSerializer.Deserialize<BulkQuestionImportDto>(jsonContent);

                if (importData?.Questions == null || !importData.Questions.Any())
                {
                    return new ImportResult { Success = false, Message = "لا توجد أسئلة في الملف" };
                }

                var questions = new List<Question>();
                var errors = new List<string>();

                foreach (var questionDto in importData.Questions)
                {
                    try
                    {
                        // التحقق من صحة البيانات
                        if (string.IsNullOrWhiteSpace(questionDto.Text))
                        {
                            errors.Add("نص السؤال مطلوب");
                            continue;
                        }

                        if (!new[] { 1, 2, 3, 4, 5, 6 }.Contains(questionDto.SectionId))
                        {
                            errors.Add($"معرف القسم غير صالح للسؤال: {questionDto.Text}");
                            continue;
                        }

                        var question = new Question
                        {
                            Text = questionDto.Text.Trim(),
                            Description = questionDto.Description?.Trim(),
                            SectionId = questionDto.SectionId,
                            MilestoneLevel = questionDto.MilestoneLevel,
                            Category = questionDto.Category?.Trim(),
                            Order = questionDto.Order,
                            Guide = questionDto.Guide?.Trim(),
                            IsActive = true
                        };

                        // إضافة الخيارات إذا كانت موجودة
                        if (questionDto.Options != null && questionDto.Options.Any())
                        {
                            question.Options = questionDto.Options.Select(opt => new QuestionOption
                            {
                                Text = opt.Text.Trim(),
                                Order = opt.Order,
                                Score = opt.Score
                            }).ToList();
                        }

                        questions.Add(question);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"خطأ في معالجة السؤال '{questionDto.Text}': {ex.Message}");
                    }
                }

                if (errors.Any())
                {
                    return new ImportResult 
                    { 
                        Success = false, 
                        Message = "تم العثور على أخطاء في البيانات",
                        Errors = errors
                    };
                }

                // إضافة الأسئلة إلى قاعدة البيانات
                _context.Questions.AddRange(questions);
                await _context.SaveChangesAsync();

                return new ImportResult 
                { 
                    Success = true, 
                    Message = $"تم استيراد {questions.Count} سؤال بنجاح",
                    ImportedCount = questions.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في استيراد الأسئلة من الملف: {FilePath}", filePath);
                return new ImportResult 
                { 
                    Success = false, 
                    Message = $"خطأ في استيراد الأسئلة: {ex.Message}" 
                };
            }
        }

        public async Task<ImportResult> ImportQuestionsFromExcelAsync(string filePath)
        {
            // TODO: إضافة دعم لملفات Excel
            return new ImportResult 
            { 
                Success = false, 
                Message = "استيراد ملفات Excel غير مدعوم حالياً" 
            };
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ImportedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
