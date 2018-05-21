# Enhanced 'using' constructs

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


*Worklist*

1. Add a definition for a Dispose method to WellKnownMemberNames.cs.
2. Modify BindUsingStatementParts to do a LookupMembersInType for the method name
   added to WellKnownMemberNames.cs.
3. Check if the lookup has at least one result
4. If there is a method that takes no parameters or type parameters, accept it.
   Otherwise, produce a warning that Dispose methods exist, but they have the
   wrong signature.
5. If (4) was a success, produce a new BoundUsingStatement found with the target
   method. If not, fallback to the old code to try and find an IDisposable
   conversion.

### `using var`

#### Design

The next feature is `using var`. The simple, intuitive description of the feature is that
variables which are declared with the `using` keyword before the type are automatically
disposed when those variables go out of scope.

Unpacking that description, there are several implications. The first is that using variables
must be valid variable declarations, so this would be an augmentation of the 
`LocalDeclarationStatementSyntax`. Next, using variables must have types that conform to the
requirements of the using statement, right now meaning that they have an implicit conversion
to IDisposable. It then seems reasonable to also implement the same restrictions that using
variables in using statements have, namely that they are read-only and cannot be modified
after declaration or be passed by-ref except by readonly ref.

Lastly, we have to consider what the effective scope of "disposal at end of scope" means. 
Using statements have braces to clearly delineate their scope. In many cases, C# provides
similar enclosing deliminations as part of blocks. Since using variables can only appear in
statements and statements usually must appear in blocks, this seems like a reasonable first
step. The behavior would be as follows: at the point a using variable is declared and assigned,
it will immediately be followed by a try-finally, where the remainder of the method will be nested
inside the `try` and the `finally` will consist of a null check and disposal of the `using` variable.

There is one small complication to the previous design: `switch` statements. `switch` statements
allow variable declarations and statements that are not nested within a single block. In addition,
variable scope breaks down in these cases because the scope of some variables in switch statements
are not equivalent to the *lifetime* of those variables. Fortunately, we don't have to get into
the details here because we are only concerned with variables declared as a part of statements,
which do not have differing lifetime and scope. Still, since the scopes overlap, it's not clear
where the start of a using variable begins and where its lexical scope ends.

TODO: Plan for switch statement

#### Implementation

The first step that needs to be taken is to add support for using variables to the 
`LocalDeclarationStatementSyntax`. This can be done by modifying Syntax.xml to contain a new
token for the `using` keyword, and augmenting the parser in `LanguageParser.cs` to recognize
and include the `using` keyword if present in that location.

After parsing, binding needs to be changed to recognize whether a given local variable is
a normal variable or a `using` variable. This change has two pieces: a change to `LocalSymbol`
to include a property indicating that this local is a using-var. Here's where terminology breaks 
down. We already have a notion of a using variable -- it's the variable inside a using statement.
We need some other flag to indicate that the variable is a using variable declared in a regular
`LocalDeclarationStatement`. Perhaps `IsLocalUsingVariable`? The binder in 
`BindLocalDeclarationStatement` will also have to be changed to pass the appropriate arguments
when constructing the `LocalSymbol`.

The tricky part will actually be constructing the try-finally's for each variable. There are
multiple reasonable appraoches. One solution to this would be add a new rewriter pass to
the "lowering" phase of the compiler. Normally this would be the kind of operation we would
perform inside the `LocalRewriter`, but unfortunately this change introduces new blocks, which
is not a great fit for the `LocalRewriter`.

The new pass would perform the following operations:

1. Override `VisitStatementList` to check if any statements are `ExpressionStatement`s. 
2. If so, check if the expression is a `BoundAssignmentExpression`. 
3. If so, check if the left-hand side is a "local using variable". 
4. If so, introduce a new `BoundUsingStatement` with a corresponding `BoundBlock`. Put all
   subsequent statements in the current statement list inside the synthesized bound block.
   Make the "using variable" the target of the using statement.

TODO: Handle switch statement.

By using this new pass to reduce using variables to regular using statements, we should
be able to rely on subsequent passes to compile the rest of the code correctly.