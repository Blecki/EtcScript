# EtcScript


EtcScript is a scripting language with some experimental features. 


## FEATURES


### Inline Function Arguments

Functions in EtcScript can take the form of macros, rules, or tests, but all three types incorporate the types and position of arguments into their signature. Where in most programming languages you write 
```
void frobnicate(thing one, frobber two) {...}
```
in EtcScript you write
```
macro frobnicate (one:thing) with (two:frobber) {...}
```
When you call the function, you write 'frobnicate foo with bar'. 

### Rule-Based Programming

Rule-based programming is an uncommon programming paradigm where rules are grouped into rulebooks and used to synthesize functions at runtime. Rules look like functions, but can also have when clauses, and statements that control the way they are ordered in the rulebook. While functions are called directly, rulebooks are invoked using the 'consider' keyword. All rules with the same signature, disregarding parameter types (but not return type), are grouped into a single rulebook. When a rule is considered, all rules matching that signature are considered. If the rule applies, it's code is run.

This is an example of a common idiom in the implementation of a game written in EtcScript.
```
Default of rule can (actor) take (item) : boolean { return true; }
Rule (actor:object) takes (item:object) { return continue; }

Macro (actor:object) takes (item:object) {
	if [consider [can actor take item]] {
		move item to actor;
		consider [actor takes item];
	}
}
```
The macro 'actor takes object' first invokes the rulebook 'can actor take object' to see if the action is allowed, and if so, it considers 'actor takes object'. The second rule allows the game to hang custom behavior on any take action.

```
Global player:object = ...;
Global hand-held-radio:object = ...;

Rule (player) takes (hand-held-radio) {
	# Code that only runs when the player takes the hand-held-radio.
	return continue;
}
```

Rules can be specialized many ways. The previous example showed specialization on global variables. The rule will only run if the arguments passed to consider are equal to 'player' and 'hand-held-radio'. Rules can also be specialized by type.

```
Rule (p:actor) enters (v:vehicle) { ... } # Only runs if p is a person and v is a vehicle.
```

Or rules can be specialized on any logic using a when clause.
```
Rule can (p:actor) take (a:ammo) : boolean when ammo-count >= max-ammo { return false; }
```
Rules are executed in order. In the case of rules that return values (Any rule with a return type declared that is not the built-in type 'rule-result'), the first rule that applies is executed and no others. For rules that return rule-result (Effectively returning nothing), each rule that applies is executed until one returns 'stop'.

Rule order can be modified using directives on rule declarations.
```
Rule foo order first {...}
Rule foo order last {...}
Rule foo with high priority {...}
Rule foo with low priority {...}
Rule foo order first with high priority {...}
```
Rules are first grouped into three catagories. Those marked order first, order last, and those with no order clause. Their order should be obvious. 
Within the grouping, rules are sorted by specificity as well as the compiler can. Each argument is considered in order. Comparing rule A and B, if A is declared to take specific global and B is not, A is considered more specific. If A is declared to take a type more derived than B, A is considered more specific. If A and B are declared to take identicle types, move on to the next argument.
If all arguments are identicle, then the rule with a when clause is more specific.
If both or neither rules have when clauses, then the high and low priority clauses are used to determine specificity.
If the two rules are in every way equally specific, then whichever one is declared in the source first will be considered first.


### Complex Strings

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


### Indexing Via Special Macros

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

### List of overloadable macros
Indexing is only an example of what can be done by defining specific macros. Here is a list of everything overloadable.

Script-defined types intrinsictly support member access, but types bound by the host application do not. You can bind these macros to implement it.
- GET X FROM (O:OBJECT-TYPE) : MEMBER-TYPE {...} - Implement member access, as in 'O.X'.
- SET X ON (O:OBJECT-TYPE) TO (M:MEMBER-TYPE) {...} - Implement member access for assignment, as in 'let O.X = foo;'.
- GET (N:STRING) FROM (O:OBJECT-TYPE) : MEMBER-TYPE {...} - Generic member access; N is the name of the member.
- SET (N:STRING) ON (O:OBJECT-TYPE) TO (M:MEMBER-TYPE) {...} - Generic member access for assignment.


- CONVERT (A:TYPE-A) TO TYPE-B : TYPE-B {...} - Define an implicit conversion from A to B. Conversions are considered when matching macros, but not when considering rules. 

As an example, conversion from complex-string to string is defined in the standard library by this macro
```
MACRO CONVERT (C:COMPLEX-STRING) TO STRING : STRING { RETURN [INVOKE [C]]:STRING; }
```

- LENGTH OF (X:TYPE) : NUMBER {...} - Actually, the return type can be any type, but it doesn't make much sense to use anything but number here.

- (A:TYPE-A) [operator] (B:TYPE-B) : TYPE-C {...} - Overload an operator, where [operator] is the operator and the types are anything at all. 

As an example, this operator appears in the standard library.
```
MACRO (S:STRING) + (C:COMPLEXSTRING) : STRING { RETURN S + [INVOKE [C]]:STRING; }
```

## Syntax

This psuedo-BNF is a semi-formal specification of the language's grammar.

- Script				:= (MacroDeclaration | RuleDeclaration | TypeDeclaration | GlobalDeclaration | Include)*
- GlobalDeclaration	:= "global" + Identifier + (: + Identifier)? + (= + Expression)? + ;
- MacroDeclaration	:= "macro" + DeclarationHeader + (: + Identifier)? + Block
- DeclarationHeader	:= (DeclarationTerm)*
- RuleDeclaration		:= "rule" + DeclarationHeader + WhenClause? + OrderClause? + PriorityClause? + Block
- WhenClause			:= "when" + Expression
- Lambda				:= "lambda" + DeclarationHeader + (: + Identifier)? + Block
- DeclarationTerm		:= (Identifier + "?"?) | (String + "?"?) | ( "(" + Identifier + (: + Identifier)? + ")" )
- Identifier			:= [a-z, A-Z, any non-operator symbol][any non-delimeter symbol]*
- Block				:= { + Statement+ + }
- Statement			:= Let | If | Invokation + ; | DynamicInvokation + ; | BracketInvokation + ; | Control | LocalDeclaration
- Let					:= "let" + (Identifier | MemberAccess | Indexer) + "=" + Expression + ;
- If					:= "if" + Expression + Block
- MemberAccess		:= Term + (StaticMember | DynamicMember)
- StaticMember		:= "." + Identifier
- DynamicMember		:= "?" + Identifier + ":" + Term
- Indexer				:= Term + @ + Term
- Expression			:= Term | BinaryOperation | New | Lambda
- Term				:= Parenthetical | Identifier | Number | BasicString | ComplexString | MemberAccess | Indexer | BracketInvokation | DynamicInvokation | Cast | List
- Parenthetical		:= "(" + Expression + ")"
- Cast				:= Term + : + Identifier
- BinaryOperation		:= Term + Operator + Term
- Invokation			:= Term+
- BracketInvokation	:= "[" + Invokation + "]"
- Control				:= Invokation + (Block | ;)
- Operator			:= Any of operators specified in operator table
- BasicString			:= Ordinary quote-delimited string				 
- ComplexString		:= @ + \" + ( Text | EmbeddedExpression )* + \"		
- Text				:= Any sequence of characters except [ or ", matched greedily.
- EmbeddedExpression	:= [ + Expression + ]
- LocalDeclaration	:= "var" + Identifier + (: + Identifier)? + ("=" + Expression)? + ;
- TypeDeclaration		:= "type" + Identifier + (: + Identifier)? + { + MemberDeclaration* + }
- MemberDeclaration	:=  "var" + Identifier + (: + Identifier)? + ;
- New					:= "new" + Identifier + InitializerBlock?
- InitializerBlock	:= { + InitializerItem* + }
- InitializerItem		:= "let" + Identifier + "=" + Expression + ;
- List				:= { + Term* + }
- Include				:= "include" + BasicString






# More complex examples

## Foreach In 


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

test _ : string { #If f wasn't properly typed, the call to 'add string string' would fail - Can't convert generic to string!
	var y = ""-"";
	foreach x in f {
		let y = [add y x];
	}
	return y;
}
```