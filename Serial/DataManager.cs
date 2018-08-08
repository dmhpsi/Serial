using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    public sealed class DataManager
    {
        private DataManager()
        {
            if (!File.Exists("db.sqlite"))
            {
                SQLiteConnection.CreateFile("db.sqlite");
            }
        }

        public static DataManager Instance { get { return Nested.instance; } }

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly DataManager instance = new DataManager();
        }

        public SQLiteConnection DbConnection;
        public readonly string logFileName = "Log.txt";
        public readonly string errorLogFileName = "ErrorLog.txt";
        public static readonly string[] SqlColumnNames = { "count", "boardid", "devid", "timestamp", "temp", "humidity" };
        static readonly string[] SqlColumnTypes = { "int", "varchar(20)", "varchar(20)", "bigint", "float", "float" };
        public readonly string SqlDbName = "data";
        static readonly int SqlColumnCount = SqlColumnNames.Length;

        public void Connect()
        {
            Console.WriteLine("Connected");
            DbConnection = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
            DbConnection.Open();
            string[] varStr = new string[SqlColumnCount];
            for (int i = 0; i < SqlColumnCount; i++)
            {
                varStr[i] = SqlColumnNames[i] + ' ' + SqlColumnTypes[i];
            }
            string columns = string.Join(", ", varStr);
            string sql = "create table if not exists " + SqlDbName + "(" + columns + ")";
            SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();

        }
        public void Disconnect()
        {
            DbConnection.Close();
            Console.WriteLine("Disconnected");
        }

        public DataSet GetDataSource(string boardid, string devid, int page, int pageSize)
        {
            DataSet dataSet = new DataSet();
            string commandString
                = @"select count, boardid, devid, 
                        strftime('%d/%m/%Y %H:%M', datetime(timestamp, 'unixepoch'), '+7 hour') as time, 
                        temp, humidity from data";
            if (boardid != "[All]")
            {
                commandString += " where boardid='" + boardid + "'";
                if (devid != "[All]")
                {
                    commandString += " and devid='" + devid + "'";
                }
            }
            //commandString += " order by time asc";
            commandString += String.Format(" limit {0}, {1}", page * pageSize, pageSize);
            //Console.WriteLine(commandString);
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(commandString, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }
        public int GetRowsCount(string boardid, string devid)
        {
            string commandString = "select count(*) from data";
            if (boardid != "[All]")
            {
                commandString += " where boardid='" + boardid + "'";
                if (devid != "[All]")
                {
                    commandString += " and devid='" + devid + "'";
                }
            }
            SQLiteCommand command = new SQLiteCommand(commandString, DbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.Read())
                {
                    return Int32.Parse(reader["count(*)"].ToString());
                }
            }
            catch
            {
            }
            return 0;
        }
        public void Log(string text)
        {
            if (!File.Exists(logFileName))
            {
                using (StreamWriter sw = File.CreateText(logFileName)) { }
            }
            using (StreamWriter sw = File.AppendText(logFileName))
            {
                sw.WriteLine(DateTime.Now.ToString("[dd/MM/yy_HH:mm:ss] ") + text.Trim());
            }
        }
        public void ErrorLog(string text)
        {
            if (!File.Exists(errorLogFileName))
            {
                using (StreamWriter sw = File.CreateText(errorLogFileName)) { }
            }
            using (StreamWriter sw = File.AppendText(errorLogFileName))
            {
                sw.WriteLine(DateTime.Now.ToString("[dd/MM/yy_HH:mm:ss] ") + text.Trim());
            }
        }
        public string[] GetBoardsList()
        {
            DataSet dataSet = new DataSet();
            string commandString = "select distinct boardid from " + SqlDbName;
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(commandString, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            List<string> boards = new List<string>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                boards.Add(row[0].ToString());
            }
            return boards.ToArray();
        }
        public string[] GetDevidsList(string boardId)
        {
            DataSet dataSet = new DataSet();
            string commandString = "select distinct devid from " + SqlDbName + " where boardid='" + boardId + "'";
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(commandString, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            List<string> devs = new List<string>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                devs.Add(row[0].ToString());
            }
            return devs.ToArray();
        }

        public Record[] GetDataByLastTime(string boardid, string devid, long seconds)
        {
            DataSet dataSet = new DataSet();
            string sql = String.Format("select * from {0} where boardid='{1}' and devid='{2}' and datetime(timestamp, 'unixepoch') >= datetime('now', '-{3} second')",
                SqlDbName,
                boardid,
                devid,
                seconds);
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(sql, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            List<Record> records = new List<Record>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Record record = new Record();
                if (record.Input(row["boardid"], row["timestamp"], row["devid"], row["temp"], row["humidity"], row["count"]))
                    records.Add(record);
            };
            return records.ToArray();
        }
    }
}
