using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{    
    public class Record
    {
        public const int numParam = 4;
        public const int interval = 300;
        public string devid, boardid;
        public float temp, humidity;
        public long time;

        public bool Intput(string raw)
        {
            string[] parts = raw.Trim().Split(',');
            if (parts.Length != numParam)
            {
                devid = "JUNK";
                boardid = "JUNK";
                return false;
            }
            else
            {
                try
                {
                    boardid = parts[0].Split(':')[1].Trim();
                    devid = parts[1].Split(':')[1].Trim();
                    if (!(float.TryParse(parts[2].Split(':')[1].Trim(), out temp) &&
                        float.TryParse(parts[3].Split(':')[1].Trim(), out humidity)))
                    {
                        return false;
                    }
                }
                catch
                {
                    devid = "JUNK";
                    boardid = "JUNK";
                    return false;
                }
            }
            this.time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000;
            return true;
        }
        public void UnsafeSave()
        {
            if (devid == "JUNK" || boardid == "JUNK")
            {
                return;
            }
            string sql;
            float mTemp = this.temp, mHumidity = this.humidity;
            int mCount = 1;
            long mTime = this.time - this.time % interval;
            string whereClause = " where boardid='" + this.boardid + "'"
                        + " and devid='" + this.devid + "'"
                        + " and time=" + mTime;

            sql = "select * from " + DataManager.Instance.SqlDbName + whereClause;
            SQLiteCommand command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                mCount = (int)reader["count"];
                mTemp = (this.temp + float.Parse(reader["temp"].ToString()) * mCount) / (mCount + 1);
                mHumidity = (this.humidity + float.Parse((string)reader["humidity"].ToString()) * mCount) / (mCount + 1);
                mCount++;

                sql = "delete from " + DataManager.Instance.SqlDbName + whereClause;
                command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
                command.ExecuteNonQuery();
            }

            int[] nums = Enumerable.Range(0, numParam + 2).ToArray<int>();
            string formatString = "insert into " + DataManager.Instance.SqlDbName + " ("
                + string.Join(",", DataManager.SqlColumnNames)
                + ") values ('{" + string.Join("}','{", nums) + "}')";
            sql = String.Format(
                formatString,
                mCount,
                this.boardid,
                this.devid,
                mTime,
                mTemp,
                mHumidity);
            command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
            command.ExecuteNonQuery();
        }

        public bool Save()
        {
            try {
                UnsafeSave();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
