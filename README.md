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
