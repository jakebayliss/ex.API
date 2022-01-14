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
    var user = await db.Users.Include(x => x.Workouts).FirstOrDefaultAsync(x => x.Id == id);
    if(user != null)
    {
        return user.Workouts;
    }
    return null;
})
.WithName("GetUserWorkouts");

app.MapGet("/user/{id}/currentworkouts", async (int id, DatabaseContext db) =>
{
    var user = await db.Users.Include(x => x.Workouts).ThenInclude(x => x.Exercises).FirstOrDefaultAsync(x => x.Id == id);
    if (user != null)
    {
        var workout = user.Workouts.FirstOrDefault(x => !x.Completed);
        if(workout != null)
        {
            return new WorkoutResponse
            {
                Id = workout.Id,
                Completed = workout.Completed,
                CreatedDate = workout.CreatedDate,
                ExerciseCount = workout.Exercises.Count()
            };
        }
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

app.MapGet("/workouts/{id}/currentexercises", async (int id, DatabaseContext db) =>
{
    var workout = await db.Workouts.Include(x => x.Exercises).FirstOrDefaultAsync(x => x.Id == id);
    if (workout != null)
    {
        return workout.Exercises.Select(x => new ExerciseResponse { Id = x.Id, Name = x.Name, Type = x.Type});

    }
    return null;
})
.WithName("GetWorkoutCurrentExercises");

app.MapPost("workouts/add", async (DatabaseContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync();
    var newWorkout = new Workout
    {
        Completed = false,
        CreatedDate = DateTime.Now,
        User = user
    };
    var workout = await db.Workouts.AddAsync(newWorkout);
    await db.SaveChangesAsync();
    return workout.Entity;
})
.WithName("AddWorkout");

app.MapPost("workouts/{id}/exercises/add", async (int id, Exercise ex, DatabaseContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    var newExercise = new Exercise
    {
        Name = ex.Name,
        Type = ex.Type,
        Workout = workout
    };
    var exercise = await db.Exercises.AddAsync(newExercise);
    await db.SaveChangesAsync();
    return new ExerciseResponse
    {
        Id = exercise.Entity.Id,
        Name = exercise.Entity.Name,
        Type = exercise.Entity.Type
    };
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