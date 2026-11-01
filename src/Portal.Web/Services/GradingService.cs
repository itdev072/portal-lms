using Portal.Web.Data;
using Portal.Web.Models.Domain;

namespace Portal.Web.Services;

/// <summary>
/// Grades quiz items, coding challenges, and exam submissions, and records the resulting
/// Attempt / StudentProgress / ExamAttempt rows. Coding challenges are graded by matching
/// the student's output against each TestCase's ExpectedOutput (see the spec's Non-Goals:
/// no sandboxed code execution in v1) — see README for how a later version can swap in
/// real compilation.
/// </summary>
public class GradingService
{
    private readonly AppData _data;

    public GradingService(AppData data)
    {
        _data = data;
    }

    public Attempt GradeQuiz(string studentId, QuizItem quiz, int selectedIndex)
    {
        var isCorrect = selectedIndex == quiz.CorrectIndex;
        var attempt = new Attempt
        {
            StudentId = studentId,
            ItemId = quiz.Id,
            ItemType = AttemptItemType.Quiz,
            SubmittedAnswer = selectedIndex.ToString(),
            IsCorrect = isCorrect,
            XpAwarded = isCorrect ? quiz.XpValue : 0
        };
        _data.Attempts.Upsert(attempt);
        if (isCorrect) AwardXp(studentId, quiz.LevelId, attempt.XpAwarded);
        return attempt;
    }

    /// <summary>
    /// Text-output matching against each test case, trimming trailing whitespace before
    /// comparing (see the content master prompt's Auto-Grading Notes). All test cases
    /// must pass for the challenge to count as correct.
    /// </summary>
    public (bool allPassed, List<bool> perTestCase) GradeChallenge(string studentId, CodingChallenge challenge, string submittedOutput)
    {
        var results = challenge.TestCases
            .Select(tc => string.Equals(
                tc.ExpectedOutput.TrimEnd(),
                submittedOutput.TrimEnd(),
                StringComparison.Ordinal))
            .ToList();

        var allPassed = results.Count > 0 && results.All(r => r);

        var attempt = new Attempt
        {
            StudentId = studentId,
            ItemId = challenge.Id,
            ItemType = AttemptItemType.Challenge,
            SubmittedAnswer = submittedOutput,
            IsCorrect = allPassed,
            XpAwarded = allPassed ? challenge.XpValue : 0
        };
        _data.Attempts.Upsert(attempt);
        if (allPassed) AwardXp(studentId, challenge.LevelId, attempt.XpAwarded);

        return (allPassed, results);
    }

    public ExamAttempt GradeExam(ExamAttempt attempt, List<ExamQuestion> questions)
    {
        var score = 0;
        var maxScore = 0;
        foreach (var q in questions)
        {
            maxScore += q.Points;
            var answer = attempt.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
            if (answer != null && answer.SelectedIndex == q.CorrectIndex)
                score += q.Points;
        }
        attempt.Score = score;
        attempt.MaxScore = maxScore;
        attempt.SubmittedAt = DateTimeOffset.UtcNow;
        _data.ExamAttempts.Upsert(attempt);
        return attempt;
    }

    private void AwardXp(string studentId, string levelId, int xp)
    {
        var existing = _data.Progress.Where(p => p.StudentId == studentId && p.LevelId == levelId && p.LessonId == null)
            .FirstOrDefault();

        if (existing == null)
        {
            existing = new StudentProgress
            {
                StudentId = studentId,
                LevelId = levelId,
                LessonId = null,
                Status = ProgressStatus.InProgress,
                XpEarned = xp
            };
        }
        else
        {
            existing.XpEarned += xp;
            existing.Status = ProgressStatus.InProgress;
        }
        _data.Progress.Upsert(existing);
    }
}
