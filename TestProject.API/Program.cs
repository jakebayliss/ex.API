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

app.MapGet("workouttypes", async (DatabaseContext db) =>
{
    return await db.WorkoutTypes.ToListAsync();
})
.WithName("GetWorkoutTypes");

app.MapGet("/user/{id}/currentworkouts", async (int id, DatabaseContext db) =>
{
    var user = await db.Users.Include(x => x.Workouts).ThenInclude(x => x.Exercises).ThenInclude(x => x.Sets).FirstOrDefaultAsync(x => x.Id == id);
    if (user != null)
    {
        var workout = user.Workouts.FirstOrDefault(x => !x.Completed);
        if(workout != null)
        {
            var workoutType = await db.WorkoutTypes.FindAsync(workout.WorkoutType);
            return new WorkoutResponse
            {
                Id = workout.Id,
                Completed = workout.Completed,
                CreatedDate = workout.CreatedDate,
                WorkoutType = workoutType.Name,
                Exercises = workout.Exercises.Select(x => new ExerciseResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Sets = x.Sets.Select(s => new SetResponse
                    {
                        Id = s.Id,
                        Reps = s.Reps,
                        Weight = s.Weight
                    }).ToList()
                }).ToList()
            };
        }
    }
    return null;
})
.WithName("GetUserCurrentWorkouts");

app.MapPost("workouts/add", async (string type, DatabaseContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync();
    var workoutType = await db.WorkoutTypes.FirstOrDefaultAsync(x => x.Name == type);
    var newWorkout = new Workout
    {
        Completed = false,
        CreatedDate = DateTime.Now,
        User = user,
        WorkoutType = workoutType
    };
    var workout = await db.Workouts.AddAsync(newWorkout);
    await db.SaveChangesAsync();
    return new WorkoutResponse
    {
        Id = workout.Entity.Id,
        Completed = workout.Entity.Completed,
        CreatedDate = workout.Entity.CreatedDate,
        WorkoutType = workoutType.Name
    };
})
.WithName("AddWorkout");

app.MapPost("workouts/{id}/save", async (int id, DatabaseContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    if(workout != null)
    {
        workout.Completed = true;
        await db.SaveChangesAsync();
    }
})
.WithName("SaveWorkout");

app.MapPost("workouts/{id}/exercises/add", async (int id, Exercise ex, DatabaseContext db) =>
{
    var workout = await db.Workouts.Include(x => x.Exercises).ThenInclude(x => x.Sets).FirstOrDefaultAsync(x => x.Id == id);
    var newExercise = new Exercise
    {
        Name = ex.Name,
        Workout = workout
    };
    var exercise = await db.Exercises.AddAsync(newExercise);
    await db.SaveChangesAsync();
    return new WorkoutResponse
    {
        Id = workout.Id,
        Completed = workout.Completed,
        CreatedDate = workout.CreatedDate,
        Exercises = workout.Exercises.Select(x => new ExerciseResponse
        {
            Id = x.Id,
            Name = x.Name,
        }).ToList()
    };
})
.WithName("AddExercise");

app.MapPost("workouts/{workoutId}/exercises/{exerciseId}/sets/add", async (int workoutId, int exerciseId, Set set, DatabaseContext db) =>
{
    var exercise = await db.Exercises.FindAsync(exerciseId);
    var newSet = new Set
    {
        Weight = set.Weight,
        Reps = set.Reps,
        Exercise = exercise
    };
    var dbSet = await db.Sets.AddAsync(newSet);
    await db.SaveChangesAsync();
    var workout = await db.Workouts.Include(x => x.Exercises).ThenInclude(x => x.Sets).FirstOrDefaultAsync(x => x.Id == workoutId);
    return new WorkoutResponse
    {
        Id = workout.Id,
        Completed = workout.Completed,
        CreatedDate = workout.CreatedDate,
        Exercises = workout.Exercises.Select(x => new ExerciseResponse
        {
            Id = x.Id,
            Name = x.Name,
            Sets = x.Sets.Select(s => new SetResponse
            {
                Id = s.Id,
                Reps = s.Reps,
                Weight = s.Weight
            }).ToList()
        }).ToList()
    };
})
.WithName("AddSet");

app.Run();