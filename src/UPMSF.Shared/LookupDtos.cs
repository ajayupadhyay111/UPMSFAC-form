namespace UPMSF.Shared;

public class LookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class CourseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ShortCode { get; set; } = "";
    public CourseType CourseType { get; set; }
    public int CouncilId { get; set; }
}
