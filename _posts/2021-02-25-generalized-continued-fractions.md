---
title: Converting Generalized Continued Fractions to Simple Continued Fractions in C#
tags: [math, C#]
---

In the [last post](/2021/02/21/continued-fractions-to-decimal) I showed how to convert a continued fraction of the form

$$
x = t_0 + \cfrac 1 {t_1 + \cfrac 1 {t_2 + \cfrac 1 {t_3 + \cdots}}}
$$

into a decimal fraction of arbitrary precision. This post is about converting a generalized continued fraction of the form

$$
x = t_0 + \cfrac {u_0} {t_1 + \cfrac {u_1} {t_2 + \cfrac {u_2} {t_3 + \cdots}}}
$$

into a simple continued fraction. There are a lot of interesting constants such as $$\pi$$ that have nice pattern for
the generalized continued fraction terms but have no discernable pattern in their simple continued fraction. For example, we
have

$$
\frac 4 \pi = 1 + \cfrac 1 {3 + \cfrac {2^2} {5 + \cfrac {3^2} {7 + \cfrac {4^2} {9 + \cfrac {5^2} {11 + \cdots}}}}}
$$

We'll use the same trick for "consuming" terms one-at-a-time. We represent the value as

$$
f(x) = \frac {a+bx}{c+dx}
$$

with starting values $$a=0, b=1, c=1, d=0$$.  Instead of a single sequence of terms $$t_i$$ and $$x=t_0+\frac 1 y$$ we instead
have two sequences $$t_i$$ and $$u_i$$ and $$x=t_0+\frac {u_0} y$$. Plugging this into $$f(x)$$ we get

$$
\begin{align}
f(x) &= \frac {a + b(t_0 + \frac {u_0) y)} {c + d(t_0 + \frac {u_0} y)} \\
&= \frac {ay + b(t_0y + u_0)} {cy + d(t_0y + u_0)} \\
&= \frac {u_0b + (a+t_0b)y} {u_0d + (c+t_0)y}
\end{align}
$$

