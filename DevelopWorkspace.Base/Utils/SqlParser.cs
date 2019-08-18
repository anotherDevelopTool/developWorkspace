using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevelopWorkspace.Base.Utils
{
    /// <summary>
    ///     Usage:
    ///         SqlParser parser = new SqlParser();
    ///         parser.Parse(sunQuerySql_8);
    /// </summary>
    public class SqlParser
    {

        /// <summary>
        /// 简单判断所入力的是否是一个SQL，注意，不是完整的去判断SQL文的正确语法性，这里只是面对数据做成支援，工具判断不到的地方需要人为的判断
        /// </summary>
        /// <param name="_querySQL"></param>
        /// <returns></returns>
        public bool IsSelectQuery(string _querySQL) {

            Regex token = new Regex(@"\bselect\b", RegexOptions.IgnoreCase);
            Match firstHitter = token.Match(_querySQL);
            if (!firstHitter.Success) return false;

            token = new Regex(@"\bfrom\b", RegexOptions.IgnoreCase);
            firstHitter = token.Match(_querySQL.Substring(firstHitter.Index));
            if (!firstHitter.Success) return false;

            //过于简单的SQL，比如只是一个单表查询而且连WHERE条件都没有的话就没有必要利用这个工具了，
            //token = new Regex(@"\b=\b", RegexOptions.IgnoreCase);
            //firstHitter = token.Match(_querySQL.Substring(firstHitter.Index));
            //if (!firstHitter.Success) return false;
            
            //不支持更新或者删除SQL文
            token = new Regex(@"\b(?:delete|update)\b", RegexOptions.IgnoreCase);
            firstHitter = token.Match(_querySQL);
            if (firstHitter.Success) return false;


            return true;
        }

        int KAKKOU_WIDTH = 1;
        public bool IsParsedSqlQuery = false;
        public Query TopQuery { get; set; }
        public List<Query> queries { get; set; }
        public bool Parse(string _querySQL,bool _querySplitOnly=false)
        {
            IsParsedSqlQuery = false;

            if (!IsSelectQuery(_querySQL)) return false;

            #region 子查询用检索正则式
            Regex queriesToken = new Regex(@"^(?<Total>(?:[^\(\)]|(?<Open>\()|(?<Content-Open>\)))+(?(Open)(?!)))$");
            Match m = queriesToken.Match(_querySQL);

            if (!m.Success) return false;

            List<Query> queries = new List<Query>();

            foreach (Capture capture in m.Groups["Content"].Captures)
            {
                //需要判断是否是select子查询，如果不是，需要skip
                Regex token = new Regex(@"^\s*select", RegexOptions.IgnoreCase);
                Match firstHitter = token.Match(capture.Value);
                if (firstHitter.Success)
                {
                    Query query = new Query() { Index = capture.Index, Length = capture.Length, Sql = capture.Value };
                    queries.Add(query);
                }
                System.Diagnostics.Debug.WriteLine(capture.Value);
            }
            //整体的SQL作为一个Query对象
            Query topQuery = new Query() { Index = m.Groups["Total"].Captures[0].Index, Length = m.Groups["Total"].Captures[0].Length, Sql = m.Groups["Total"].Captures[0].Value };
            queries.Add(topQuery);
            #endregion

            this.TopQuery = topQuery;
            this.queries = queries;

            //这里只作简单的子查询的分拆（记录开始位置和结束位置），注意这个类其他部分已不再使用（条件解析等机能）
            if (_querySplitOnly) goto lret;

            #region 解析第一阶段：在这个阶段是子查询和父查询之间关系通过List互联了
            //最顶层的为totalQuery
            foreach (Query query in queries)
            {
                //查找自己的直系父查询
                Query parent = (from wrapQuery in queries
                                where wrapQuery.Index < query.Index && wrapQuery.Index + wrapQuery.Length > query.Index + query.Length
                                orderby wrapQuery.Index descending
                                select wrapQuery).FirstOrDefault();
                if (parent != null)
                {
                    query.parent = parent;
                    parent.queries.Add(query);
                }
                else {
                    System.Diagnostics.Debug.WriteLine("#### top level sql");
                }
            }
            #endregion
            #region 解析第二阶段：针对每个子查询是否使用了alias别名
            Regex queryAliasAsTableToken = new Regex(@"^\s*(?<alias>(?!as\s+)\w*)\b", RegexOptions.IgnoreCase);
            Regex queryAliasAsColumnToken = new Regex(@"^\s*as\s+(?<alias>\w*)\b", RegexOptions.IgnoreCase);
            foreach (Query query in queries)
            {
                if (query.parent != null)
                {
                    string subSql = topQuery.Sql.Substring(query.Index + query.Length + KAKKOU_WIDTH);
                    Match mAliasToken = queryAliasAsTableToken.Match(subSql);
                    if (mAliasToken.Success)
                    {
                        query.SqlType = QueryType.TABLE;
                        query.AliasName = mAliasToken.Groups["alias"].Value;
                    }
                    else {
                        Match mAliasAsColumnToken = queryAliasAsColumnToken.Match(subSql);
                        if (mAliasAsColumnToken.Success)
                        {
                            query.SqlType = QueryType.COLUMN;
                            query.AliasName = mAliasAsColumnToken.Groups["alias"].Value;
                        }
                    }
                }
            }
            #endregion
            #region 取from之后的表名及其别名以及取select之后from之前的字段名
            foreach (Query query in queries)
            {
                string subSql = query.Sql;
                foreach (Query childQuery in (from sortedQuery in query.queries orderby sortedQuery.Index descending select sortedQuery))
                {
                    //如果有子查询，这把其剔除后再做解析动作
                    subSql = subSql.Substring(0, childQuery.Index - query.Index - KAKKOU_WIDTH * 1) +
                             childQuery.AliasName +
                             subSql.Substring(childQuery.Index + childQuery.Length - query.Index + KAKKOU_WIDTH * 1);
                }
                //取from之后的表名及其别名
                {
                    string from_where_section = subSql;
                    Regex token = new Regex(@"\bwhere\b|\border\s*?by\b", RegexOptions.IgnoreCase);
                    Match firstHitter = token.Match(from_where_section);
                    if (firstHitter.Success)
                    {
                        from_where_section = from_where_section.Substring(0, firstHitter.Index);
                    }
                    token = new Regex(@"\bfrom\b", RegexOptions.IgnoreCase);
                    firstHitter = token.Match(from_where_section);
                    if (firstHitter.Success)
                    {
                        from_where_section = from_where_section.Substring(firstHitter.Index + firstHitter.Length);
                    }

                    //分成两段一段是结合表之前的内容，另一段是join之后到where的内容
                    //如果没有结合的话那么直接就是from和where之间的内容作为分析对象
                    string from_join_section = from_where_section;
                    string join_where_section = null;

                    token = new Regex(@"(?:\bleft\b|\binner\b|\bright\b)", RegexOptions.IgnoreCase);
                    firstHitter = token.Match(from_join_section);
                    if (firstHitter.Success)
                    {
                        from_join_section = from_where_section.Substring(0, firstHitter.Index);
                        join_where_section = from_where_section.Substring(firstHitter.Index);
                    }
                    //目前这个正则有些bug,某些特殊的组合可能识别错误
                    //from table1 t1,table2,table3 as t3,table4 inner join
                    //如果碰到oracle的使用(+)等结合方式则不能正确识别，本人对(+)的写法内心很是排斥
                    token = new Regex(@"(?<tablename>\w+)(?:\s{0,})(?:as\s+|\s?)(?<aliasname>\w+|,|[ ]{0,})", RegexOptions.IgnoreCase);
                    MatchCollection allHitters = token.Matches(from_join_section);
                    foreach (Match hitter in allHitters)
                    {
                        query.SelectTables.Add(new KeyValuePair<string, string>(hitter.Groups["tablename"].Value, hitter.Groups["aliasname"].Value));
                    }
                    if (join_where_section != null)
                    {
                        token = new Regex(@"(?:\bleft\b|\binner\b|\bright\b)\s+?join\s+?(?<tablename>\w+)(?:\s{0,})(?:as\s+|\s?)(?!on\b)(?<aliasname>\w+|,|[ ])\b", RegexOptions.IgnoreCase);
                        allHitters = token.Matches(join_where_section);
                        foreach (Match hitter in allHitters)
                        {
                            query.JoinTables.Add(new KeyValuePair<string, string>(hitter.Groups["tablename"].Value, hitter.Groups["aliasname"].Value));
                        }

                        //目前只支持这样的格式：on t1.a=t2.a and t1.b=t2.b
                        token = new Regex(@"(?<left_tablename>\w+).(?<left_columnname>\w+)=(?<right_tablename>\w+).(?<right_columnname>\w+)", RegexOptions.IgnoreCase);
                        allHitters = token.Matches(join_where_section);
                        foreach (Match hitter in allHitters)
                        {
                            query.JoinConditions.Add(
                                new KeyValuePair<KeyValuePair<string, string>, KeyValuePair<string, string>>(
                                    new KeyValuePair<string, string>(hitter.Groups["left_tablename"].Value, hitter.Groups["left_columnname"].Value),
                                    new KeyValuePair<string, string>(hitter.Groups["right_tablename"].Value, hitter.Groups["right_columnname"].Value)
                                )
                            );
                        }
                    }
                }
                //取select之后from之前的字段名
                {
                    string select_from_section = subSql;
                    Regex token = new Regex(@"\bfrom\b", RegexOptions.IgnoreCase);
                    Match firstHitter = token.Match(select_from_section);
                    if (firstHitter.Success)
                    {
                        select_from_section = select_from_section.Substring(0, firstHitter.Index);
                    }
                    token = new Regex(@"\bselect\b", RegexOptions.IgnoreCase);
                    firstHitter = token.Match(select_from_section);
                    if (firstHitter.Success)
                    {
                        select_from_section = select_from_section.Substring(firstHitter.Index + firstHitter.Length);
                    }
                    token = new Regex(@"((?<tablename>\w+)(?:\.)(?<columnname>\w+)\b\s+as\s+(?<aliasname>\w+))|(?<tablename>\w+)(?:\.)(?<columnname>\w+)\b|(?<columnname>\w+)\b\s+as\s+(?<aliasname>\w+)|\b(?<columnname>\w+)\b", RegexOptions.IgnoreCase);
                    MatchCollection allHitters = token.Matches(select_from_section);
                    foreach (Match hitter in allHitters)
                    {
                        query.SelectColumns.Add(new KeyValuePair<string, string>(hitter.Groups["tablename"].Value, hitter.Groups["columnname"].Value));
                    }
                }

                //取where之后order by之前的字段名
                {
                    string where_orderby_section = subSql;
                    Regex token = new Regex(@"\border\b", RegexOptions.IgnoreCase);
                    Match firstHitter = token.Match(where_orderby_section);
                    if (firstHitter.Success)
                    {
                        where_orderby_section = where_orderby_section.Substring(0, firstHitter.Index);
                    }
                    token = new Regex(@"\bwhere\b", RegexOptions.IgnoreCase);
                    firstHitter = token.Match(where_orderby_section);
                    if (firstHitter.Success)
                    {
                        where_orderby_section = where_orderby_section.Substring(firstHitter.Index + firstHitter.Length);
                    }
                    token = new Regex(@"(?<left_tablename>\w+)\.(?<left_columnname>\w+)\s{0,}(?<op>like\b|between\b|<>|>=|<=|>|<|=)\s{0,}(?<right_tablename>\w+)\.(?<right_columnname>\w+)|(?<left_tablename>\w+)\.(?<left_columnname>\w+)\s{0,}(?<op>like\b|between\b|<>|>=|<=|>|<|=)\s{0,}(?<right_columnname>\w+)|(?<left_columnname>\w+)\s{0,}(?<op>like\b|between\b|<>|>=|<=|>|<|=)\s{0,}(?<right_columnname>\w+)", RegexOptions.IgnoreCase);
                    MatchCollection allHitters = token.Matches(where_orderby_section);
                    foreach (Match hitter in allHitters)
                    {
                        query.WhereConditions.Add(new string[] {
                            hitter.Groups["left_tablename"].Value,
                            hitter.Groups["left_columnname"].Value,
                            hitter.Groups["op"].Value,
                            hitter.Groups["right_tablename"].Value,
                            hitter.Groups["right_columnname"].Value });
                    }
                }
            }
            #endregion
            lret:
            IsParsedSqlQuery = true;
            return true;
        }
    }
    public enum QueryType
    {
        TABLE,
        COLUMN
    }
    public class Query
    {
        public Query()
        {
            SqlType = QueryType.COLUMN;
        }
        List<Query> _queries = new List<Query>();
        public int Index { get; set; }
        public int Length { get; set; }
        public string Sql { get; set; }
        public string AliasName { get; set; }
        public QueryType SqlType { get; set; }
        public List<KeyValuePair<string, string>> SelectColumns = new List<KeyValuePair<string, string>>();
        public List<string[]> WhereConditions = new List<string[]>();
        public List<KeyValuePair<string, string>> SelectTables = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> JoinTables = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<KeyValuePair<string, string>, KeyValuePair<string, string>>> JoinConditions = new List<KeyValuePair<KeyValuePair<string, string>, KeyValuePair<string, string>>>();
        public List<KeyValuePair<string, string>> OrderbyConditions = new List<KeyValuePair<string, string>>();

        public Query parent { get; set; }
        public List<Query> queries
        {
            get
            {
                return _queries;
            }
            set
            {
                _queries = value;
            }
        }
    }
}
