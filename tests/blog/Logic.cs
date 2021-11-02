
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

        // Conjunction commutivity
        // p ∧ q ↔ q ∧ p
        public static Iff<And<P, Q>, And<Q, P>> AndComm<P, Q>() =>
            Iff<And<P, Q>, And<Q, P>>.Intro(
                (And<P, Q> pq) => (pq.Right, pq.Left),
                (And<Q, P> qp) => (qp.Right, qp.Left));

        // Disjunction commutivity
        // p ∨ q ↔ q ∨ p
        public static Iff<Or<P, Q>, Or<Q, P>> OrComm<P, Q>() =>
            Iff<Or<P, Q>, Or<Q, P>>.Intro(
                (Or<P, Q> pq) => pq.Elim<Or<Q, P>>(
                    p => p,
                    q => q),
                (Or<Q, P> qp) => qp.Elim<Or<P, Q>>(
                    q => q,
                    p => p));

        // Conjunction associativity
        // (p ∧ q) ∧ r ↔ p ∧ (q ∧ r)
        public static Iff<And<And<P, Q>, R>, And<P, And<Q, R>>> AndAssoc<P, Q, R>() =>
            Iff<And<And<P, Q>, R>, And<P, And<Q, R>>>.Intro(
                h => (h.Left.Left, (h.Left.Right, h.Right)),
                h => ((h.Left, h.Right.Left), h.Right.Right));

        // Disjunction associativity
        // (p ∨ q) ∨ r ↔ p ∨ (q ∨ r)
        public static Iff<Or<Or<P, Q>, R>, Or<P, Or<Q, R>>> OrAssoc<P, Q, R>() =>
            Iff<Or<Or<P, Q>, R>, Or<P, Or<Q, R>>>.Intro(
                (Or<Or<P, Q>, R> h) => h.Elim<Or<P, Or<Q, R>>>(
                    (Or<P, Q> pq) => pq.Elim<Or<P, Or<Q, R>>>(
                        p => p,
                        q => Or<P, Or<Q, R>>.Intro(q)),
                    r => Or<P, Or<Q, R>>.Intro(r)),
                (Or<P, Or<Q, R>> h) => h.Elim<Or<Or<P, Q>, R>>(
                    p => Or<Or<P, Q>, R>.Intro(p),
                    (Or<Q, R> qr) => qr.Elim<Or<Or<P, Q>, R>>(
                        q => Or<Or<P, Q>, R>.Intro(q),
                        r => r)));

        // Conjunction distributivity
        // p ∧ (q ∨ r) ↔ (p ∧ q) ∨ (p ∧ r)
        //public static Iff<> AndDist<P, Q, R>() =>
        //    ;

        // Non-contradiction
        // ~(p && ~p)
        public static Not<And<P, Not<P>>> NonContra<P>() =>
            (And<P, Not<P>> h) => h.Right(h.Left);

        // One of DeMorgan's Rules
        // ¬p ∨ ¬q → ¬(p ∧ q)
        public static Not<And<P, Q>> DeMorgan<P, Q>(Or<Not<P>, Not<Q>> h) =>
            h.Elim<Not<And<P, Q>>>(
                np => pq => np(pq.Left),
                nq => pq => nq(pq.Right));

        // Law of Excluded Middle
        // p || ~p
        public static Or<P, Not<P>> LEM<P>() =>
            default; // we can't prove it, so we make it an axiom

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
                p => Or<Not<P>, Not<Q>>.Intro(q => h((p, q))),
                np => np);

        // (∀ x : α, p x ∧ q x) → ∀ y : α, p y
        public static ForAll<A, Predicate<A, P>> Example<A, P, Q>(ForAll<A, And<Predicate<A, P>, Predicate<A, Q>>> h) =>
            ForAll<A, Predicate<A, P>>.Intro(
                t => h.Elim(t).Left
            );
    }
}