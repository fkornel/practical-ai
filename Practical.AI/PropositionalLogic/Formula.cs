using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practical.AI.PropositionalLogic
{
    public abstract class Formula
    {
        public abstract bool Evaluate();
        public abstract IEnumerable<Variable> Variables();
    }

    public abstract class BinaryGate : Formula
    {
        public Formula P { get; set; }
        public Formula Q { get; set; }

        public BinaryGate(Formula p, Formula q)
        {
            P = p;
            Q = q;
        }

        public override IEnumerable<Variable> Variables()
        {
            return P.Variables().Concat(Q.Variables());
        }
    }

    public class And : BinaryGate
    {
        public And(Formula p, Formula q): base(p, q)
        { }

        public override bool Evaluate()
        {
            return P.Evaluate() && Q.Evaluate();
        }
    }

    public class Or : BinaryGate
    {
        public Or(Formula p, Formula q) : base(p, q)
        { }

        public override bool Evaluate()
        {
            return P.Evaluate() || Q.Evaluate();
        }
    }

    public class Not : Formula
    {
        public Formula P { get; set; }

        public Not(Formula p)
        {
            P = p;
        }

        public override bool Evaluate()
        {
            return !P.Evaluate();
        }

        public override IEnumerable<Variable> Variables()
        {
            return new List<Variable>(P.Variables());
        }
    }
    public class Variable : Formula
    {
        public bool Value { get; set; }

        public Variable(bool value)
        {
            Value = value;
        }

        public override bool Evaluate()
        {
            return Value;
        }

        public override IEnumerable<Variable> Variables()
        {
            return new List<Variable>() { this };
        }
    }
}
