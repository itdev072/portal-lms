using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

[Route("api")]
[Authorize]
public class ClassesController : ApiControllerBase
{
    public ClassesController(AppData data) : base(data) { }

    private ClassDto ToDto(ClassSection c)
    {
        var teacher = Data.Users.Find(c.TeacherId);
        var level = Data.Levels.Find(c.LevelId);
        var shift = Data.Shifts.Find(c.ShiftId);
        var location = Data.CampusLocations.Find(c.CampusLocationId);
        var enrolled = Data.Enrollments.Where(e => e.ClassSectionId == c.Id).Count;

        return new ClassDto(
            c.Id, c.Name, c.TeacherId, teacher?.DisplayName ?? "(unassigned)", c.SemesterLabel,
            c.LevelId, level?.NameEn ?? c.LevelId, c.ShiftId, shift?.Name ?? "", c.CampusLocationId,
            location?.Name ?? "", c.Capacity, enrolled);
    }

    [HttpGet("admin/classes")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminList() => Ok(Data.ClassSections.All().Select(ToDto));

    [HttpPost("admin/classes")]
    [Authorize(Roles = "Admin")]
    public IActionResult Create(ClassSaveRequest req)
    {
        var cls = new ClassSection
        {
            Name = req.Name,
            TeacherId = req.TeacherId,
            SemesterLabel = req.SemesterLabel,
            LevelId = req.LevelId,
            ShiftId = req.ShiftId,
            CampusLocationId = req.CampusLocationId,
            Capacity = req.Capacity
        };
        Data.ClassSections.Upsert(cls);
        return Ok(ToDto(cls));
    }

    [HttpPut("admin/classes/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(string id, ClassSaveRequest req)
    {
        var cls = Data.ClassSections.Find(id);
        if (cls == null) return NotFound();
        cls.Name = req.Name;
        cls.TeacherId = req.TeacherId;
        cls.SemesterLabel = req.SemesterLabel;
        cls.LevelId = req.LevelId;
        cls.ShiftId = req.ShiftId;
        cls.CampusLocationId = req.CampusLocationId;
        cls.Capacity = req.Capacity;
        Data.ClassSections.Upsert(cls);
        return Ok(ToDto(cls));
    }

    [HttpDelete("admin/classes/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(string id)
    {
        Data.ClassSections.Delete(id);
        foreach (var e in Data.Enrollments.Where(e => e.ClassSectionId == id))
            Data.Enrollments.Delete(e.Id);
        return Ok();
    }

    /// <summary>A teacher's own assigned classes.</summary>
    [HttpGet("teacher/classes")]
    [Authorize(Roles = "Teacher")]
    public IActionResult MyClasses()
    {
        var mine = Data.ClassSections.Where(c => c.TeacherId == CurrentUserId).Select(ToDto);
        return Ok(mine);
    }

    /// <summary>A teacher creates their own new class — course + shift + a campus
    /// location (new or existing). TeacherId is always the signed-in teacher, never
    /// taken from the request, so a teacher can't assign classes to someone else.</summary>
    [HttpPost("teacher/classes")]
    [Authorize(Roles = "Teacher")]
    public IActionResult CreateMine(ClassSaveRequest req)
    {
        var cls = new ClassSection
        {
            Name = req.Name,
            TeacherId = CurrentUserId!,
            SemesterLabel = req.SemesterLabel,
            LevelId = req.LevelId,
            ShiftId = req.ShiftId,
            CampusLocationId = req.CampusLocationId,
            Capacity = req.Capacity
        };
        Data.ClassSections.Upsert(cls);
        return Ok(ToDto(cls));
    }

    /// <summary>Published courses — for the teacher's own "create class" course dropdown.</summary>
    [HttpGet("teacher/courses")]
    [Authorize(Roles = "Teacher")]
    public IActionResult CoursesForTeacher()
        => Ok(Data.Levels.Where(l => l.IsPublished).OrderBy(l => l.Order));

    [HttpGet("teacher/classes/{id}/roster")]
    [Authorize(Roles = "Teacher,Admin")]
    public IActionResult Roster(string id)
    {
        var cls = Data.ClassSections.Find(id);
        if (cls == null) return NotFound();
        if (CurrentRole == UserRole.Teacher && cls.TeacherId != CurrentUserId) return Forbidden();

        var studentIds = Data.Enrollments.Where(e => e.ClassSectionId == id).Select(e => e.StudentId).ToHashSet();
        var roster = Data.Users.Where(u => studentIds.Contains(u.Id))
            .OrderBy(u => u.DisplayName)
            .Select(u => new { u.Id, u.DisplayName, u.Email });
        return Ok(roster);
    }

    /// <summary>All published courses, for dropdowns.</summary>
    [HttpGet("teachers")]
    [Authorize(Roles = "Admin")]
    public IActionResult Teachers()
        => Ok(Data.Users.Where(u => u.Role == UserRole.Teacher && u.IsActive).Select(UserDto.From));
}
