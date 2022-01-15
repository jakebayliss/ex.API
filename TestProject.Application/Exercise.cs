using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Application
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Set> Sets { get; set; }
        public Workout Workout { get; set; }
    }

    public class ExerciseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SetResponse> Sets { get; set; }
    }
}
