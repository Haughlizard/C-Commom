using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Common;

namespace WinLib.DBUtility
{
    public abstract class DbHelperSQL
    {
        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.		
        public static string connStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        public DbHelperSQL() { }


        #region 执行sql语句
        /// <summary>
        /// 执行cmd.ExecuteNonQuery，返回受影响的行数
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqlstr, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlstr, conn))
                {
                    try
                    {
                        conn.Open();
                        if (parameters != null)
                        {
                            foreach (SqlParameter para in parameters)
                            {
                                cmd.Parameters.Add(para);
                            }
                        }
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlstr, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlstr, conn))
                {
                    try
                    {
                        conn.Open();
                        if (parameters != null)
                        {
                            foreach (SqlParameter para in parameters)
                            {
                                cmd.Parameters.Add(para);
                            }
                        }
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {

                        throw e;
                    }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sqlstr, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlstr, conn))
                {
                    conn.Open();
                    if (parameters != null)
                    {
                        foreach (SqlParameter para in parameters)
                        {
                            cmd.Parameters.Add(para);
                        }
                    }
                    DataSet ds = new DataSet();
                    try
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(ds);
                        cmd.Parameters.Clear();
                        return ds.Tables[0];
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {

                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 外部调用完成时候需要关闭dr.Close();
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sqlstr, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(sqlstr, conn);
            try
            {
                conn.Open();
                if (parameters != null)
                {
                    foreach (SqlParameter para in parameters)
                    {
                        cmd.Parameters.Add(para);
                    }
                }
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return dr;
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                throw e;
            }
        }
        #endregion

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            //循环
                            foreach (DictionaryEntry myDE in SQLStringList)
                            {
                                string cmdText = myDE.Key.ToString();
                                SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                int val = cmd.ExecuteNonQuery();
                                //cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }

                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的SqlParameter[]）</param>
        public static int ExecuteSqlTran(List<string> strLists)
        {
            int i = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            //循环
                            foreach (string strList in strLists)
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = strList;
                                cmd.Transaction = trans;
                                cmd.CommandType = CommandType.Text;
                                int val = cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                                i++;
                            }

                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
            return i;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 执行存储过程，用于插入、修改、删除。因希望执行的语句有返回数据，采用ExecuteScalar执行。
        /// 数据库的存储过程相应返回数据是：
        /// 插入-返回ID
        /// 修改、删除-返回影响的行数
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalarProceduce(string sqlstr, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlstr, conn))
                {
                    try
                    {
                        conn.Open();
                        if (parameters != null)
                        {
                            foreach (SqlParameter para in parameters)
                            {
                                cmd.Parameters.Add(para);
                            }
                        }
                        cmd.CommandType = CommandType.StoredProcedure;
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        return obj;
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {

                        throw e;
                    }
                }
            }
        }

        public static DataTable ExecuteQueryProceduce(string sqlstr, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(sqlstr, conn))
                {
                    try
                    {
                        conn.Open();
                        if (parameters != null)
                        {
                            foreach (SqlParameter para in parameters)
                            {
                                cmd.Parameters.Add(para);
                            }
                        }
                        cmd.CommandType = CommandType.StoredProcedure;
                        DataSet ds = new DataSet();
                        try
                        {
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            da.Fill(ds);
                            cmd.Parameters.Clear();
                            return ds.Tables[0];
                        }
                        catch (System.Data.SqlClient.SqlException e)
                        {

                            throw e;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException e)
                    {

                        throw e;
                    }
                }
            }
        }
    }
}
