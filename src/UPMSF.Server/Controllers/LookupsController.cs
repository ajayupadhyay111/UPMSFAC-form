using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPMSF.Server.Data;
using UPMSF.Shared;

namespace UPMSF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LookupsController(AppDbContext db) => _db = db;

    [HttpGet("districts")]
    public async Task<List<LookupItem>> Districts() =>
        await _db.Districts.OrderBy(d => d.Name)
            .Select(d => new LookupItem { Id = d.Id, Name = d.Name }).ToListAsync();

    [HttpGet("councils")]
    public async Task<List<LookupItem>> Councils() =>
        await _db.Councils.OrderBy(c => c.Name)
            .Select(c => new LookupItem { Id = c.Id, Name = c.Name }).ToListAsync();

    /// <summary>Courses filtered by council + course type (drives the dependent dropdown).</summary>
    [HttpGet("courses")]
    public async Task<List<CourseDto>> Courses([FromQuery] int councilId, [FromQuery] CourseType courseType) =>
        await _db.Courses.Where(c => c.CouncilId == councilId && c.CourseType == courseType)
            .OrderBy(c => c.Name)
            .Select(c => new CourseDto
            {
                Id = c.Id, Name = c.Name, ShortCode = c.ShortCode,
                CourseType = c.CourseType, CouncilId = c.CouncilId
            }).ToListAsync();
}
