﻿namespace TestProject.Application
{
    public class Workout
    {
        public int Id { get; set; }
        public User User { get; set; }
        public List<Exercise> Exercises { get; set; }
    }
}