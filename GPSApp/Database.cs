using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using Mono.Data.Sqlite;

namespace GPSApp
{

    public class Database
    {
        const string SQLITE_DATABASE_NAME = "database.sqlite3";
        SqliteConnection conn;

        public Database()
        {
            var rootPath = Android.OS.Environment.ExternalStorageDirectory.Path;
            conn = new SqliteConnection($"Data Source={Path.Combine(rootPath, SQLITE_DATABASE_NAME)}");
            if (!File.Exists(Path.Combine(rootPath, SQLITE_DATABASE_NAME)))
            {
                SqliteConnection.CreateFile(Path.Combine(rootPath, SQLITE_DATABASE_NAME));
                CreateCoordenatesTable();
            }
        }

        public void CreateCoordenatesTable()
        {
            string sql = "CREATE TABLE COORDS(Latitude DECIMAL, Longitude DECIMAL, Date TEXT)";
            try
            {
                conn.Open();
                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }

        public void InsertCoordenate(double latitude, double longitude, string date)
        {
            string sql = "INSERT INTO COORDS(Latitude, Longitude, Date)" +
                $"VALUES({latitude},{longitude},'{date}')";
            try
            {
                conn.Open();
                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }

        public IEnumerable<dynamic> SelectCoordenate()
        {
            List<dynamic> resultados = new List<dynamic>();
            string sql = "SELECT Latitude,Longitude,Date FROM COORDS";
            try
            {
                conn.Open();
                using (var cmd = new SqliteCommand(sql, conn))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        resultados.Add(new
                        {
                            Latitude = Convert.ToDouble(reader["Latitude"]),
                            Longitude = Convert.ToDouble(reader["Longitude"]),
                            Date = reader["Date"].ToString()
                        });
                    }
                    reader.Close();
                    return resultados;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}