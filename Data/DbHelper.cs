using Microsoft.Data.SqlClient;
using System.Data;

namespace Inventaire.Data
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public System.Data.DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);
            if (parameters != null)
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
            con.Open();
            using var adapter = new SqlDataAdapter(cmd);
            var table = new System.Data.DataTable();
            adapter.Fill(table);
            return table;
        }

        public int ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);
            if (parameters != null)
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
            con.Open();
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, con);
            if (parameters != null)
                foreach (var p in parameters)
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
            con.Open();
            return cmd.ExecuteScalar();
        }
    }
}