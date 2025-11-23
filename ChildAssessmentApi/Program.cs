//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast = Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");

//app.Run();

//internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}


using ChildAssessmentApi.Models;
using ChildAssessmentApi.Services;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static ChildAssessmentApi.Models.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<QuestionImportService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();


// ??????? ??????? ?? ??????? ??????? (milestones_questions.json ??????)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AssessmentDbContext>();
    var webRoot = app.Environment.WebRootPath;
    var dataPath = Path.Combine(webRoot, "data");

    var files = new[]
    {
        "milestones_questions.json",
        "barriers_questions.json",
        "transitions_questions.json",
        "task_analysis_questions.json",
        "eesa_questions.json"
    };

    int totalImported = 0;

    foreach (var file in files)
    {
        var fullPath = Path.Combine(dataPath, file);
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"?????: ????? ??? ????? ? {fullPath}");
            continue;
        }

        var json = await File.ReadAllTextAsync(fullPath);

        // ??? Deserialize ??? ???? ?????? ?? ??? ?? ???
        var questions = JsonSerializer.Deserialize<List<VbmappQuestionDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (questions == null || !questions.Any()) continue;

        foreach (var q in questions)
        {
            var question = new Question
            {
                Text = q.Text ?? q.Question ?? q.Description ?? "??? ????",
                SectionId = MapAreaToSectionId(q.Area ?? q.Category ?? ""),
                MilestoneLevel = q.Level ?? 1,
                Category = q.Area ?? q.Category ?? q.QuestionNumber,
                Order = q.Order 
                    ?? (int.TryParse(q.QuestionNumber, out var questionNumberInt) ? questionNumberInt : totalImported),
                IsActive = true
            };

            // ???? ???????
            if (!context.Questions.Any(x => x.Text == question.Text && x.SectionId == question.SectionId))
            {
                context.Questions.Add(question);
            }
        }

        totalImported += questions.Count;
        Console.WriteLine($"?? ??????? {questions.Count} ???? ?? ? {file}");
    }

    if (totalImported > 0)
    {
        await context.SaveChangesAsync();
        Console.WriteLine($"?? ????? ??????? {totalImported} ???? ??????! ??? PDF ???? 100%");
    }
    else
    {
        Console.WriteLine("?? ??? ??????? ?? ????? ????? (???? ???? ?????? ?????)");
    }
}

Console.WriteLine("???????? ???? ?????? - ??? Generate PDF ??????");



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
