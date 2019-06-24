using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serial
{
    // record data wrapper for ease of use
    public class Record
    {
        public const int numParam = 4;
        public string devid, boardid, reference;
        public float temp, humidity;
        private long time;

        private void GenerateReference()
        {
            reference = String.Format("{0:20},{1:20},{2:20}", boardid, devid, time);
        }
        public void SetTime(long newTime)
        {
            this.time = newTime;
            GenerateReference();
        }
        public long GetTime()
        {
            return this.time;
        }

        private long GetTimestamp()
        {
            long res = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            //res -= res % Constants.dataInterval;
            return res;
        }

        public bool Input(object boardid, object time, object devid, object temp, object humidity)
        {
            try
            {
                this.devid = devid.ToString();
                this.boardid = boardid.ToString();
                this.temp = float.Parse(temp.ToString());
                this.humidity = float.Parse(humidity.ToString());
                this.time = long.Parse(time.ToString());
                GenerateReference();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public void Input(string boardid, string devid, float temp, float humidity)
        {
            this.devid = devid;
            this.boardid = boardid;
            this.temp = temp;
            this.humidity = humidity;
            this.time = GetTimestamp();
            GenerateReference();
        }
        public bool Input(string raw)
        {
            raw = raw.Trim();
            raw = raw.TrimEnd(';');
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
            this.time = GetTimestamp();
            GenerateReference();
            return true;
        }
        //public void UnsafeSave()
        //{
        //    if (devid == "JUNK" || boardid == "JUNK")
        //    {
        //        return;
        //    }
        //    string sql;
        //    float mTemp = this.temp, mHumidity = this.humidity;
        //    int mCount = this.count;
        //    string whereClause = " where boardid='" + this.boardid + "'"
        //                + " and devid='" + this.devid + "'"
        //                + " and timestamp=" + this.time;

        //    sql = "select * from " + DataManager.Instance.SqlDbName + whereClause;
        //    SQLiteCommand command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
        //    SQLiteDataReader reader = command.ExecuteReader();
        //    if (reader.Read())
        //    {
        //        mCount = (int)reader["count"];
        //        mTemp = (this.temp * this.count + float.Parse(reader["temp"].ToString()) * mCount)
        //            / (this.count + mCount);
        //        mHumidity = (this.humidity * this.count + float.Parse(reader["humidity"].ToString()) * mCount)
        //            / (this.count + mCount);
        //        mCount += this.count;

        //        sql = "delete from " + DataManager.Instance.SqlDbName + whereClause;
        //        command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
        //        command.ExecuteNonQuery();
        //    }

        //    int[] nums = Enumerable.Range(0, numParam + 2).ToArray<int>();
        //    string formatString = "insert into " + DataManager.Instance.SqlDbName + " ("
        //        + string.Join(",", DataManager.SqlColumnNames)
        //        + ") values ('{" + string.Join("}','{", nums) + "}')";
        //    sql = String.Format(
        //        formatString,
        //        mCount,
        //        this.boardid,
        //        this.devid,
        //        this.time,
        //        mTemp,
        //        mHumidity);
        //    command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
        //    command.ExecuteNonQuery();
        //}

        public void UnsafeSave()
        {
            int[] nums = Enumerable.Range(0, numParam + 1).ToArray<int>();
            string formatString = "insert into " + DataManager.Instance.SqlDbNames[0] + " ("
                                 + string.Join(",", DataManager.SqlColumnNames)
                                 + ") values ('{" + string.Join("}','{", nums) + "}')";
            string sql = String.Format(
                formatString,
                this.boardid,
                this.devid,
                this.time,
                this.temp,
                this.humidity);

            SQLiteCommand command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
            command = new SQLiteCommand(sql, DataManager.Instance.DbConnection);
            command.ExecuteNonQuery();
        }

        public bool Save()
        {
            try
            {
                UnsafeSave();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public int CompareTo(Record y)
        {
            if (y == null)
                return 1;
            else
            {
                return reference.CompareTo(y.reference);
            }
        }

        public override string ToString()
        {
            DateTime datetime = new DateTime(time * TimeSpan.TicksPerSecond);
            return string.Format("[{0}] boardid:{1}, devid:{2}, temperature:{3}, humidity:{4}", datetime.ToString("dd/MM/yyyy HH:mm:ss"), boardid, devid, temp, humidity);
        }
        public string ToJSON()
        {
            DateTime datetime = new DateTime(time * TimeSpan.TicksPerSecond);
            return string.Format(
                "{{\"time\":\"{0}\",\"boardid\":\"{1}\",\"devid\":\"{2}\",\"temperature\":{3},\"humidity\":{4},\"timestamp\":{5}}}", 
                datetime.ToString("dd/MM/yyyy HH:mm:ss"), 
                boardid, 
                devid, 
                temp, 
                humidity,
                time);
        }
    }
}
