using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UPMSF.Server.Data;
using UPMSF.Shared;

namespace UPMSF.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    // D5: lookups are static reference data — cache them instead of hitting the DB per request.
    private static readonly MemoryCacheEntryOptions CacheFor24h =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };

    public LookupsController(AppDbContext db, IMemoryCache cache) { _db = db; _cache = cache; }

    [HttpGet("districts")]
    public Task<List<LookupItem>> Districts() =>
        _cache.GetOrCreateAsync("lk_districts", e =>
        {
            e.SetOptions(CacheFor24h);
            return _db.Districts.OrderBy(d => d.Name)
                .Select(d => new LookupItem { Id = d.Id, Name = d.Name }).ToListAsync();
        })!;

    [HttpGet("councils")]
    public Task<List<LookupItem>> Councils() =>
        _cache.GetOrCreateAsync("lk_councils", e =>
        {
            e.SetOptions(CacheFor24h);
            return _db.Councils.OrderBy(c => c.Name)
                .Select(c => new LookupItem { Id = c.Id, Name = c.Name }).ToListAsync();
        })!;

    /// <summary>Courses filtered by council + course type (drives the dependent dropdown).</summary>
    [HttpGet("courses")]
    public Task<List<CourseDto>> Courses([FromQuery] int councilId, [FromQuery] CourseType courseType) =>
        _cache.GetOrCreateAsync($"lk_courses_{councilId}_{(int)courseType}", e =>
        {
            e.SetOptions(CacheFor24h);
            return _db.Courses.Where(c => c.CouncilId == councilId && c.CourseType == courseType)
                .OrderBy(c => c.Name)
                .Select(c => new CourseDto
                {
                    Id = c.Id, Name = c.Name, ShortCode = c.ShortCode,
                    CourseType = c.CourseType, CouncilId = c.CouncilId
                }).ToListAsync();
        })!;
}
