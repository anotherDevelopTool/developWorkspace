grammar MyGrammar;

/*
 * Parser Rules
 */
@header {
using DevelopWorkspace.Base;
}

program
  : expression {
      DevelopWorkspace.Base.Logger.WriteLine($expression.v);
      DevelopWorkspace.Base.Logger.WriteLine($text);
    }
;

expression returns [string v] 
  : '(' expression ')'    {$v = '(' + $expression.v + ')';}
  | a=expression operate = ('*' | '/') b=expression {$v = $a.v +"MUL" + $b.v;}
  | a=expression operate = ('+' | '-') b=expression {$v = $a.v +"ADD" + $b.v;}  
  | INT {$v = $INT.int.ToString();} 
;


/*
 * Lexer Rules
 */

ADD : '+' ;
SUB : '-' ;
MUL : '*' ;
DIV : '/' ;

INT : '0'..'9'+ ;

WS : [ \t\r\n]+ -> skip ;
