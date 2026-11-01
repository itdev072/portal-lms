using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

[Route("api/admin/shifts")]
[Authorize(Roles = "Admin")]
public class ShiftsController : ApiControllerBase
{
    public ShiftsController(AppData data) : base(data) { }

    [HttpGet]
    public IActionResult List() => Ok(Data.Shifts.All().Select(ShiftDto.From));

    [HttpPost]
    public IActionResult Create(ShiftSaveRequest req)
    {
        var shift = new Shift { Name = req.Name, StartTime = req.StartTime, EndTime = req.EndTime, Days = req.Days };
        Data.Shifts.Upsert(shift);
        return Ok(ShiftDto.From(shift));
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, ShiftSaveRequest req)
    {
        var shift = Data.Shifts.Find(id);
        if (shift == null) return NotFound();
        shift.Name = req.Name;
        shift.StartTime = req.StartTime;
        shift.EndTime = req.EndTime;
        shift.Days = req.Days;
        Data.Shifts.Upsert(shift);
        return Ok(ShiftDto.From(shift));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (Data.ClassSections.Where(c => c.ShiftId == id).Any())
            return BadRequest(new { message = "Can't delete a shift that classes are still scheduled on." });
        Data.Shifts.Delete(id);
        return Ok();
    }
}

[Route("api/admin/locations")]
[Authorize(Roles = "Admin,Teacher")]
public class LocationsController : ApiControllerBase
{
    public LocationsController(AppData data) : base(data) { }

    [HttpGet]
    public IActionResult List() => Ok(Data.CampusLocations.All().Select(LocationDto.From));

    [HttpPost]
    public IActionResult Create(LocationSaveRequest req)
    {
        var loc = new CampusLocation { Name = req.Name, Latitude = req.Latitude, Longitude = req.Longitude, RadiusMeters = req.RadiusMeters };
        Data.CampusLocations.Upsert(loc);
        return Ok(LocationDto.From(loc));
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, LocationSaveRequest req)
    {
        var loc = Data.CampusLocations.Find(id);
        if (loc == null) return NotFound();
        loc.Name = req.Name;
        loc.Latitude = req.Latitude;
        loc.Longitude = req.Longitude;
        loc.RadiusMeters = req.RadiusMeters;
        Data.CampusLocations.Upsert(loc);
        return Ok(LocationDto.From(loc));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(string id)
    {
        Data.CampusLocations.Delete(id);
        return Ok();
    }
}
