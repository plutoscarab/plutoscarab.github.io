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
x = t_0 + \cfrac {u_1} {t_1 + \cfrac {u_2} {t_2 + \cfrac {u_3} {t_3 + \cdots}}}
$$

into a simple continued fraction. There are a lot of interesting constants such as $$pi$$ that have nice pattern for
the generalized continued fraction terms but have no discernable pattern in their simple continued fraction. For example, we
have

$$
\frac 4 \pi = 1 + \cfrac 1 {3 + \cfrac 4 {5 + \cfrac 9 {7 + \cfrac {16} {9 + \cfrac {25} {11 + \cdots}}}}}
$$
