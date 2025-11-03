using System;
using Microsoft.Data.SqlClient;
using SmartGreenhouse.Models;

namespace SmartGreenhouse.Data
{
    public class DataService
    {
        private readonly string _connectionString;

        public DataService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void SaveLog(LogRecord record)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var cmd = new SqlCommand(
                "INSERT INTO Logs (Timestamp, Action, Device, Value) VALUES (@time, @action, @device, @value)", 
                connection);

            cmd.Parameters.AddWithValue("@time", record.Timestamp);
            cmd.Parameters.AddWithValue("@action", record.Action);
            cmd.Parameters.AddWithValue("@device", (object?)record.Device ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@value", (object?)record.Value ?? (object)DBNull.Value);

            cmd.ExecuteNonQuery();
        }
    }
}

