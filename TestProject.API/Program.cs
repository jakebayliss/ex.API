using Microsoft.EntityFrameworkCore;
using TestProject.API;
using TestProject.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/workouts", async (DatabaseContext db) =>
{
    return await db.Workouts.ToListAsync();
})
.WithName("GetWorkouts");

app.MapGet("/workouts/{id}", async (int id, DatabaseContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    if(workout != null)
    {
        return workout;
    }
    return null;
})
.WithName("GetWorkout");

app.MapGet("/user/{id}/workouts", async (int id, DatabaseContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if(user != null)
    {
        var workouts = await db.Workouts.Where(x => x.User.Id == user.Id).ToListAsync();
        return workouts;
    }
    return null;
})
.WithName("GetUserWorkouts");

app.MapGet("workouts/{id}/exercises", async (int id, DatabaseContext db) =>
{
    var workout = await db.Workouts
        .Include(x => x.Exercises)
        .ThenInclude(x => x.Sets)
        .FirstOrDefaultAsync(x => x.Id == id);
    if(workout != null)
    {
        return workout.Exercises;
    }
    return null;
})
.WithName("GetExercises");

app.MapPost("workouts/add", async (Workout workout, DatabaseContext db) =>
{
    var newWorkout = await db.Workouts.AddAsync(workout);
    return newWorkout;
})
.WithName("AddWorkout");

app.MapPost("exercises/add", async (Exercise ex, DatabaseContext db) =>
{
    var exercise = await db.Exercises.AddAsync(ex);
    return exercise;
})
.WithName("AddExercise");

app.MapGet("exrcises/{id}/sets", async (int id, DatabaseContext db) =>
{
    var sets = await db.Exercises.Include(x => x.Sets).Where(x => x.Id == id).ToListAsync();
    return sets;
})
.WithName("GetSets");

app.MapPost("sets/add", async (Set set, DatabaseContext db) =>
{
    var newSet = await db.Sets.AddAsync(set);
    return newSet;
})
.WithName("AddSet");

app.Run();