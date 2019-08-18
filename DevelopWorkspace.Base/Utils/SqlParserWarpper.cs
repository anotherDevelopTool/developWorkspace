using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Parser = Laan.Sql.Parser;
using Laan.Sql.Formatter;
using System.Text;
namespace DevelopWorkspace.Base.Utils
{
    /// <summary>
    ///     Usage:
    ///         这里面使用了开源的Lann.Sql.Parser来作为SQL的解析引擎
    ///         主要提供了SQLFormatter的功能
    ///         并通过SQL解析出来的语法树收集取得表对象，关联条件以及抽取条件，为数据做成提供情报
    /// </summary>
    public class SqlParserWrapper
    {
        public bool IsParsedSqlQuery = false;

        public List<Query> queries { get; set; }

        public class WhereCondition {
            public string leftTableName;
            public string LeftFieldName;
            public string op;
            public string rightTableName;
            public string rightFieldName;
            public string value;
        }
        //表别名情报收集，最终数据抽出以及做成时需要别名还原到是表名
        //目前通过statement的数结构遍历后得到了表别名一览，以及是表名一览，条件中的=的项目的收集（=以外的情况比较多，目前这个版本暂时不对应）
        //这个字典里面维护着虚拟SELECT表和实际表名（别名-实际表名）的对应关系，在后面的WHERE条件清洗的时候使用虚拟SELECT表名置换至对应关系中的第一个表的实际表名（虽然不严格，80%以上的SQL文基本符合这个规则）
        Dictionary<string, List<KeyValuePair<string, string>>> dVirtualSelect = new Dictionary<string, List<KeyValuePair<string, string>>>();
        //维护抽取实际表的一览
        List<KeyValuePair<string, string>> lRealSelect = new List<KeyValuePair<string, string>>();
        //维护抽取条件
        List<WhereCondition> lWhereCondition = new List<WhereCondition>();
        /// <summary>
        /// 简单判断所入力的是否是一个SQL，注意，不是完整的去判断SQL文的正确语法性，这里只是面对数据做成支援，工具判断不到的地方需要人为的判断
        /// </summary>
        /// <param name="_querySQL"></param>
        /// <returns></returns>
        public bool IsSelectQuery(string _querySQL) {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">解析用的SQL文</param>
        /// <param name="bErrorOutput">SQL解析时的出错信息是否抑制</param>
        /// <returns></returns>
        public bool Parse(string sql,bool bErrorOutput = true)
        {
            dVirtualSelect.Clear();
            lRealSelect.Clear();
            lWhereCondition.Clear();
            try
            {
                //SQL解析g
                var statements = Parser.ParserFactory.Execute(sql);

                 if (statements.Count > 0)
                {
                    //SQL解析树的遍历，数据做成信息取得
                    foreach (var statement in statements)
                    {
                        if (statement is Parser.Entities.SelectStatement)
                        {
                            RecurseTableName((Parser.Entities.SelectStatement)statement);
                        }
                    }

                    //子查询分割（暂定版，需要优化）
                    SqlParser parser = new SqlParser();
                    parser.Parse(sql, true);
                    queries = parser.queries;

                    //抽取实际表名一览
                    //var lRealTables = (from pair in lRealSelect select new { pair.Value }).Distinct();
                    //抽取条件的别名替换
                    foreach (string virtualTableName in dVirtualSelect.Keys)
                    {
                        foreach (WhereCondition whereCondition in lWhereCondition)
                        {
                            if (virtualTableName.Equals(whereCondition.leftTableName))
                            {
                                whereCondition.leftTableName = dVirtualSelect[virtualTableName][0].Value;
                            }
                            if (virtualTableName.Equals(whereCondition.rightTableName))
                            {
                                whereCondition.rightTableName = dVirtualSelect[virtualTableName][0].Value;
                            }

                        }
                    }
                    IsParsedSqlQuery = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                if(bErrorOutput) Logger.WriteLine(Environment.NewLine + ex.ToString(),Level.ERROR);
            }
            IsParsedSqlQuery = false;
            return false;

        }

        public List<KeyValuePair<string, string>> SelectedTables() {
            //var lRealTables = (from pair in lRealSelect select new { pair.Value }).Distinct();
            return lRealSelect;
        }
        public List<WhereCondition> WhereConditiones() { 
            return lWhereCondition;
        }

        //通过这个方式递归出抽取表一览情报
        //利用同样的方式获取Join的表信息，ON条件信息，WHERE信息，并对条件值进行简单的预测后做成数据
        private void RecurseTableName(Parser.Entities.SelectStatement statement, List<KeyValuePair<string, string>> aliasList = null)
        {
            foreach (Parser.Entities.Table table in (statement as Parser.Entities.SelectStatement).From)
            {
                if (table is Parser.Entities.DerivedTable)
                {
                    Parser.Entities.DerivedTable derivedTable = (Parser.Entities.DerivedTable)table;
                    if (!derivedTable.Alias.Name.Equals(""))
                    {
                        List<KeyValuePair<string, string>> _aliasList = new List<KeyValuePair<string, string>>();
                        dVirtualSelect.Add(derivedTable.Alias.Name, _aliasList);
                        RecurseTableName(derivedTable.SelectStatement, _aliasList);
                    }
                    else
                    {
                        RecurseTableName(derivedTable.SelectStatement);
                    }
                }
                else
                {
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(table.Alias.Name, table.Name);
                    //在此收集抽取表名信息
                    if (aliasList != null)
                    {
                        aliasList.Add(pair);
                    }
                    lRealSelect.Add(pair);
                }
                foreach (Parser.Entities.Join join in table.Joins)
                {
                    if (join.Condition is Parser.Expressions.CriteriaExpression)
                    {
                        RecurseCriteriaExpression((Parser.Expressions.CriteriaExpression)join.Condition, statement);
                    }
                    if (join is Parser.Entities.DerivedJoin)
                    {
                        Parser.Entities.DerivedJoin derivedJoin = (Parser.Entities.DerivedJoin)join;
                        if (!derivedJoin.Alias.Name.Equals(""))
                        {
                            List<KeyValuePair<string, string>> _aliasList = new List<KeyValuePair<string, string>>();
                            dVirtualSelect.Add(derivedJoin.Alias.Name, _aliasList);
                            RecurseTableName(derivedJoin.SelectStatement, _aliasList);
                        }
                        else
                        {
                            RecurseTableName(derivedJoin.SelectStatement);
                        }
                    }
                    else
                    {
                        KeyValuePair<string, string> pair = new KeyValuePair<string, string>(join.Alias.Name, join.Name);
                        //在此收集抽取表名信息
                        if (aliasList != null)
                        {
                            aliasList.Add(pair);
                        }
                        lRealSelect.Add(pair);
                    }
                }
            }
            if (statement.Where is Parser.Expressions.NegationExpression)
            {
                RecurseNegationExpression((Parser.Expressions.NegationExpression)statement.Where, statement);
            }
            if (statement.Where is Parser.Expressions.CriteriaExpression)
            {
                RecurseCriteriaExpression((Parser.Expressions.CriteriaExpression)statement.Where, statement);
            }
        }
        private void RecurseNegationExpression(Parser.Expressions.NegationExpression expression, Parser.Entities.SelectStatement statement)
        {
            if (expression.Expression is Parser.Expressions.CriteriaExpression)
            {
                RecurseCriteriaExpression((Parser.Expressions.CriteriaExpression)expression.Expression, statement);
            }
        }
        private void RecurseCriteriaExpression(Parser.Expressions.CriteriaExpression expression, Parser.Entities.SelectStatement statement)
        {
            if (expression.Left is Parser.Expressions.NestedExpression)
            {
                RecurseNestedExpress((Parser.Expressions.NestedExpression)expression.Left, statement);
            }
            if (expression.Left is Parser.Expressions.CriteriaExpression)
            {
                RecurseCriteriaExpression((Parser.Expressions.CriteriaExpression)expression.Left, statement);
            }
            if (expression.Left is Parser.Expressions.FunctionExpression)
            {
                Parser.Expressions.FunctionExpression func = expression.Left as Parser.Expressions.FunctionExpression;
                foreach (var argument in func.Arguments)
                {
                    if (argument is Parser.Expressions.SelectExpression && (argument as Parser.Expressions.SelectExpression).Statement is Parser.Entities.SelectStatement)
                    {
                        RecurseTableName((argument as Parser.Expressions.SelectExpression).Statement);
                    }
                }
            }
            if (expression.Right is Parser.Expressions.NestedExpression)
            {
                RecurseNestedExpress((Parser.Expressions.NestedExpression)expression.Right, statement);
            }
            if (expression.Right is Parser.Expressions.CriteriaExpression)
            {
                RecurseCriteriaExpression((Parser.Expressions.CriteriaExpression)expression.Right, statement);
            }
            if (expression.Right is Parser.Expressions.FunctionExpression)
            {
                Parser.Expressions.FunctionExpression func = expression.Right as Parser.Expressions.FunctionExpression;
                foreach (var argument in func.Arguments)
                {
                    if (argument is Parser.Expressions.SelectExpression
                        && (argument as Parser.Expressions.SelectExpression).Statement is Parser.Entities.SelectStatement)
                    {
                        RecurseTableName((argument as Parser.Expressions.SelectExpression).Statement);
                    }
                }
            }
            //Where里面的字段对字段的条件抽出： 字段 op 数字 或者 字段 op 字段的场合
            if (expression.Left is Parser.Expressions.IdentifierExpression
                && expression.Right is Parser.Expressions.IdentifierExpression)
            {
                Parser.Expressions.IdentifierExpression identifierExpressionLeft = (Parser.Expressions.IdentifierExpression)expression.Left;
                Parser.Expressions.IdentifierExpression identifierExpressionRight = (Parser.Expressions.IdentifierExpression)expression.Right;
                //如果抽取条件里面没有表别名的话，那么需要到所属的SelectStatmement的From属性里去取
                string tablename = "";
                if (identifierExpressionLeft.Parts.Count == 1 && identifierExpressionRight.Parts.Count == 1)
                {
                    tablename = (statement.From[0] as Parser.Entities.Table).Name;
                }
                WhereCondition whereCondition = new WhereCondition();
                whereCondition.op = expression.Operator;
                lWhereCondition.Add(whereCondition);
                if (identifierExpressionLeft.Parts.Count == 1)
                {
                    whereCondition.leftTableName = tablename;
                    whereCondition.LeftFieldName = identifierExpressionLeft.Parts[0];
                }
                else
                {
                    whereCondition.leftTableName = identifierExpressionLeft.Parts[0];
                    whereCondition.LeftFieldName = identifierExpressionLeft.Parts[1];
                }
                //TODO 需要判断不是表字段,这里按一般经验原则，如果是单个值的写法，就认为是Value
                if (identifierExpressionRight.Parts.Count == 1)
                {
                    whereCondition.value = identifierExpressionRight.Parts[0];
                }
                else
                {
                    whereCondition.rightTableName = identifierExpressionRight.Parts[0];
                    whereCondition.rightFieldName = identifierExpressionRight.Parts[1];
                }
                //对alias别名进行实际表名替换（Todo：对字段的别名也需要考虑的了）
                var lFromTables = from table in statement.From select new { table.Alias, table.Name };
                var lJoinTables = from table in statement.From[0].Joins select new { table.Alias, table.Name };
                foreach (var table in lFromTables.Union(lJoinTables))
                {
                    if (table.Alias.Value.Trim().Equals(whereCondition.leftTableName))
                    {
                        whereCondition.leftTableName = table.Name;
                    }
                    if (table.Alias.Value.Trim().Equals(whereCondition.rightTableName))
                    {
                        whereCondition.rightTableName = table.Name;
                    }
                }
            }
            //字段 op 文字列的场合
            if (expression.Left is Parser.Expressions.IdentifierExpression
                && expression.Right is Parser.Expressions.StringExpression)
            {
                Parser.Expressions.IdentifierExpression identifierExpressionLeft = (Parser.Expressions.IdentifierExpression)expression.Left;
                Parser.Expressions.StringExpression stringExpressionRight = (Parser.Expressions.StringExpression)expression.Right;
                //如果抽取条件里面没有表别名的话，那么需要到所属的SelectStatmement的From属性里去取
                string tablename = "";
                if (identifierExpressionLeft.Parts.Count == 1)
                {
                    tablename = (statement.From[0] as Parser.Entities.Table).Name;
                }
                WhereCondition whereCondition = new WhereCondition();
                whereCondition.op = expression.Operator;
                lWhereCondition.Add(whereCondition);
                if (identifierExpressionLeft.Parts.Count == 1)
                {
                    whereCondition.leftTableName = tablename;
                    whereCondition.LeftFieldName = identifierExpressionLeft.Parts[0];
                }
                else
                {
                    whereCondition.leftTableName = identifierExpressionLeft.Parts[0];
                    whereCondition.LeftFieldName = identifierExpressionLeft.Parts[1];
                }

                whereCondition.value = stringExpressionRight.Content;

                //对alias别名进行实际表名替换（Todo：对字段的别名也需要考虑的了）
                var lFromTables = from table in statement.From select new { table.Alias, table.Name };
                var lJoinTables = from table in statement.From[0].Joins select new { table.Alias, table.Name };
                foreach (var table in lFromTables.Union(lJoinTables))
                {
                    if (table.Alias.Value.Trim().Equals(whereCondition.leftTableName))
                    {
                        whereCondition.leftTableName = table.Name;
                    }
                }
            }

            //字段 in (值1，值2，值3)的场合
            if (expression.Left is Parser.Expressions.IdentifierExpression
                && expression.Operator.ToLower().Equals("in")
                && expression.Right is Parser.Expressions.NestedExpression)
            {
                Parser.Expressions.IdentifierExpression identifierExpressionLeft = (Parser.Expressions.IdentifierExpression)expression.Left;
                Parser.Expressions.NestedExpression nestedExpressionRight = (Parser.Expressions.NestedExpression)expression.Right;
                //如果抽取条件里面没有表别名的话，那么需要到所属的SelectStatmement的From属性里去取
                string tablename = "";
                if (identifierExpressionLeft.Parts.Count == 1)
                {
                    tablename = (statement.From[0] as Parser.Entities.Table).Name;
                }
                WhereCondition whereCondition = new WhereCondition();
                whereCondition.op = expression.Operator;
                lWhereCondition.Add(whereCondition);
                if (identifierExpressionLeft.Parts.Count == 1)
                {
                    whereCondition.leftTableName = tablename;
                    whereCondition.LeftFieldName = identifierExpressionLeft.Parts[0];
                }
                else
                {
                    whereCondition.leftTableName = identifierExpressionLeft.Parts[0];
                    whereCondition.LeftFieldName = identifierExpressionLeft.Parts[1];
                }

                whereCondition.value = nestedExpressionRight.Value;

                //对alias别名进行实际表名替换（Todo：对字段的别名也需要考虑的了）
                var lFromTables = from table in statement.From select new { table.Alias, table.Name };
                var lJoinTables = from table in statement.From[0].Joins select new { table.Alias, table.Name };
                foreach (var table in lFromTables.Union(lJoinTables))
                {
                    if (table.Alias.Value.Trim().Equals(whereCondition.leftTableName))
                    {
                        whereCondition.leftTableName = table.Name;
                    }
                }
            }
        }
        private void RecurseNestedExpress(Parser.Expressions.NestedExpression expression, Parser.Entities.SelectStatement statement)
        {
            if (expression.Expression is Parser.Expressions.CriteriaExpression)
            {
                RecurseCriteriaExpression((Parser.Expressions.CriteriaExpression)expression.Expression, statement);
            }
            if (expression.Expression is Parser.Expressions.SelectExpression)
            {
                RecurseTableName((expression.Expression as Parser.Expressions.SelectExpression).Statement);
            }

            if (expression.Expression is Parser.Expressions.FunctionExpression)
            {
                Parser.Expressions.FunctionExpression func = expression.Expression as Parser.Expressions.FunctionExpression;
                foreach (var argument in func.Arguments)
                {
                    if (argument is Parser.Expressions.SelectExpression)
                    {
                        RecurseTableName((argument as Parser.Expressions.SelectExpression).Statement);
                    }
                }
            }

        }

        public string format(string sql)
        {
            string output = "";

            var engine = new FormattingEngine();
            try
            {
                output = engine.Execute(sql);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(Environment.NewLine + ex.ToString(), Level.ERROR);
                return sql;
            }
            //sqlOutput.DataSource = output.Split(new[] { "\r\n" }, StringSplitOptions.None);
            return output;
        }
    }
}
