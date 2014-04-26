EtcScript
====

EtcScript is a scripting language with some experimental features. 


FEATURES
====

Inline Function Arguments
==

Functions in EtcScript can take the form of macros, rules, or tests, but all three types incorporate the types and position of arguments into their signature. Where in most programming languages you write 
```
void frobnicate(thing one, frobber two) {...}
```
in EtcScript you write
```
macro frobnicate (one:thing) with (two:frobber) {...}
```
When you call the function, you write 'frobnicate foo with bar'. 


Complex Strings
==

Code can be embedded into strings, to be evaluated at a later time. For example,
```
$"[room.printed-name]\n[room.description]\n"
```
is roughly eqivilent to 
```
lambda _ : string {
	return room.printed-name + "\n" + room.description + "\n";
}
```

Complex strings are automatically evaluated when they are assigned to a variable of type 'string'.


Indexing Via Special Macros
==

Any type can be turned into an indexable type simply by declaring the appropriate macros. For example, this is the implementation of these macros fro the list type, in C#.
```
Environment.AddSystemMacro(
	"SET AT (N:NUMBER) ON (L:LIST) TO (V:GENERIC)",
	(c, a) => { 
		(a[1] as List<Object>)[Convert.ToInt32(a[0])] = a[2]; 
		return null; 
	});

Environment.AddSystemMacro(
	"GET AT (N:NUMBER) FROM (L:LIST) : GENERIC",
	(c, a) => { 
		return (a[1] as List<Object>)[Convert.ToInt32(a[0])]; 
	});
```

Any type can be indexed using the @ operator if these macros are defined. 

This same mechanism is used to implement conversion and other mechanisms.


Syntax
===





More complex examples
===

Foreach In 
==

Foreach In is a simple control macro that implements a foreach loop. The syntax is 'foreach x in y', where x is the name of the local variable created and y is a term resulting in a list. The type of x is taken from the return type of y's get at macro. Foreach In won't work with types that don't have a defined get at macro. 

Here is a contrived example.
```
global f:list = { 1 2 3 4 };

test _ : number { #return the sum of all elements of f.
	var y = 0;
	foreach x in f {
		let y = y + x;
	}
	return y;
}
```

Since all lists in EtcScript are lists of generic, if you want to pass the element to another function you will have to cast it. You can, however, create strongly typed lists by exploiting the fact that Foreach In is implemented using the get at macro.

```
type aliased-list {}

#Call the built-in 'get at n from list' function, which returns a generic, then cast the result of that.
macro get at (n:number) from (l:aliased-list) : string { return ((l):list@n):string; } 

#Just to avoid some casting
macro convert (l:aliased-list) to list : list { return (l):list; }
macro convert (l:list) to aliased-list : aliased-list { return (l):aliased-list; }

#And a type-checked add function
macro add (a:string) (b:string) : string { return a + b; }

global f:aliased-list = { ""a"" ""b"" ""c"" };

test _ : string { #If f wasn't properly types, the call to 'add string string' would fail - Can't convert generic to string!
	var y = ""-"";
	foreach x in f {
		let y = [add y x];
	}
	return y;
}
```