
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
        public static Iff<And<P, Or<Q, R>>, Or<And<P, Q>, And<P, R>>> AndDist<P, Q, R>() =>
            Iff<And<P, Or<Q, R>>, Or<And<P, Q>, And<P, R>>>.Intro(
                (And<P, Or<Q, R>> h) => h.Right.Elim<Or<And<P, Q>, And<P, R>>>(
                    q => Or<And<P, Q>, And<P, R>>.Intro((h.Left, q)),
                    r => Or<And<P, Q>, And<P, R>>.Intro((h.Left, r))),
                (Or<And<P, Q>, And<P, R>> h) => h.Elim<And<P, Or<Q, R>>>(
                    (And<P, Q> pq) => (pq.Left, pq.Right),
                    (And<P, R> pr) => (pr.Left, pr.Right)));

        // Disjunction distributivity
        // p || (q && r) <-> (p || q) && (p || r)
        public static Iff<Or<P, And<Q, R>>, And<Or<P, Q>, Or<P, R>>> OrDist<P, Q, R>() =>
            Iff<Or<P, And<Q, R>>, And<Or<P, Q>, Or<P, R>>>.Intro(
                (Or<P, And<Q, R>> h) => h.Elim<And<Or<P, Q>, Or<P, R>>>(
                    p => (p, p),
                    qr => (qr.Left, qr.Right)),
                (And<Or<P, Q>, Or<P, R>> h) => h.Left.Elim<Or<P, And<Q, R>>>(
                    p => p,
                    q => h.Right.Elim<Or<P, And<Q, R>>>(
                        p => p,
                        r => Or<P, And<Q, R>>.Intro((q, r)))));

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

        // (p → (q → r)) ↔ (p ∧ q → r)
        public static Iff<Implies<P, Implies<Q, R>>, Implies<And<P, Q>, R>> Example1<P, Q, R>() =>
            Iff<Implies<P, Implies<Q, R>>, Implies<And<P, Q>, R>>.Intro(
                (Implies<P, Implies<Q, R>> h) =>
                    (And<P, Q> g) => h(g.Left)(g.Right),
                (Implies<And<P, Q>, R> h) =>
                    p => q => h((p, q)));

        // ((p ∨ q) → r) ↔ (p → r) ∧ (q → r)
        public static Iff<Implies<Or<P, Q>, R>, And<Implies<P, R>, Implies<Q, R>>> Example2<P, Q, R>() =>
            Iff<Implies<Or<P, Q>, R>, And<Implies<P, R>, Implies<Q, R>>>.Intro(
                (Implies<Or<P, Q>, R> h) => (
                    p => h(p),
                    q => h(q)),
                (And<Implies<P, R>, Implies<Q, R>> h) =>
                    (Or<P, Q> g) => g.Elim<R>(
                        p => h.Left(p),
                        q => h.Right(q)));

        // ¬(p ∨ q) ↔ ¬p ∧ ¬q
        public static Iff<Not<Or<P, Q>>, And<Not<P>, Not<Q>>> Example3<P, Q, R>() =>
            Iff<Not<Or<P, Q>>, And<Not<P>, Not<Q>>>.Intro(
                (Not<Or<P, Q>> h) => (p => h(p), q => h(q)),
                (And<Not<P>, Not<Q>> h) => (Or<P, Q> pq) => pq.Elim<False>(
                    p => h.Left(p),
                    q => h.Right(q)));

        // ¬p ∨ ¬q → ¬(p ∧ q)
        public static Not<And<P, Q>> Example4<P, Q, R>(Or<Not<P>, Not<Q>> h) =>
            (And<P, Q> pq) => h.Elim<False>(
                np => np(pq.Left),
                nq => nq(pq.Right));

        // p ∧ ¬q → ¬(p → q)
        public static Not<Implies<P, Q>> Example6<P, Q>(And<P, Not<Q>> h) =>
            (Implies<P, Q> g) => h.Right(g(h.Left));

        // ¬p → (p → q)
        public static Implies<P, Q> Example7<P, Q>(Not<P> h) =>
            p => False.Elim<P, Q>(p, h);

        // (¬p ∨ q) → (p → q)
        public static Implies<P, Q> Example8<P, Q>(Or<Not<P>, Q> h) =>
            p => h.Elim<Q>(
                np => False.Elim<P, Q>(p, np),
                q => q);

        // p ∨ false ↔ p
        public static Iff<Or<P, False>, P> Example9<P>() =>
            Iff<Or<P, False>, P>.Intro(
                (Or<P, False> h) => h.Elim<P>(
                    p => p,
                    f => f.Elim<P>()),
                p => p);

        //  p ∧ false ↔ false
        public static Iff<And<P, False>, False> Example10<P>() =>
            Iff<And<P, False>, False>.Intro(
                (And<P, False> h) => h.Right,
                f => f.Elim<And<P, False>>()
            );

        // This one is a real tongue-twister.
        // ¬(p ↔ ¬p)
        public static Not<Iff<P, Not<P>>> Example11<P>() =>
            (Iff<P, Not<P>> h) => 
            {
                Not<P> np = p => h.Forward(p)(p);
                return np(h.Reverse(np));
            };

        // (p → q) → (¬q → ¬p)
        public static Implies<Not<Q>, Not<P>> Example12<P, Q>(Implies<P, Q> h) =>
            nq => ModusTollens<P, Q>((h, nq));

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

        // (p → r ∨ s) → ((p → r) ∨ (p → s))
        public static Or<Implies<P, R>, Implies<P, S>> Example1<P, R, S>(Implies<P, Or<R, S>> h) =>
            LEM<P>().Elim<Or<Implies<P, R>, Implies<P, S>>>(
                p => h(p).Elim<Or<Implies<P, R>, Implies<P, S>>>(
                    r => Or<Implies<P, R>, Implies<P, S>>.Intro(p => r),
                    s => Or<Implies<P, R>, Implies<P, S>>.Intro(p => s)
                ),
                np => Or<Implies<P, R>, Implies<P, S>>.Intro(p => False.Elim<P, R>(p, np))
            );

        //  ¬(p → q) → p ∧ ¬q
        public static And<P, Not<Q>> Example2<P, Q>(Not<Implies<P, Q>> h) =>
            LEM<P>().Elim<And<P, Not<Q>>>(
                p => LEM<Q>().Elim<And<P, Not<Q>>>(
                    q => h(p => q).Elim<And<P, Not<Q>>>(),
                    nq => (p, nq)),
                np => LEM<Q>().Elim<And<P, Not<Q>>>(
                    q => h(p => q).Elim<And<P, Not<Q>>>(),
                    nq => h(p => np(p).Elim<Q>()).Elim<And<P, Not<Q>>>()));

        // (p → q) → (¬p ∨ q)
        public static Or<Not<P>, Q> Example3<P, Q>(Implies<P, Q> h) =>
            LEM<P>().Elim<Or<Not<P>, Q>>(
                p => Or<Not<P>, Q>.Intro(h(p)),
                np => np);

        // (¬q → ¬p) → (p → q)
        public static Implies<P, Q> Example4<P, Q>(Implies<Not<Q>, Not<P>> h) =>
            p => LEM<Q>().Elim<Q>(
                q => q,
                nq => False.Elim<P, Q>(p, h(nq)));

        // ((p → q) → p) → p
        public static P Peirce<P, Q>(Implies<Implies<P, Q>, P> h) =>
            LEM<P>().Elim<P>(
                p => p,
                np => h(p => False.Elim<P, Q>(p, np)));
    }
}