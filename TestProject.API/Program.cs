using Microsoft.EntityFrameworkCore;
using TestProject.API;
using TestProject.Application;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, builder =>
        {
            builder.WithOrigins("*").AllowAnyHeader()
                                .AllowAnyMethod(); ;
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
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

app.MapGet("/user/{id}/currentworkouts", async (int id, DatabaseContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user != null)
    {
        var workouts = await db.Workouts.Where(x => x.User.Id == user.Id).ToListAsync();
        return workouts.Where(x => !x.Completed);
    }
    return null;
})
.WithName("GetUserCurrentWorkouts");

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

app.MapPost("workouts/add", async (DatabaseContext db) =>
{
    var newWorkout = new Workout
    {
        Completed = false,
        CreatedDate = DateTime.Now
    };
    var workout = await db.Workouts.AddAsync(newWorkout);
    await db.SaveChangesAsync();
    return workout.Entity;
})
.WithName("AddWorkout");

app.MapPost("exercises/add", async (Exercise ex, DatabaseContext db) =>
{
    var exercise = await db.Exercises.AddAsync(ex);
    await db.SaveChangesAsync();
    return exercise.Entity;
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
    await db.SaveChangesAsync();
    return newSet;
})
.WithName("AddSet");

app.Run();