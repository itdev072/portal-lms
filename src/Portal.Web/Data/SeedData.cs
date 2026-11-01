using Microsoft.AspNetCore.Identity;
using Portal.Web.Models.Domain;

namespace Portal.Web.Data;

/// <summary>
/// Seeds just enough data for the Portal to be usable the first time it runs: one
/// account per role, one class, CSK1's content (lessons/quiz/challenge/case-study —
/// shaped exactly like the Phase 1 content master prompt's JSON so real CSK1 output
/// can replace this later), a campus location, and a sample midterm.
/// Demo logins (see console output on first run / README): admin@portal.local,
/// teacher@portal.local, student@portal.local — all password "Passw0rd!".
/// </summary>
public static class SeedData
{
    public static void EnsureSeeded(AppData data)
    {
        var hasher = new PasswordHasher<ApplicationUser>();

        var admin = new ApplicationUser { Id = "admin-1", Email = "admin@portal.local", DisplayName = "Admin", Role = UserRole.Admin };
        admin.PasswordHash = hasher.HashPassword(admin, "Passw0rd!");

        var teacher = new ApplicationUser { Id = "teacher-1", Email = "teacher@portal.local", DisplayName = "Kouet", Role = UserRole.Teacher };
        teacher.PasswordHash = hasher.HashPassword(teacher, "Passw0rd!");

        var student = new ApplicationUser { Id = "student-1", Email = "student@portal.local", DisplayName = "Sok Dara", Role = UserRole.Student };
        student.PasswordHash = hasher.HashPassword(student, "Passw0rd!");

        var usersSeeded = data.Users.SeedIfEmpty(new[] { admin, teacher, student });

        var section = new ClassSection { Id = "section-1", Name = "SPD Year 2 - Section A", TeacherId = teacher.Id, SemesterLabel = "Semester 1" };
        data.ClassSections.SeedIfEmpty(new[] { section });

        data.Enrollments.SeedIfEmpty(new[]
        {
            new Enrollment { StudentId = student.Id, ClassSectionId = section.Id }
        });

        var csk1 = new Level
        {
            Id = "CSK1",
            Order = 1,
            NameKm = "កម្រិតដំបូង ១ — មូលដ្ឋាន C#",
            NameEn = "Beginner I - C# Basics",
            SourceChapters = new List<int> { 1, 2, 3, 4 },
            Prerequisite = null,
            EstimatedHours = 12,
            DescriptionKm = "ស្គាល់ភាសា C#, រចនាសម្ព័ន្ធកម្មវិធី, អថេរ, និងប្រមាណវិធីជាមូលដ្ឋាន"
        };
        var csk2 = new Level
        {
            Id = "CSK2",
            Order = 2,
            NameKm = "កម្រិតដំបូង ២ — តក្កវិជ្ជា និងរចនាសម្ព័ន្ធបញ្ជា",
            NameEn = "Beginner II - Control Flow",
            SourceChapters = new List<int> { 5, 6, 7, 8, 9, 10 },
            Prerequisite = "CSK1",
            EstimatedHours = 18,
            DescriptionKm = "លក្ខខណ្ឌ, រង្វិលជុំ, មេតូដ, អារេ, ខ្សែអក្សរ, កំហុសកម្មវិធី"
        };
        data.Levels.SeedIfEmpty(new[] { csk1, csk2 });

        var lesson1 = new Lesson
        {
            Id = "CSK1-L01",
            LevelId = "CSK1",
            Order = 1,
            TitleKm = "សេចក្តីផ្តើមអំពី C# និង .NET",
            TitleEn = "Introduction to C# and .NET",
            KeyTerms = new List<KeyTerm> { new() { Km = "អថេរ", En = "variable" }, new() { Km = "កម្មវិធី", En = "program" } },
            ExplainerMd = "C# ជាភាសាសរសេរកម្មវិធីទំនើបមួយ រត់នៅលើវេទិកា .NET។ កម្មវិធី C# ដំបូងបំផុតបោះពុម្ពអក្សរចេញតាម Console។",
            CodeExamples = new List<CodeExample>
            {
                new() { Caption = "Hello World", Code = "using System;\n\nConsole.WriteLine(\"Hello, World!\");" }
            },
            XpValue = 10,
            EstimatedMinutes = 15
        };
        var lesson2 = new Lesson
        {
            Id = "CSK1-L02",
            LevelId = "CSK1",
            Order = 2,
            TitleKm = "អថេរ និងប្រភេទទិន្នន័យ",
            TitleEn = "Variables and Data Types",
            KeyTerms = new List<KeyTerm> { new() { Km = "ប្រភេទទិន្នន័យ", En = "data type" }, new() { Km = "ចំនួនគត់", En = "integer" } },
            ExplainerMd = "អថេរគឺជាកន្លែងផ្ទុកទិន្នន័យដែលមានឈ្មោះ។ C# មានប្រភេទទិន្នន័យដូចជា int, double, string, bool។",
            CodeExamples = new List<CodeExample>
            {
                new() { Caption = "Declaring variables", Code = "int age = 20;\nstring name = \"Dara\";\nConsole.WriteLine($\"{name} is {age} years old\");" }
            },
            XpValue = 10,
            EstimatedMinutes = 20
        };
        data.Lessons.SeedIfEmpty(new[] { lesson1, lesson2 });

        var quiz1 = new QuizItem
        {
            Id = "CSK1-Q01",
            LevelId = "CSK1",
            LessonId = lesson1.Id,
            Type = QuizType.MultipleChoice,
            QuestionKm = "តើមេតូដណាដែលប្រើដើម្បីបោះពុម្ពអក្សរចេញតាម Console ជាមួយការចុះបន្ទាត់ថ្មី?",
            Options = new List<string> { "Console.Write", "Console.WriteLine", "Console.Print", "Console.Read" },
            CorrectIndex = 1,
            ExplanationKm = "Console.WriteLine បោះពុម្ពអត្ថបទ ហើយបន្ថែមការចុះបន្ទាត់ថ្មីដោយស្វ័យប្រវត្តិ។",
            Difficulty = Difficulty.Easy,
            XpValue = 5
        };
        var quiz2 = new QuizItem
        {
            Id = "CSK1-Q02",
            LevelId = "CSK1",
            LessonId = lesson2.Id,
            Type = QuizType.TrueFalse,
            QuestionKm = "ត្រូវ ឬ ខុស៖ \"int\" អាចផ្ទុកលេខទសភាគបាន (ឧទាហរណ៍ 3.14)។",
            Options = new List<string> { "ត្រូវ", "ខុស" },
            CorrectIndex = 1,
            ExplanationKm = "int ផ្ទុកបានតែចំនួនគត់ប៉ុណ្ណោះ។ សម្រាប់លេខទសភាគ ត្រូវប្រើ double ឬ float។",
            Difficulty = Difficulty.Easy,
            XpValue = 5
        };
        data.QuizItems.SeedIfEmpty(new[] { quiz1, quiz2 });

        var challenge1 = new CodingChallenge
        {
            Id = "CSK1-C01",
            LevelId = "CSK1",
            LessonId = lesson2.Id,
            PromptKm = "សូមបំពេញកូដខាងក្រោម ដើម្បីបោះពុម្ពឈ្មោះសិស្សម្នាក់ (ប្រើអថេរ name)",
            StarterCode = "string name = \"Dara\";\nConsole.WriteLine(___);",
            SolutionCode = "string name = \"Dara\";\nConsole.WriteLine(name);",
            TestCases = new List<TestCase> { new() { Input = "", ExpectedOutput = "Dara" } },
            Difficulty = Difficulty.Easy,
            XpValue = 15
        };
        data.CodingChallenges.SeedIfEmpty(new[] { challenge1 });

        var caseStudy1 = new CaseStudyTask
        {
            Id = "CSK1-CS01",
            LevelId = "CSK1",
            PromptKm = "ប្រកាសអថេរបីសម្រាប់ប្រព័ន្ធគ្រប់គ្រងសិស្ស៖ ឈ្មោះសិស្ស (string), អាយុ (int), និងពិន្ទុមធ្យម (double)។ បោះពុម្ពតម្លៃទាំងអស់។",
            SolutionNotesKm = "string studentName; int age; double averageScore; — បោះពុម្ពជាមួយ Console.WriteLine។",
            XpValue = 20
        };
        data.CaseStudyTasks.SeedIfEmpty(new[] { caseStudy1 });

        var campus = new CampusLocation
        {
            Id = "campus-main",
            Name = "Main Campus - Building A",
            Latitude = 11.5564,
            Longitude = 104.9282,
            RadiusMeters = 120
        };
        data.CampusLocations.SeedIfEmpty(new[] { campus });

        var exam = new MidtermExam
        {
            Id = "exam-csk1-midterm",
            ClassSectionId = section.Id,
            Title = "Midterm Exam - CSK1 Basics",
            LevelId = "CSK1",
            TimeLimitMinutes = 45,
            OpensAt = DateTimeOffset.UtcNow.AddDays(-1),
            ClosesAt = DateTimeOffset.UtcNow.AddDays(30),
            IsPublished = true
        };
        data.Exams.SeedIfEmpty(new[] { exam });

        data.ExamQuestions.SeedIfEmpty(new[]
        {
            new ExamQuestion { Id = "exq-1", ExamId = exam.Id, Order = 1, Type = QuizType.MultipleChoice,
                QuestionKm = "តើប្រភេទទិន្នន័យណាសមស្របបំផុតសម្រាប់អាយុសិស្ស?",
                Options = new List<string> { "string", "int", "bool", "double" }, CorrectIndex = 1, Points = 10 },
            new ExamQuestion { Id = "exq-2", ExamId = exam.Id, Order = 2, Type = QuizType.TrueFalse,
                QuestionKm = "ត្រូវ ឬ ខុស៖ Console.WriteLine អាចបោះពុម្ពទាំងអត្ថបទ និងលេខ។",
                Options = new List<string> { "ត្រូវ", "ខុស" }, CorrectIndex = 0, Points = 10 }
        });

        if (usersSeeded)
        {
            Console.WriteLine("Seeded demo accounts (password: Passw0rd!): admin@portal.local, teacher@portal.local, student@portal.local");
        }
    }
}
