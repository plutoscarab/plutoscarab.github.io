
using Logic;

namespace tests
{
    public static class LogicTests
    {
        // Modus Ponens
        // (p -> q) && p -> q
        public static Q ModusPonens<P, Q>(And<Implies<P, Q>, P> h) =>
            h.Left(h.Right);
        //  p->q      
        //   applied to
        //            p

        // Modus Tollens
        // (p -> q) && ~q -> ~p
        public static Not<P> ModusTollens<P, Q>(And<Implies<P, Q>, Not<Q>> h) =>
            p => h.Right(h.Left(p));
        //               p->q
        //                 applied to p
        //       q->false
        //             applied to q
        //  p -> false

        // Syllogism
        // (p -> q) && (q -> r) -> (p -> r)
        public static Implies<P, R> Syllogism<P, Q, R>(Implies<P, Q> pq, Implies<Q, R> qr) =>
            p => qr(pq(p));

        // Law of Excluded Middle
        // p || ~p
        public static Or<P, Not<P>> LEM<P>() =>
            default; // we can't prove it, so we make it an axiom

        // One of DeMorgan's Rules
        // ¬p ∨ ¬q → ¬(p ∧ q)
        public static Not<And<P, Q>> DeMorgan<P, Q>(Or<Not<P>, Not<Q>> h) =>
            h.Elim<Not<And<P, Q>>>(
                np => pq => np(pq.Left),
                nq => pq => nq(pq.Right));

        // Double negation
        // p <-> ~~p
        public static Iff<P, Not<Not<P>>> DoubleNeg<P>() =>
            Iff<P, Not<Not<P>>>.Intro(
                p => np => np(p),
                nnp => LEM<P>().Elim<P>(
                    p => p,
                    np => False.Elim<Not<P>, P>(np, nnp)));

        // The DeMorgan rule that requires LEM
        // ¬(p ∧ q) → ¬p ∨ ¬q
        public static Or<Not<P>, Not<Q>> DeMorgan<P, Q>(Not<And<P, Q>> h) =>
            LEM<P>().Elim<Or<Not<P>, Not<Q>>>(
                p => Or<Not<P>, Not<Q>>.Intro(q => h(And<P, Q>.Intro(p, q))),
                np => Or<Not<P>, Not<Q>>.Intro(np));

        // (∀ x : α, p x ∧ q x) → ∀ y : α, p y
        public static ForAll<A, Predicate<A, P>> Example<A, P, Q>(ForAll<A, And<Predicate<A, P>, Predicate<A, Q>>> h) =>
            ForAll<A, Predicate<A, P>>.Intro(
                t => h.Elim(t).Left
            );
    }
}