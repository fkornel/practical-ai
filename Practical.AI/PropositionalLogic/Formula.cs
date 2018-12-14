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
        public abstract IEnumerable<Formula> Literals();

        
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

        public override IEnumerable<Formula> Literals()
        {
            return P.Literals().Concat(Q.Literals());
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

        public override string ToString()
        {
            return "(" + P + " & " + Q + ")";
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

        public override string ToString()
        {
            return "(" + P + " | " + Q + ")";
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

        public override string ToString()
        {
            return "!" + P;
        }

        public override IEnumerable<Formula> Literals()
        {
            return P is Variable ? new List<Formula>() { this } : P.Literals();
        }
    }
    public class Variable : Formula
    {
        public bool Value { get; set; }
        public string Name { get; set; }

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

        public override string ToString()
        {
            return Name;
        }

        public override IEnumerable<Formula> Literals()
        {
            return new List<Formula>() { this };
        }
    }

    public class Clause
    {
        public List<Formula> Literals { get; set; }

        public Clause()
        {
            Literals = new List<Formula>();
        }

        public bool Contains(Formula literal)
        {
            if(!IsLiteral(literal))
            {
                throw new ArgumentException("Specified formula is not a literal");
            }

            foreach (var formula in Literals)
            {
                if (LiteralEquals(formula, literal))
                    return true;
            }

            return false;
        }

        public Clause RemoveLiteral(Formula literal)
        {
            if(!IsLiteral(literal))
                throw new ArgumentException("Specified formula is not a literal");

            var result = new Clause();

            for (var i = 0; i < Literals.Count; i++)
            {
                if (!LiteralEquals(literal, Literals[i]))
                    result.Literals.Add(Literals[i]);
            }

            return result;
        }

        public bool LiteralEquals(Formula p, Formula q)
        {
            if (p is Variable && q is Variable)
                return (p as Variable).Name == (q as Variable).Name;
            if (p is Not && q is Not)
                return LiteralEquals((p as Not).P, (q as Not).P);

            return false;
        }

        public bool IsLiteral(Formula p)
        {
            return p is Variable || (p is Not && (p as Not).P is Variable);
        }
    }

    public class Cnf
    {
        public List<Clause> Clauses { get; set; }

        public Cnf()
        {
            Clauses = new List<Clause>();
        }

        public Cnf(And and)
        {
            Clauses = new List<Clause>();
            RemoveParenthesis(and);
        }

        public void SimplifyCnf()
        {
            Clauses.RemoveAll(TautologyClauses);
        }

        private bool TautologyClauses(Clause clause)
        {
            for (var i = 0; i < clause.Literals.Count; i++)
            {
                for (var j = i + 1; j < clause.Literals.Count - 1; j++ )
                {
                    // Checking that literal i and literal j are not of the same type; i.e., both variables or negated literals
                    if (!(clause.Literals[i] is Variable && clause.Literals[j] is Variable) &&
                        !(clause.Literals[i] is Not && clause.Literals[j] is Not))
                    {
                        var not = clause.Literals[i] is Not ? clause.Literals[i] as Not : clause.Literals[j] as Not;
                        var @var = clause.Literals is Variable ? clause.Literals[i] as Variable : clause.Literals[j] as Variable;
                        if (IsNegation(not, @var))
                            return true;
                    }
                }
            }

            return false;
        }

        private bool IsNegation(Not f1, Variable f2)
        {
            return (f1.P as Variable).Name == f2.Name;
        }

        private void Join(IEnumerable<Clause> others)
        {
            Clauses.AddRange(others);
        }

        private void RemoveParenthesis(And and)
        {
            var currentAnd = and;

            while (true)
            {
                // If P is OR or literal and Q is OR or literal.
                if ((currentAnd.P is Or || currentAnd.P is Variable || currentAnd.P is Not) &&
                   (currentAnd.Q is Or || currentAnd.Q is Variable || currentAnd.Q is Not))
                {
                    Clauses.Add(new Clause { Literals = new List<Formula>(currentAnd.P.Literals()) });
                    Clauses.Add(new Clause { Literals = new List<Formula>(currentAnd.Q.Literals()) });
                    break;
                }

                // If P is AND and Q is OR or literal.
                if (currentAnd.P is And && (currentAnd.Q is Or || currentAnd.Q is Variable || currentAnd.Q is Not))
                {
                    Clauses.Add(new Clause { Literals = new List<Formula>(currentAnd.Q.Literals()) });
                    currentAnd = currentAnd.P as And;
                }

                // If P is OR or literal and Q is AND.
                if ((currentAnd.P is Or || currentAnd.P is Variable || currentAnd.P is Not) && currentAnd.Q is And)
                {
                    Clauses.Add(new Clause { Literals = new List<Formula>(currentAnd.P.Literals()) });
                    currentAnd = currentAnd.Q as And;
                }

                // If both P and Q are ANDs.
                if(currentAnd.P is And && currentAnd.Q is And)
                {
                    RemoveParenthesis(currentAnd.P as And);
                    RemoveParenthesis(currentAnd.Q as And);
                    break;
                }
            }
        }

        #region DPLL
        public bool Dpll()
        {
            return Dpll(new Cnf { Clauses = new List<Clause>(Clauses) });
        }

        private bool Dpll(Cnf cnf)
        {
            // The CNF with no clauses is assumed to be True
            if (cnf.Clauses.Count == 0)
                return true;

            // Rule One Literal: if there exists a clause with a single literal
            // we assign it True and remove every clause containing it.
            var cnfAfterOneLit = One
        }

        private Tuple<Cnf, int>OneLiteral(Cnf cnf)
        {
            var unitLiteral = UnitClause(cnf);
            if (unitLiteral == null)
                return new Tuple<Cnf, int>(cnf, 1);

            var newCnf = new Cnf();

            while (unitLiteral != null)
            {
                var clausesToRemove = new List<int>();
                var i = 0;

                // 1st Loop - Finding clauses where the unit literal is, these clauses will not be considered in the new Cnf
                foreach (var clause in cnf.Clauses)
                {
                    if (clause.Literals.Any(literal => clause.LiteralEquals(literal, unitLiteral)))
                        clausesToRemove.Add(i);
                    i++;
                }

                // New Cnf after removing every clause where unit literal is
                newCnf = new Cnf();

                //2nd Loop - Leave clause that do not include the unit literal
                for (var j = 0; j < cnf.Clauses.Count; j++)
                {
                    if (!clausesToRemove.Contains(j))
                        newCnf.Clauses.Add(cnf.Clauses[j]);
                }
                
                // No clauses, which implies SAT
                if (newCnf.Clauses.Count == 0)
                    return new Tuple<Cnf, int>(newCnf, 0);

                // Remove negation of unit luiteral from remaining clauses
                var unitNegated = NegateLiteral(unitLiteral);
                var clausesNoLitNeg = new List<Clause>();

                foreach (var clause in newCnf.Clauses)
                {
                    var newClause = new Clause();

                    // Leaving every literal except the unit literal negated
                    foreach (var literal in clause.Literals)
                        if (!clause.LiteralEquals(literal, unitNegated))
                            newClause.Literals
                }
            }
        }

        public Formula NegateLiteral(Formula literal)
        {
            if (literal is Variable)
                return new Not(literal);
            if (literal is Not)
                return (literal as Not).P;

            return null;
        }

        private Formula UnitClause(Cnf cnf)
        {
            foreach (var clause in cnf.Clauses)
                if (clause.Literals.Count == 1)
                    return clause.Literals.First();

            return null;
        }

        #endregion
    }


}
