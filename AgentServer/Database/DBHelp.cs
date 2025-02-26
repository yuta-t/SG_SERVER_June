using System;
//using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections;
using MySql.Data.MySqlClient;


namespace AgentServer.Database
{
    public sealed class DBHelp
    {
        public static bool CheckParams(params object[] args)//防SQL注入
        {
            string[] Lawlesses = { "=", "", "\'", "-", " " };
            if (Lawlesses == null || Lawlesses.Length <= 0) return true;
            //构造正则表达式,例:Lawlesses是=号和号,则正则表达式为 .*[=}].* (正则表达式相关内容请见MSDN)
            //另外,由于我是想做通用而且容易修改的函数,所以多了一步由字符数组到正则表达式,实际使用中,直接写正则表达式亦可;


            string str_Regex = ".*[";
            for (int i = 0; i < Lawlesses.Length - 1; i++)
                str_Regex += Lawlesses[i] + "|";
            str_Regex += Lawlesses[Lawlesses.Length - 1] + "].*";
            //
            foreach (object arg in args)
            {
                if (arg is string)//如果是字符串,直接检查
                {
                    if (Regex.Matches(arg.ToString(), str_Regex).Count > 0)
                        return false;
                }
                else if (arg is ICollection)//如果是一个集合,则检查集合内元素是否字符串,是字符串,就进行检查
                {
                    foreach (object obj in (ICollection)arg)
                    {
                        if (obj is string)
                        {
                            if (Regex.Matches(obj.ToString(), str_Regex).Count > 0)
                                return false;
                        }
                    }
                }
            }
            return true;
        }
        public static string ReadTableToString(string query, string column)
        {
            string result = string.Empty;
            MySqlConnection conn = new MySqlConnection(Conf.Connstr);
            MySqlCommand command = conn.CreateCommand();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            try
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader(); //execure the reader
                while (reader.Read())
                {
                    result = reader.GetString(column);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = string.Empty;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
        public static int ReadTableToInt(string query, string column)
        {
            int result = 0;
            MySqlConnection conn = new MySqlConnection(Conf.Connstr);
            MySqlCommand command = conn.CreateCommand();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            try
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader(); //execure the reader
                while (reader.Read())
                {
                    result = reader.GetInt32(column);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = 0;
            }
            finally
            {
                cmd.Cancel();
                conn.Close();
                conn.Dispose();
            }
            return result;
        }
        public static bool ExecuteCommand(string query)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(Conf.Connstr);
            MySqlCommand command = conn.CreateCommand();
            try
            {
                conn.Open();
                command.CommandText = query;
                command.ExecuteNonQuery();
                result = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return result;
        }
        public static bool CheckTable(string query)
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection(Conf.Connstr);
            MySqlCommand command = conn.CreateCommand();
            MySqlCommand cmd = new MySqlCommand(query, conn);
            try
            {
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader(); //execure the reader
                while (reader.Read())
                {
                    result = reader.HasRows;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = false;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
    }
}
