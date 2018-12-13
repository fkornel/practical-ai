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
        public abstract Formula ToNnf();
        public abstract Formula ToCnf();

        
        public Formula DistributeCnf(Formula p, Formula q)
        {
            if (p is And)
                return new And(DistributeCnf((p as And).P, q), DistributeCnf((p as And).Q, q));

            if (q is And)
                return new And(DistributeCnf(p, (q as And).P), DistributeCnf(p, (q as And).Q));

            return new Or(p, q);
        }
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

        public override Formula ToNnf()
        {
            return new And(P.ToNnf(), Q.ToNnf());
        }

        public override Formula ToCnf()
        {
            return new And(P.ToCnf(), Q.ToCnf());
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

        public override Formula ToNnf()
        {
            return new Or(P.ToNnf(), Q.ToNnf());
        }

        public override Formula ToCnf()
        {
            return DistributeCnf(P.ToCnf(), Q.ToCnf());
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

        public override Formula ToNnf()
        {
            if (P is And)
                return new Or(new Not((P as And).P), new Not((P as And).Q));

            if (P is Not)
                return new Not((P as Not).P);

            return this;
        }

        public override Formula ToCnf()
        {
            return this;
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

        public override Formula ToNnf()
        {
            throw new NotImplementedException();
        }

        public override Formula ToCnf()
        {
            throw new NotImplementedException();
        }
    }
}
