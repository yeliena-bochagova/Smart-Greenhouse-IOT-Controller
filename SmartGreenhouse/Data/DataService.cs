using System;
using Microsoft.Data.SqlClient;
using SmartGreenhouse.Models;
using System.Threading.Tasks;

namespace SmartGreenhouse.Data
{
    public class DataService
    {
        private readonly string _connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=SmartGreenhouseDB;Trusted_Connection=True;";

        public void SaveLog(LogRecord record)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var cmd = new SqlCommand(
                    "INSERT INTO Logs (Timestamp, Action, Device, Value) VALUES (@time, @action, @device, @value)",
                    connection);

                cmd.Parameters.AddWithValue("@time", record.Timestamp);
                cmd.Parameters.AddWithValue("@action", record.Action);
                cmd.Parameters.AddWithValue("@device", (object?)record.Device ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@value", (object?)record.Value ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB ERROR: {ex.Message}");
            }
        }

        public async Task<bool> InsertLogAsync(string message)
{
    try
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(
            "INSERT INTO Logs (Timestamp, Action, Device, Value) VALUES (@time, @action, NULL, NULL)",
            connection);

        cmd.Parameters.AddWithValue("@time", DateTime.Now);
        cmd.Parameters.AddWithValue("@action", message);

        await cmd.ExecuteNonQueryAsync();
        return true; // ✅ успішно
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB ERROR (async): {ex.Message}");
        return false; //  з помилкою
    }
}



        
    }
}
