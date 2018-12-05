using Practical.AI.PropositionalLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practical.AI
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Variable(false);
            var q = new Variable(false);

            var formula = new Or(new Not(p), q);
            Console.WriteLine(formula.Evaluate());

            p.Value = true;
            Console.WriteLine(formula.Evaluate());
            
            Console.ReadKey();
        }
    }
}
