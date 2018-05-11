Enhanced 'using' constructs
============================

This document is meant to detail potential changes to C# post-7.3 to provide
extensions for two uses of the 'using' statement: pattern-based 'using' and
'using var'.

## Background


In C# 7.3, there are two types of 'using' constructs -- 'using' statements
and 'using' directives.

A 'using' directive is a mechanism for importing names into the current
namespace, for instance

```C#
using System;

class C
{
    ...
}
```

There are no proposed changes to this feature, so that's the last time
it will be mentioned.

The second use of the 'using' keyword is in the 'using' statement. This
is the primary focus of the design document. The 'using' statement is
about giving a resource a specific scope that it 'lives' for -- and at
the end of which it is disposed. For instance,

```C#
void M(string filePath)
{
    using (var fileStream = new FileStream(filePath))
    {
        ...
    }
}
```

In the previous example, the `fileStream` variable holds on to an
operating system resource which needs to be acquired and released in
a definite time span. The using statement provides a convenient way for
C# programmers to avoid managing these resources inline. Instead, the
variable inside a using statement must be of a type, like `FileStream`,
which implements the `IDisposable` interface. The `IDisposable` interface
is defined as follows:

```C#
interface IDisposable
{
    void Dispose();
}
```

In other words, it just wraps a simple method called `Dispose`. The
semantics of the using statement are that the above example would be
equivalent to the following code:

```C#
FileStream fileStream = null;
try
{
    fileStream = new FileStream(filePath);
}
finally
{
    fileStream?.Dispose();
}
```

The using statement is essentially syntactic sugar for a try-finally.

## Proposed Changes

### Pattern-based using statement

While conceptually simple, there are some drawbacks to the current design.
The first change to improve usability is allow "pattern implementation" of
Dispose. This means that as long as the type of the using variable 
structurally matches the IDisposable interface (has a public void-returning
Dispose method), then the type doesn't need to actually formally implement
the IDisposable interface. Aside from allowing some esoteric use cases
where the type can't actually implement the IDisposable interface (ref structs), this feature also enables adding a suitable Dispose method as an
extension method, which means that types could be used in a using statement
even if they cannot be recompiled.

The work necessary in the compiler is well-understood. Pattern-based
type recognition already works for the `foreach` statement, which allows
users to provide a suitable method called `GetEnumerator` instead of actually
implementing the `IEnumerable` type. Practically, almost all the work
for this feature will be in the binding of the using statement, currently
implemented mostly in [UsingStatementBinder.BindUsingStatementParts](http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Binder/UsingStatementBinder.cs,60). Right now the using statement
binder just attempts to [find the IDisposable type](http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Binder/UsingStatementBinder.cs,71) and, if it's successful, proceeds
to assert that the type of the using variable has an implicit conversion
to the `IDisposable` interface.

The new implementation would look a lot like the `foreach` implementation
in the [ForEachLoopBinder](http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Binder/ForEachLoopBinder.cs,559). 'Using' would bind similarly to 'foreach' here by [first checking
if the type structurally matches](http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Binder/ForEachLoopBinder.cs,604) `IDisposable` and only falling back
to the interface if the structural match fails.

The structural match itself would rely on a [conventional call to 
`Lookup...InType`](http://source.roslyn.io/#Microsoft.CodeAnalysis.CSharp/Binder/ForEachLoopBinder.cs,782),
which are a family of helpers that search for members on a type which
match given constraints. These constraints reduce to a check for a method
called "Dispose", which we will consider a [WellKnownMemberName](http://source.roslyn.io/#Microsoft.CodeAnalysis/Symbols/WellKnownMemberNames.cs,11).

Lookup is designed to handle most of the language complexity itself, so that
should be just about everything required for the language feature.