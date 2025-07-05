using CV_2025.CristalVision.Vision;
using System.Configuration;
using System.Text.Json;

namespace CV_2025.CristalVision.Database
{
    public abstract class CVDatabase
    {
        public string? tableName;
        public List<string> tableNames;
        public enum Type { Acess, MySQL, SQLServer }
        public static Type type;
        public static string connectionString = String.Empty;
        
        public abstract void ExecuteNonQuery(string query);
        public abstract List<dynamic>? Filter(string uniqueColumn, dynamic uniqueValue);

        public abstract void Close();
    }
}
