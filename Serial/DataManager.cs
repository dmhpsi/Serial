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

        private static bool connected = false;
        public SQLiteConnection DbConnection;
        public static readonly string [] SqlColumnNames = {"count", "boardid", "devid", "time", "temp", "humidity" };
        static readonly string [] SqlColumnTypes = {"int", "varchar(20)", "varchar(20)", "bigint", "float", "float" };
        public readonly string SqlDbName = "data";
        static readonly int SqlColumnCount = SqlColumnNames.Length;

        public void Connect()
        {
            while (connected)
            {
                // wait for others transaction complete
            }
            Console.WriteLine("Connected");
            DbConnection = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
            connected = true;
            DbConnection.Open();
            string [] varStr = new string[SqlColumnCount];
            for (int i = 0; i < SqlColumnCount; i++)
            {
                varStr[i] = SqlColumnNames[i] + ' ' + SqlColumnTypes[i];
            }
            string columns = string.Join(", ", varStr);
            string sql = "create table if not exists " + SqlDbName + "("+ columns + ")";
            SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();

        }
        public void Disconnect()
        {
            DbConnection.Close();
            connected = false;
            Console.WriteLine("Disconnected");
        }
        
        public DataSet GetDataSource()
        {
            DataSet dataSet = new DataSet();
            string commandString = "select * from data";
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(commandString, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }
    }
}
