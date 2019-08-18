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
    public class QuestionnaireGrammar
    {
        public static Questionnaire ParseQuestionnaire(string questionnaire)
        {
            return Questionnaire.End().Parse(questionnaire);
        }

        internal static Parser<string> QuotedText =
            (from lquot in Parse.Char('"')
             from content in Parse.CharExcept('"').Many().Text()
             from rquot in Parse.Char('"')
             select content).Token();

        internal static Parser<string> Identifier = Parse.Letter.AtLeastOnce().Text().Token();

        internal static Parser<AnswerType> AnswerTypeIndicator =
            Parse.Char('#').Return(AnswerType.Natural)
                .Or(Parse.Char('$').Return(AnswerType.Number))
                .Or(Parse.Char('%').Return(AnswerType.Date))
                .Or(Parse.Char('?').Return(AnswerType.YesNo));

        internal static Parser<Question> Question =
            from at in AnswerTypeIndicator.Or(Parse.Return(AnswerType.Text))
            from id in Identifier
            from prompt in QuotedText
            select new Question(id, prompt, at);

        internal static Parser<Section> Section =
            from id in Identifier
            from title in QuotedText
            from lbracket in Parse.Char('[').Token()
            from questions in Question.AtLeastOnce()
            from rbracket in Parse.Char(']').Token()
            select new Section(id, title, questions);

        internal static Parser<Questionnaire> Questionnaire =
            from sections in Section.AtLeastOnce()
            select new Questionnaire(sections);
    }

    public enum AnswerType
    {
        Text,
        Date,
        Natural,
        Number,
        YesNo
    }
    public class Question
    {
        readonly string _id;
        readonly string _prompt;
        readonly AnswerType _answerType;

        public Question(string id, string prompt, AnswerType answerType)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (prompt == null) throw new ArgumentNullException("prompt");

            _prompt = prompt;
            _id = id;
            _answerType = answerType;
        }

        public string Id
        {
            get { return _id; }
        }

        public AnswerType AnswerType
        {
            get { return _answerType; }
        }

        public string Prompt
        {
            get { return _prompt; }
        }
    }
    public class Questionnaire
    {
        readonly IEnumerable<Section> _sections;

        public Questionnaire(IEnumerable<Section> sections)
        {
            if (sections == null) throw new ArgumentNullException("sections");

            _sections = sections.ToArray();
        }

        public IEnumerable<Section> Sections { get { return _sections; } }
    }
    public class Section
    {
        readonly string _id;
        readonly string _title;
        readonly IEnumerable<Question> _questions;

        public Section(string id, string title, IEnumerable<Question> questions)
        {
            if (id == null) throw new ArgumentNullException("id");
            if (title == null) throw new ArgumentNullException("title");
            if (questions == null) throw new ArgumentNullException("questions");

            _id = id;
            _title = title;
            _questions = questions.ToArray();
        }

        public string Id { get { return _id; } }

        public string Title { get { return _title; } }

        public IEnumerable<Question> Questions { get { return _questions; } }
    }

    public static void Main(string[] args)
    {
        DevelopWorkspace.Base.Logger.WriteLine("Process called");

        var input = args[0];
        var parsed = QuestionnaireGrammar.ParseQuestionnaire(input);


        //DevelopWorkspace.Base.Logger.WriteLine(DevelopWorkspace.Base.Dump.ToDump(r));
    }
}
