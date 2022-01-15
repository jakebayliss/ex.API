using Microsoft.EntityFrameworkCore;
using TestProject.Application;

namespace TestProject.API
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Workout> Workouts => Set<Workout>();
        public DbSet<WorkoutType> WorkoutTypes => Set<WorkoutType>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<Set> Sets => Set<Set>();
    }
}
