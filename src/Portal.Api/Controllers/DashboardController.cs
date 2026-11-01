using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;

namespace Portal.Api.Controllers;

[Route("api")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    public DashboardController(AppData data) : base(data) { }

    [HttpGet("admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        var users = Data.Users.All();
        var students = users.Count(u => u.Role == UserRole.Student);
        var teachers = users.Count(u => u.Role == UserRole.Teacher);
        var classes = Data.ClassSections.All();
        var courses = Data.Levels.All();
        var enrollments = Data.Enrollments.All();

        // "Revenue" = sum of each enrolled student's course price (demo data; no real payments are taken).
        decimal revenue = 0m;
        foreach (var e in enrollments)
        {
            var cls = Data.ClassSections.Find(e.ClassSectionId);
            var course = cls == null ? null : Data.Levels.Find(cls.LevelId);
            revenue += course?.Price ?? 0;
        }

        var enrollmentsByCourse = courses.Select(c => new
        {
            c.Id,
            c.NameEn,
            enrolled = classes.Where(cs => cs.LevelId == c.Id)
                .SelectMany(cs => Data.Enrollments.Where(e => e.ClassSectionId == cs.Id))
                .Count()
        }).OrderByDescending(x => x.enrolled).ToList();

        var last7Days = Enumerable.Range(0, 7).Select(i => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-i)).Reverse();
        var attendanceTrend = last7Days.Select(d => new
        {
            date = d,
            present = Data.AttendanceRecords.Where(r => r.Date == d && r.Status == AttendanceStatus.Present).Count,
            absent = Data.AttendanceRecords.Where(r => r.Date == d && r.Status == AttendanceStatus.Absent).Count
        });

        return Ok(new
        {
            studentCount = students,
            teacherCount = teachers,
            classCount = classes.Count,
            courseCount = courses.Count,
            totalEnrollments = enrollments.Count,
            revenue,
            enrollmentsByCourse,
            attendanceTrend
        });
    }

    [HttpGet("teacher/dashboard")]
    [Authorize(Roles = "Teacher")]
    public IActionResult Teacher()
    {
        var classes = Data.ClassSections.Where(c => c.TeacherId == CurrentUserId);
        var classIds = classes.Select(c => c.Id).ToHashSet();
        var studentCount = Data.Enrollments.Where(e => classIds.Contains(e.ClassSectionId)).Select(e => e.StudentId).Distinct().Count();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todaysShiftClasses = classes.Where(c =>
        {
            var shift = Data.Shifts.Find(c.ShiftId);
            return shift != null && shift.Days.Contains(DateTime.UtcNow.DayOfWeek);
        }).Select(c => new
        {
            c.Id,
            c.Name,
            shift = Data.Shifts.Find(c.ShiftId)?.Name,
            location = Data.CampusLocations.Find(c.CampusLocationId)?.Name
        });

        return Ok(new
        {
            classCount = classes.Count,
            studentCount,
            todaysClasses = todaysShiftClasses,
            classes = classes.Select(c => new { c.Id, c.Name, course = Data.Levels.Find(c.LevelId)?.NameEn })
        });
    }

    [HttpGet("student/dashboard")]
    [Authorize(Roles = "Student")]
    public IActionResult Student()
    {
        var enrollments = Data.Enrollments.Where(e => e.StudentId == CurrentUserId);
        var classIds = enrollments.Select(e => e.ClassSectionId).ToHashSet();
        var classes = Data.ClassSections.Where(c => classIds.Contains(c.Id));
        var courseIds = classes.Select(c => c.LevelId).Distinct().ToList();

        var progress = Data.Progress.Where(p => p.StudentId == CurrentUserId);
        var totalXp = progress.Sum(p => p.XpEarned);

        var courseProgress = courseIds.Select(courseId =>
        {
            var course = Data.Levels.Find(courseId);
            var lessonCount = Data.Lessons.Where(l => l.LevelId == courseId).Count;
            var completed = progress.Count(p => p.LevelId == courseId && p.LessonId != null && p.Status == ProgressStatus.Completed);
            return new
            {
                courseId,
                courseName = course?.NameEn ?? courseId,
                emoji = course?.ThumbnailEmoji,
                lessonCount,
                completed,
                percent = lessonCount == 0 ? 0 : (int)Math.Round(100.0 * completed / lessonCount)
            };
        });

        var upcomingExams = classIds.SelectMany(cid => Data.Exams.Where(e => e.ClassSectionId == cid && e.IsPublished && e.ClosesAt > DateTimeOffset.UtcNow))
            .Select(e => new { e.Id, e.Title, e.OpensAt, e.ClosesAt });

        var schedule = classes.Select(c => new
        {
            c.Id,
            c.Name,
            shift = Data.Shifts.Find(c.ShiftId)?.Name,
            days = Data.Shifts.Find(c.ShiftId)?.Days
        });

        return Ok(new { totalXp, courseProgress, upcomingExams, schedule });
    }
}
