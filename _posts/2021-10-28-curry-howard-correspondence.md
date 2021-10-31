---
title: Curry-Howard Correspondence
tags: math, logic, curry-howard
---

The Curry-Howard correspondence (or isomorphism) refers to a remarkable alignment between propositions in logic
and types in computer programs. Statements in logic can be interpreted as data types in programs,
and if the type is inhabited (the program is able to instantiate the type) then the program
represents a proof of the proposition. A proof corresponds to a function in a program, and the
proposition it proves corresponds to the return type of the function. If the type cannot be instantiated,
the function cannot be written or cannot complete, and the proposition is not proved. If the function
completes by returning an instance of the type, then the function is a proof of the proposition.

Note that this doesn't work for every type of logic, or every programming language. For example,
in C# every type can be instantiated, either with *null* for reference types, or the default
constructor for value types. You need a language with a sufficiently-expressive type system and
without kludges like *null*.

[Propositions and Proofs](https://github.com/leanprover/tutorial/blob/master/03_Propositions_and_Proofs.org)
from the [Lean Prover Tutorial](https://github.com/leanprover/tutorial) is a great illustration of this.

## Correspondence between logical implication P → Q and function P ⇒ Q

Here's how to represent a simple theorem in Lean.

```lean
constants p q : Prop

theorem t1 : p → q → p := λ Hp : p, λ Hq : q, Hp
```

The Curry-Howard correspondence says there two ways to interpret this. Here, the programmer's interpretation is for a language
that doesn't have dependent types. For a language the *does* have dependent types, `p` and `q` could be values instead of types.

|The Content|The Logic Interpretation|The Programmer's Interpretation|
|---|---|---|
|`Prop`|The type that represents propositions|Same|
|`constants p q : Prop`|`p` and `q` are two propositions|`p` and `q` are two types that are `Prop`|
|`theorem t1`|There is a theorem called `t1`|There is a value called `t1`|
|`: p → q → p`|It will prove that `p` implies that `q` implies `p`|The value `t1` is a function that takes a `p` and returns a function that takes a `q` and returns a `p`|
|`:=`|Here's the proof|Here's the implementation of that function|
|`λ Hp : p,`|Hypothesis `Hp` is that `p` is true|Anonymous function that takes an argument called `Hp` of type `p`|
|`λ Hq : q,`|Hypothesis `Hp` is that `q` is true|Nested anonymous function that takes an argument called `Hq` of type `q`|
|`Hp`|Use the first hypothesis to prove the proposition|Return any `p` and we're good|

## The other logical connectives, in C# just for fun/pain (it's not that bad!)

I decided to try to do this stuff in C#, not because it's a good idea, but to help me understand some of the trickier stuff I was
reading in the Lean tutorial. And it worked! (For me.) Even C# is good enough to handle basic logic stuff, like 

|English|Logic|C#|
|---|---|---|
|Implication (implies)|→|=>|
|Negation (not)|¬|~|
|Conjunction (and)|∧|&&|
|Disjunction (or)|∨|&#124;&#124;|
|Equivalence|↔|n/a|

Generic types in C# are sufficient to express types that correspond to each of these logical connectives,
and to write basic proofs. If the code compiles, the proof is correct. 
C# lets you cheat by using `null` or `default` or `Activator.CreateInstance`,
but that's okay--we need to be able to do that in order to introduce axioms. If you introduce something as an
axiom that you didn't mean to, then you're going to get bad proofs, and that's true of Lean, as well.

Here's what I ended up with for the logic framework:

```csharp
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

// The definition of → (implication). This is just function application,
// no different from Func<P, Q>, but it makes things more readible.
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
```

This is enough for us to start proving some simple things using constructive logic.

```csharp
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

// One of DeMorgan's Rules
// ¬p ∨ ¬q → ¬(p ∧ q)
public static Not<And<P, Q>> DeMorgan<P, Q>(Or<Not<P>, Not<Q>> h) =>
    h.Elim<Not<And<P, Q>>>(
        np => pq => np(pq.Left),
        nq => pq => nq(pq.Right));
```

If we want to use classical logic which assumes the Law of Excluded Middle, we have
to introduce it as an axiom..

```csharp
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
        p => Or<Not<P>, Not<Q>>.Intro(q => h(And<P, Q>.Intro(p, q))),
        np => Or<Not<P>, Not<Q>>.Intro(np));
```

The need to introduce LEM as an axiom is one reason you might want to work without LEM. 
It forces us to reason by contradiction in many cases, and allows
proving the existence of mathematical objects without providing a 
way to construct them. Whether that's good or bad is a matter of 
personal taste and interest.
The increasing use of proof verification systems might cause 
constructivism to become more popular, but even if it doesn't, it
will ensure that constructive and classical logic are clearly
delineated.

For example, any proof of any proposition using the basic logical
connectives can be verified by a truth table. This is taught in
Logic 101. But truth tables assume LEM! If you can't construct
a proof without using a truth table, you're doing classical logic
not constructive logic. It's neither good or bad, it's just 
playing by a different set of rules.

## Universal Quantifer

The Universal Qualifier is used
to form propositions like "for all instances x of some type, the
proposition p(x) holds".

We can sort of model the idea of a predicate as a function that returns
a proposition (a type):

```csharp
public delegate Type Predicate<A>(A x);
```

But the proposition type is getting erased, e.g. if our predicate always
returns an `Or<P, Q>` we lose that fact. But we can model predicates that
always return the same *type* of proposition.

```csharp
public delegate P Predicate<A, P>(A x);
```

Note that, like `Implies<P, Q>`, this is no different than `Func<A, P>`
in .NET, so we don't really need it, but we use it for readibility.

And we can sort of model the introduction rule for ∀, we just don't
have a way to indicate that `p` was shown in a context where its 
argument was arbitrary. We have to assume it was, e.g., as an assumption.
The elimination rule is straightforward.

```csharp
public sealed class ForAll<A, P>
{
    public static ForAll<A, P> Intro(Predicate<A, P> p) => new ForAll<A, P>(p);

    private readonly Predicate<A, P> p;

    private ForAll(Predicate<A, P> p)
    {
        this.p = p;
    }

    public P Elim(A t) => this.p(t);
}
```

Because C# doesn't have dependent types, it means we have to specify, in advance,
the type of the proposition being returned. This means that `ForAll` is just taking
a `Func<A, P>` and wrapping it in `Intro` and `Elim`. So in C#, `ForAll` is also
just `Func`! But once again, it makes things a little more readible.

Now we can prove some things implied by universal quantification, such as

```csharp
// (∀ x : α, p x ∧ q x) → ∀ y : α, p y
public static ForAll<A, Predicate<A, P>> Example<A, P, Q>(ForAll<A, And<Predicate<A, P>, Predicate<A, Q>>> h) =>
    ForAll<A, Predicate<A, P>>.Intro(
        t => h.Elim(t).Left
    );
```

I know this stuff is ugly and hard to read in C#, but if it's interesting to you, definitely learn
Lean. It's super fun. I only did this in C# to verify my understanding of how the Curry-Howard
Correspondence works and what theorem verifiers might
be doing under the covers, and maybe someone else will find this useful.
