# About

This is a Scanner generator that works along the lines of flex, a popular C++ parser generator.

An example project is included to demonstrate the current capabilities.
Some of the files need to be rearranged for better organization, but the basics are there.

## Regular Expressions

Regexes currently have a rudamentary implementation. All possibilities are as follows:

*: Kleene star. Zero or more of the preceeding letters. It is best to wrap this in parentheses, as expressions like a(ba)* will allow '' and force an 'aba' pattern.

(): Parentheses. An expression in parentheses will be evaluated on its own - it's a recursive algorithm. When in doubt, use more.

[*-*]: A macro for adding a set of characters. Uses the character value to check whether it is between first and second, for example [a-z] will allow all lowercase letters.

|: Or. Will allow the entire left half of the expression or the entire right half, so it should be used with parentheses - a(a|b) allows aa or ab

