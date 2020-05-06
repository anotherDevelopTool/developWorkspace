using System;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sprache;
using DevelopWorkspace.Base;
public class Script
{
    static readonly Parser<char> CellSeparator = Parse.Char(',');

    static readonly Parser<char> QuotedCellDelimiter = Parse.Char('"');

    static readonly Parser<char> QuoteEscape = Parse.Char('"');

    static Parser<T> Escaped<T>(Parser<T> following)
    {
        return from escape in QuoteEscape
               from f in following
               select f;
    }

    static readonly Parser<char> QuotedCellContent =
        Parse.AnyChar.Except(QuotedCellDelimiter).Or(Escaped(QuotedCellDelimiter));

    static readonly Parser<char> LiteralCellContent =
        Parse.AnyChar.Except(CellSeparator).Except(Parse.String(Environment.NewLine));

    static readonly Parser<string> QuotedCell =
        from open in QuotedCellDelimiter
        from content in QuotedCellContent.Many().Text()
        from end in QuotedCellDelimiter
        select content;

    static readonly Parser<string> NewLine =
        Parse.String(Environment.NewLine).Text();

    static readonly Parser<string> RecordTerminator =
        Parse.Return("").End().XOr(
        NewLine.End()).Or(
        NewLine);

    static readonly Parser<string> Cell =
        QuotedCell.XOr(
        LiteralCellContent.XMany().Text());

    static readonly Parser<IEnumerable<string>> Record =
        from leading in Cell
        from rest in CellSeparator.Then(_ => Cell).Many()
        from terminator in RecordTerminator
        select Cons(leading, rest);

    static readonly Parser<IEnumerable<IEnumerable<string>>> Csv =
        Record.XMany().End();

    static IEnumerable<T> Cons<T>(T head, IEnumerable<T> rest)
    {
        yield return head;
        foreach (var item in rest)
            yield return item;
    }





    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");

        var input = "a,b,c" + Environment.NewLine + "d,e,f";
        var r = Csv.Parse(input);
        var l1 = r.First().ToArray();
        var l2 = r.Skip(1).First().ToArray();
        DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(r));
        DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(l1));
        return "";
    }
}
