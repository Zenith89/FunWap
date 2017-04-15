# FUNW@AP
 is a DSL that able to interperate,compile and Generating IL Code 
 

Grammer 
<Module>::= {<Procudure>}+
<Procudure>::= fun identifier “(“ <arglist>“)” <retType> <Statments>
<retType>::= <type> | fun “(“ <typelist>“)” <retType>
<typelist>::= <type> | [<typelist>]
<type>::= numeric | boolean |string
<arglist>::= “( “ “) “ | “ ( “ identifier <type> [ , <arglist> ] “ ) “
<Statments>::= {<stmt>}+
<stmt>::= <varDeclarestmt>|<Printstmt>|<assigmentstmt> | <callstmt> | <ifstmt>
| < whilestmt > | <retstmt>
<varDeclarestmt>::= var identifier <type> “ ; “ | var identifier <type>=<BExpr> “;”
| var identifier fun =<callexpr> “;”
<Printstmt>::=printline <BExpr>“;”
<assigmentstmt>::= identifier “=“ <BExpr> “;” | identifier “=“ <Async>
<ifstmt>::=if <BExpr> “{“<Statments> } [ else “{“<Statments>“}”]
<whilestmt>::= while “(“ <Bexpr> ”)” “{“ <Statments> “}”
<retstmt>=return <Bexpr> “;”
<rexpr>=<BExpr> | fun “(“ <arglist>“)” <retType> <Statments> “;”
<Async>=“async” return “{“ <callexpr>“}”
<BExpr> ::= <LExpr> <LOGIC_OP> <LExpr>
<LExpr> ::= <Expr> <REL_OP ><Expr>
<Expr> ::= <Term> <ADD_OP ><Expr>
<Term>::= <Factor> <MUL_OP> <Term>
<Factor> ::= <numeric> | <string> | true | false | <identifier> | <callexpr> | “(“ <expr> “)” | {+|-|!}
<callexpr> ::= identifier “( “ “) “ | identifier “(“ <callpar> “)” “;”
<callpar>::=<BExpr> | [',< callpar >']
<LOGIC_OP> := “&&” | “||”
<REL_OP> := “>“ |” < “|” >=“ |” <=“ |” <>“ |” ==“
 <MUL_OP> := “*” |” /”
<ADD_OP> := “+” |” -”
<identifier>::= letter { letter | digit }
<numeric>::=digit
