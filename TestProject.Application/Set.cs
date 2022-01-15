using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Application
{
    public class Set
    {
        public int Id { get; set; }
        public int Reps { get; set; }
        public decimal Weight { get; set; }
        public Exercise Exercise { get; set; }
    }

    public class SetResponse
    {
        public int Id { get; set; }
        public int Reps { get; set; }
        public decimal Weight { get; set; }
    }
}
