#region Using statements
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
#endregion Using statements

#region Service collection
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CmsDatabaseContext>(options =>
    options.UseInMemoryDatabase("CmsDatabase"));
builder.Services.AddAutoMapper(typeof(CmsMapper));
#endregion Service collection

var app = builder.Build();

#region Minimal API endpoints
app.MapGet("/", () => "Hello World!");

app.MapGet("/courses", async (CmsDatabaseContext db) => 
{
    try
    {
        var result = await db.Courses.ToListAsync();
        return Results.Ok(result);
    }
    catch (System.Exception ex)
    {
        return Results.Problem(ex.Message);
    }
    
});

//app.MapPost("/courses", async ([FromBody] CourseDto courseDto, [FromServices] CmsDatabaseContext db, [FromServices] IMapper mapper) => 
app.MapPost("/courses", async (CourseDto courseDto, CmsDatabaseContext db, IMapper mapper) => 
{   
    try
    {
        var newCourse = mapper.Map<Course>(courseDto);

        db.Courses.Add(newCourse);
        await db.SaveChangesAsync();

        var result = mapper.Map<CourseDto>(newCourse);
        return Results.Created($"/courses/{result.CourseId}", result);
    }
    catch (System.Exception ex)
    {
        // throw new InvalidOperationException();
        // return Results.StatusCode(500);
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/courses/{courseId}", async (int courseId, CmsDatabaseContext db, IMapper mapper) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if(course == null)
    {
        return Results.NotFound();
    }

    var result = mapper.Map<CourseDto>(course);
    return Results.Ok(result);    
});

app.MapPut("/courses/{courseId}", async (int courseId, CourseDto courseDto, CmsDatabaseContext db, IMapper mapper) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if(course == null)
    {
        return Results.NotFound();
    }

    course.CourseName = courseDto.CourseName;
    course.CourseDuration = courseDto.CourseDuration;
    course.CourseType = (int)courseDto.CourseType;
    await db.SaveChangesAsync();

    var result = mapper.Map<CourseDto>(course);
    return Results.Ok(result);    
});

app.MapDelete("/courses/{courseId}", async (int courseId, CmsDatabaseContext db, IMapper mapper) =>
{
    var course = await db.Courses.FindAsync(courseId);
    if(course == null)
    {
        return Results.NotFound();
    }

    db.Courses.Remove(course);
    await db.SaveChangesAsync();

    var result = mapper.Map<CourseDto>(course);
    return Results.Ok(result);    
});

#endregion Minimal API endpoints

app.Run();

#region Auto Mapper
public class CmsMapper : Profile
{
    public CmsMapper()
    {
        CreateMap<Course, CourseDto>();
        CreateMap<CourseDto, Course>();
    }
}
#endregion Auto Mapper

#region Models
public class Course
{
    public int CourseId { get; set; }
    public string CourseName { get; set; }  = string.Empty;
    public int CourseDuration { get; set; }
    public int CourseType { get; set; }
}

public class CourseDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; }  = string.Empty;
    public int CourseDuration { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public COURSE_TYPE CourseType { get; set; }
}

public enum COURSE_TYPE
{
    ENGINEERING = 1,
    MEDICAL,
    MANAGEMENT
}
#endregion Models

#region Database context
public class CmsDatabaseContext : DbContext
{
    public DbSet<Course> Courses => Set<Course>();

    public CmsDatabaseContext(DbContextOptions options) : base(options)
    {
    }
}
#endregion Database context