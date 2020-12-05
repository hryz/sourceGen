# C# v9 Source Generators demo.
Sometimes you need a simple in-memory cache, but with secondary indices.
Also, it's nice when they can have a complex key.
When you implement such minimalistic in-memory DB 
you have an interesting design decision - how to express 
index querying and index registration in such a way so that it's type safe, 
fool-proof, readable, low-ceremony, and performant.
One possible solution is using `(Expr<Func<TModel,TField>> field, TField value)`, 
but comparing expression trees is not trivial and allocation of Expr with all its internals is definitely not cheap.

## System Design
There are 3 parts of the solution:
- **In-memory database** - the DB engine (for the sake of simplicity this demo includes a naive impl.)
- **Client code**. It defines the models, cache collections within a DB facade, and registers indices. 
  (something similar to EF DBContext and Fluent definitions of indices etc)
- **Source Generator** - a piece of wizardry that generates the Extension methods for each model property 
  embedding the property type and name hardcoded into the method.
- **Source Generator Tests** - how are you gonna test a plugin for a compiler? 
  That's right! You assemble the compilation pipeline in the test. 
  You create an in-memory solution with projects with references and sources 
  and also references your generator to see that everything builds and it emits the code that you expect.

Source generators just like analyzers work as plugins for the compiler.
Whenever the compiler analyzes a new file (to highlight the syntax or detect issues before the build)
it passes the syntax and semantic models to these plugins. They can emit code or warnings/errors.

## The killer feature
Source generators are executed during the code analysis (not during the build).
This means that you can add a property and in a fraction of a second 
a new code based on this property is generated. 

## C# Source generators vs F# type providers
This is similar to F# type providers, 
but the biggest difference is the popularity. F# is a niche thing by itself, 
but writing a type provider is a niche thing in the F# community. 
This means that if something doesn't work - good luck!  
In contrast to this C# Code Generators are on top of the hype tide. 
Very soon we're going to see many useful things and tons of Q&As/blogs etc.