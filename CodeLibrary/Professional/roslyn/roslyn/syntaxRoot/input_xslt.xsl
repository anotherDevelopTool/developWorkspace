grammar CSV;

@header {
using System.Collections.Generic;
using DevelopWorkspace.Base;
}

/** Derived from rule "file : hdr row+ ;" */
file
locals [int i=0]
     : hdr ( rows+=row[$hdr.text.Split(',')] {$i++;} )+
       {
       DevelopWorkspace.Base.Logger.WriteLine($i+" rows");
       foreach (RowContext r in $rows) {
           //DevelopWorkspace.Base.Logger.WriteLine("row token interval: "+r.getSourceInterval());
       }
       }
     ;

hdr : row[null] {DevelopWorkspace.Base.Logger.WriteLine("header: '"+$text+"'");} ;

/** Derived from rule "row : field (',' field)* '\r'? '\n' ;" */
row[string[] columns] returns [Dictionary<string,string> values]
locals [int col=0]
@init {
    $values = new Dictionary<string,string>();
}
@after {
    if ($values!=null && $values.Count>0) {
        DevelopWorkspace.Base.Logger.WriteLine("values = "+$values);
    }
}
// rule row cont'd...
    :   field
        {
        if ($columns!=null) {
            $values.Add($columns[$col++], $field.text);
        }
        }
        (   ',' field
            {
            if ($columns!=null) {
                $values.Add($columns[$col++], $field.text);
            }
            }
        )* '\r'? '\n'
    ;

field
    :   TEXT
    |   STRING
    |
    ;

TEXT : ~[,\n\r"]+ ;
STRING : '"' ('""'|~'"')* '"' ; // quote-quote is an escaped quote
