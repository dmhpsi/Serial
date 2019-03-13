using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    public sealed class DataManager
    {
        private DataManager()
        {
            dataDirectory = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDoc‌​uments), "CSLab", "EnvMon");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
            logFileName = Path.Combine(dataDirectory, "Log.txt");
            errorLogFileName = Path.Combine(dataDirectory, "ErrorLog.txt");
            dbPath = Path.Combine(dataDirectory, "db.sqlite");
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
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
        public PrivateFontCollection fontCollection;
        public readonly string logFileName;
        public readonly string errorLogFileName;
        public static readonly string[] SqlColumnNames = { "boardid", "devid", "timestamp", "temp", "humidity" };
        static readonly string[] SqlColumnTypes = { "varchar(20)", "varchar(20)", "bigint", "float", "float" };
        public readonly string[] SqlDbNames = { "raw", "avg1min", "avg1hour" };
        static readonly int SqlColumnCount = SqlColumnNames.Length;
        private readonly string dataDirectory;
        private readonly string dbPath;

        public enum DbSelect
        {
            raw = 0, avg1min = 1, avg1hour = 2
        }

        public void Connect()
        {
            Console.WriteLine("Connected");
            DbConnection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
            DbConnection.Open();
            string[] varStr = new string[SqlColumnCount];
            for (int i = 0; i < SqlColumnCount; i++)
            {
                varStr[i] = SqlColumnNames[i] + ' ' + SqlColumnTypes[i];
            }
            string columns = string.Join(", ", varStr);
            foreach (string sqlName in SqlDbNames)
            {
                string sql = "create table if not exists " + sqlName + "(" + columns + ")";
                SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
                command.ExecuteNonQuery();
            }
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
                = @"select * from " + SqlDbNames[0];
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
            string commandString = "select count(*) from " + SqlDbNames[0];
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
            string commandString = "select distinct boardid from " + SqlDbNames[1];
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
            string commandString = "select distinct devid from " + SqlDbNames[1] + " where boardid='" + boardId + "'";
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(commandString, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            List<string> devs = new List<string>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                devs.Add(row[0].ToString());
            }
            return devs.ToArray();
        }

        public List<Record> RecordsReduction(List<Record> src)
        {
            int severity = src.Count / 20;
            if (severity < 1)
                return new List<Record>(src.ToArray());
            List<Record> res = new List<Record>();
            for (int i = 1; i < src.Count; i += severity)
            {
                //---------------------------------------------------------------avg
                var start = i; // (i - severity > 0 ? i - severity : 0);
                var end = (i + severity < src.Count ? i + severity : src.Count);

                float sumTemp = 0, sumHumidity = 0;

                for (int j = start; j < end; j++)
                {
                    sumTemp += src[j].temp;
                    sumHumidity += src[j].humidity;
                }

                var avgTemp = sumTemp / (end - start);
                var avgHumidity = sumHumidity / (end - start);
                //---------------------------------------------------------------
                Record record = new Record();
                record.Input(src[i].boardid, src[i].GetTime(), src[i].devid, avgTemp, avgHumidity);
                res.Add(record);
            }
            return res;
        }

        public void Migrate(DbSelect from, DbSelect to, long untilTimestamp)
        {
            int range = 60;
            long outTime = 14400;    // 4 hours
            if (to == DbSelect.avg1hour)
            {
                outTime = 2592000;   // 30 days
                range = 3600;
            }
            else if (to == DbSelect.raw)
                return;
            string sql = string.Format(
                @"insert into {0} (boardid, devid, timestamp, temp, humidity)
                    select min(boardid), min(devid), ceiling(min(timestamp) / {1}) * {1}, avg(temp), avg(humidity)
                    from {2}
                    where timestamp <= {3} and 
                        timestamp > (select
                                    case when max(timestamp) is null then 0  else  max(timestamp) end
                                    from {0})
                    group by boardid, devid, timestamp / {1}", SqlDbNames[(int)to], range, SqlDbNames[(int)from], untilTimestamp);

            SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();
            sql = string.Format("delete from {0} where timestamp <= {1}",
                SqlDbNames[(int)from],
                untilTimestamp - outTime);
            command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();
        }

        public long GetLastMigrateTimestamp(DbSelect db)
        {
            string sql = "select max(timestamp) from " + SqlDbNames[(int)db];
            SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                try
                {
                    return long.Parse(reader["max(timestamp)"].ToString());
                }
                catch
                {

                }
            }
            return 0;
        }

        public Record[] GetLastData()
        {
            string sql = string.Format(@"select boardid, devid, temp, humidity, max(timestamp) as maxts
                    from raw 
                    where devid in (1,2,3) 
                    group by devid");
            DataSet dataSet = new DataSet();
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(sql, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            List<Record> records = new List<Record>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Record record = new Record();
                if (record.Input(row["boardid"], row["maxts"], row["devid"], row["temp"], row["humidity"]))
                    records.Add(record);
            };
            return records.ToArray();
        }

        public Record[] GetDataByLastTime(string boardid, string devid, long seconds)
        {
            long curentSeconds = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            string sql = string.Format(@"select min(mts1) as mts
                    from (
                        select min(timestamp) as mts1
                        from {0}  
                        where boardid='{3}' and devid='{4}' and timestamp >= {5}
                        union
                        select min(timestamp) as mts1
                        from {1}  
                        where boardid='{3}' and devid='{4}' and timestamp >= {5}
                        union
                        select min(timestamp) as mts1
                        from {2}  
                        where boardid='{3}' and devid='{4}' and timestamp >= {5})",
                    SqlDbNames[0],
                    SqlDbNames[1],
                    SqlDbNames[2],
                    boardid,
                    devid,
                    curentSeconds - seconds);
            SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                try
                {
                    Console.Write("mts");
                    Console.WriteLine(reader["mts"]);
                    seconds = curentSeconds - long.Parse(reader["mts"].ToString());
                }
                catch
                {
                    Console.WriteLine("Wrong mts");
                }
            }

            int timeAverageRange;
            string SqlDbName;
            if (seconds <= 120)   // 2 minutes
            {
                SqlDbName = SqlDbNames[0];
                timeAverageRange = 5;
            }
            else if (seconds <= 1200)   // 20 minutes
            {
                SqlDbName = SqlDbNames[1];
                timeAverageRange = 60;
            }
            else if (seconds <= 3600)   // 1 hours
            {
                SqlDbName = SqlDbNames[1];
                timeAverageRange = 300;
            }
            else if (seconds <= 21600)   // 6 hours
            {
                SqlDbName = SqlDbNames[1];
                timeAverageRange = 600;
            }
            else if (seconds <= 43200)   // 12 hours
            {
                SqlDbName = SqlDbNames[1];
                timeAverageRange = 1800;
            }
            else if (seconds <= 86400)  // 24 hours
            {
                SqlDbName = SqlDbNames[1];
                timeAverageRange = 3600;
            }
            else if (seconds <= 604800)  // 7 days
            {
                SqlDbName = SqlDbNames[2];
                timeAverageRange = 10800;  // 3 hours
            }
            else
            {
                SqlDbName = SqlDbNames[2];
                timeAverageRange = 86400;
            }
            long secondsCount = curentSeconds - seconds;
            DataSet dataSet = new DataSet();
            sql = String.Format(
                @"select 
                    min(boardid) as boardid, min(devid) as devid,
                    avg(temp) as temp, avg(humidity) as humidity,
                    ceiling(min(timestamp) / {4}) * {4} as timestamp1
                    from {0}  
                    where boardid='{1}' and devid='{2}' and timestamp >= {3}
                    group by timestamp / {4}
                    order by timestamp1 asc",
                SqlDbName,
                boardid,
                devid,
                secondsCount,
                timeAverageRange);
            SQLiteDataAdapter sqlDataAdapter = new SQLiteDataAdapter(sql, DbConnection);
            sqlDataAdapter.Fill(dataSet);
            List<Record> records = new List<Record>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                Record record = new Record();
                if (record.Input(row["boardid"], row["timestamp1"], row["devid"], row["temp"], row["humidity"]))
                    records.Add(record);
            };
            //return RecordsReduction(records).ToArray();
            return records.ToArray();
        }


        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
        public void InitCustomFont(byte[] resourceFont)
        {
            fontCollection = new PrivateFontCollection();
            Stream fontStream = new MemoryStream(resourceFont);
            int fontLength = resourceFont.Length;
            System.IntPtr data = Marshal.AllocCoTaskMem(fontLength);
            //create a buffer to read in to
            Byte[] fontData = new Byte[fontStream.Length];
            //fetch the font program from the resource
            fontStream.Read(fontData, 0, fontLength);
            //copy the bytes to the unsafe memory block
            Marshal.Copy(fontData, 0, data, fontLength);

            // We HAVE to do this to register the font to the system (Weird .NET bug !)
            uint cFonts = 0;
            AddFontMemResourceEx(data, (uint)fontData.Length, IntPtr.Zero, ref cFonts);

            //pass the font to the font collection
            fontCollection.AddMemoryFont(data, fontLength);
            //close the resource stream
            fontStream.Close();
            //free the unsafe memory
            Marshal.FreeCoTaskMem(data);
        }
    }
}
