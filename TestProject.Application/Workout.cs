namespace TestProject.Application
{
    public class Workout
    {
        public int Id { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public User User { get; set; }
        public List<Exercise> Exercises { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Completed { get; set; }
    }

    public class WorkoutResponse
    {
        public int Id { get; set; }
        public string WorkoutType { get; set; }
        public List<ExerciseResponse> Exercises { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Completed { get; set; }
    }
}