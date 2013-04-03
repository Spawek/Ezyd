using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ezyd.Models
{
    public class DataBase
    {
        public MySqlDataReader reader; //always use that reader (its automaticly closing old connections so its much easier)

        private MySqlConnection conn = new MySqlConnection();

        public DataBase(String _server, String _database, String _UID, String _password = "")
        {
            Debug.Assert(!String.IsNullOrEmpty(_server), "_server is null or empty.");
            Debug.Assert(!String.IsNullOrEmpty(_database), "_database is null or empty.");
            Debug.Assert(!String.IsNullOrEmpty(_UID), "_UID is null or empty.");

            MySqlConnectionStringBuilder cntStrBuilder = new MySqlConnectionStringBuilder();
            cntStrBuilder.Server = _server;
            cntStrBuilder.Database = _database;
            cntStrBuilder.UserID = _UID;
            cntStrBuilder.Password = _password;
            cntStrBuilder.CharacterSet = "utf8";

            conn.ConnectionString = cntStrBuilder.ToString();
            
            /*
            conn.ConnectionString =
                "server=" + _server + ";"
                + "database=" + _database + ";"
                + "uid=" + _UID + ";"
                + "password=" + _password + ";";
            */

            try
            {
                conn.Open();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show("ERROR - cannot open " + _server + " connection to database " + _database + " as user " + _UID);
                throw;
            }         
        }

        public bool isOpen() //check if connection is open
        {
            return conn.State.Equals(ConnectionState.Open);
        }

        public void readTable(String tableName) //write all records from tableName to console
        {
            Debug.Assert(!String.IsNullOrEmpty(tableName), "tableName is null or empty.");

            if (reader != null) //old reader has to be closed before making new query
                reader.Close();

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM `" + tableName + "`";
            System.Console.WriteLine(cmd.CommandText);
            reader = cmd.ExecuteReader();

            int counter = 0;
            while (reader.Read())
            {
                System.Console.WriteLine("record " + counter++ + ": ");
                for (int i = 0; i < reader.VisibleFieldCount; i++)
                {
                    System.Console.Write(reader[i] + " ");
                }
                System.Console.WriteLine();
            }
            reader.Close();
        }

        public MySqlDataReader SqlQuery(String _SqlQuery, bool wantToLogIt = true)//query with reader output (important - it returns this object reader (not a new one) 
        {
            if (reader != null) //old reader has to be closed before making new query
                reader.Close();

            Debug.Assert(!String.IsNullOrEmpty(_SqlQuery), "SqlQuery is null or empty.");

            if (wantToLogIt)
                LogQuery(_SqlQuery);

            refreshConnection();

            try
            {
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = _SqlQuery;
                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                if (wantToLogIt)
                    LogQuery(_SqlQuery, ex); //it ll be logged both in good and bad queries but its ok
                throw;
            }

            return reader;
        }

        public int SqlNonQuery(String _SqlQuery, bool wantToLogIt = true)//here without reader output //
        {
            if (reader != null) //old reader has to be closed before making new query
                reader.Close();

            Debug.Assert(!String.IsNullOrEmpty(_SqlQuery), "SqlQuery is null or empty.");

            if (wantToLogIt)
                LogQuery(_SqlQuery);

            refreshConnection();

            try
            {
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = _SqlQuery;

                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if(wantToLogIt)
                    LogQuery(_SqlQuery, ex); //it ll be logged both in good and bad queries but its ok
                throw;
            }
        }

        /// <summary>
        /// logs query into DB or into file if cant into DB when the query is good (no exception)
        /// </summary>
        /// <param name="query"></param>
        private void LogQuery(String query)
        {
            try
            {
                SqlNonQuery(String.Format(
                    "INSERT INTO log_queries_good (`query`, `date`) VALUES ('{0}', {1})",
                    query.Replace('\'','?'),
                    "NOW()"
                    ),
                false //dont log it (it'd be recursive logging)
                );
            }
            catch (Exception)
            {
                TextWriter log2 = new StreamWriter(System.Web.HttpContext.Current.Request.MapPath("~/Infrastructure/Log/SqlQueries.txt"), true);
                log2.WriteLine(query);
                log2.Close();
                throw;
            }
        }

        /// <summary>
        /// logs query into DB or into file if cant into DB when the query is bad (throwed excepton)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="error"></param>
        private void LogQuery(String query, Exception ex)
        {
            try
            {
                SqlNonQuery(String.Format(
                    "INSERT INTO log_queries_bad (`query`, `error`, `date`) VALUES ('{0}', '{1}', {2})",
                    query.Replace('\'', '?'),
                    ex.ToString().Replace('\'','?'),
                    "NOW()"
                    ),
                false //dont log it (it'd be recursive logging)
                );
            }
            catch (Exception)
            {
                TextWriter log2 = new StreamWriter(System.Web.HttpContext.Current.Request.MapPath("~/Infrastructure/Log/SqlBadQueries.txt"), true);
                log2.WriteLine(query);
                log2.WriteLine(ex.ToString());
                log2.Close();
                throw;
            }
        }

        /// <summary>
        /// in case of closed conn -> open it
        /// in case of busy conn -> wait
        /// in case of broken conn -> throw an exception
        /// </summary>
        /// <param name="tryNo">
        /// dont use that - PLEASE DONT USE THAT! ;)
        /// </param>
        private void refreshConnection(int tryNo = 0)
        {
            int tries = 0;

            if (conn == null) //in here its rather not needed, but in case its rly null it'd throw some exception
                conn.Open();

            switch (conn.State)
            {
                case ConnectionState.Closed:
                    conn.Open();
                    break;

                case ConnectionState.Broken:
                    throw new Exception(String.Format("DB conn is broken"));
                    break;

                //if db is in 1 of these states it will be open soon (i hope so ;))
                case ConnectionState.Connecting:
                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                    while (!conn.State.Equals(ConnectionState.Open) && tries++ < 1000) //max 1 tries
                        Thread.Sleep(1); //1 try every 1 sec -> so max 1s wait

                    if (!conn.State.Equals(ConnectionState.Open))
                        MessageBox.Show("After waiting 1s, database is still in state '{0}'", conn.State.ToString());

                    break;

                //everything is ok
                case ConnectionState.Open:
                    break;

                default:
                    throw new Exception("unknown state");
            }
        
        }
    }
}