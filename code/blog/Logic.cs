
using System;

namespace Logic
{
    // A class that can't be instantiated in any normal way.
    public static class False
    { 
        // Contradiction lets us prove anything. If we're given
        // a proof of P and ¬P, we can prove any Q.
        public static Q Elim<P, Q>(P p, Not<P> np) => default;
    }

    // The definition of ¬ (not). Even though False can't be instantiated,
    // the type system doesn't care. We can write functions that return the
    // False type and we can write proofs that use False.
    public delegate False Not<P>(P _);

    // The definition of → (implication). This is just function application.
    public delegate Q Implies<P, Q>(P _);

    // The definition of ∧ (conjunction).
    public sealed class And<P, Q>
    {
        // Note that we don't actually have to retain the proofs of the left
        // and right sides. They've already been proven so we don't need the
        // details. We just care about their types.
        public static And<P, Q> Intro(P p, Q q) => new And<P, Q>();
        private And() { }

        // This isn't cheating. if P ∧ Q, then we know that P and we know that Q by definition of ∧.
        public P Left => default;
        public Q Right => default;
    }

    // The definition of ∨ (disjunction).
    public sealed class Or<P, Q>
    {
        public static Or<P, Q> Intro(P _) => new Or<P, Q>();
        public static Or<P, Q> Intro(Q _) => new Or<P, Q>();
        public R Elim<R>(Implies<P, R> pr, Implies<Q, R> qr) => default;
        private Or() { }
    }

    // The definition of ↔ (equivalence).
    public sealed class Iff<P, Q>
    {
        public static Iff<P, Q> Intro(Implies<P, Q> pr, Implies<Q, P> qr) => new Iff<P, Q>();
        private Iff() { }
        public Implies<P, Q> Left => default;
        public Implies<Q, P> Right => default;
    }

    public sealed class ForAll<A, P>
    {
        public static ForAll<A, P> Intro(Func<A, P> p) => new ForAll<A, P>(p);

        private readonly Func<A, P> p;

        private ForAll(Func<A, P> p)
        {
            this.p = p;
        }

        public P Elim(A t) => this.p(t);
    }
}