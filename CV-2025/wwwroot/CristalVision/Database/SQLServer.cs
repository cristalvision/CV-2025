using CV_2025.CristalVision.Database;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using System.Runtime.Versioning;

namespace CV_2025.wwwroot.CristalVision.Database
{
    [SupportedOSPlatform("windows")]
    public class SQLServer : CVDatabase
    {
        private readonly SqlConnection connection;

        public SQLServer()
        {
            connection = new(CVDatabase.connectionString);
            connection.Open();

            DataTable userTables = connection.GetSchema("Tables");

            tableNames = [];
            for (int i = 0; i < userTables.Rows.Count; i++)
            {
                tableNames.Add((string)userTables.Rows[i][2]);
            }
        }

        public void Insert(List<string> colNames, List<dynamic> values)
        {
            //Check database type vs given type
            /*List<string> parameters = colNames.Select(parameter => "@" + parameter.Replace(" ", String.Empty)).ToList();
            string query1 = "INSERT INTO `" + tableName + "` (`" + String.Join("`, `", colNames) + "`) VALUES (" + String.Join(", ", parameters) + ")";

            OleDbCommand command = new(query1, connection);

            for (int index = 0; index < values.Count; index++)
                command.Parameters.AddWithValue(parameters[index], values[index]);

            command.ExecuteNonQuery();*/
        }

        public override List<dynamic>? Filter(string uniqueColumn, dynamic uniqueValue)
        {
            string query = "SELECT TOP (1000) [Test1],[Test2],[Test3] FROM [OCRDrawings].[dbo].[Table_1]";
            //string query = "SELECT * FROM [" + tableName + "] WHERE [" + uniqueColumn + "] = " + uniqueValue;
            SqlCommand command = new(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            List<dynamic>? rows = [];
            while (reader.Read())
            {
                for (int colNr = 0; colNr < reader.FieldCount; colNr++)
                    rows.Add(reader.GetValue(colNr));
            }

            reader.Close();

            return rows.Count != 0 ? rows : null;
        }

        public override void ExecuteNonQuery(string query)
        {
            //OleDbCommand oleDbCommand = new(query, connection);
            //oleDbCommand.ExecuteNonQuery();
        }

        public override void Close()
        {
            connection.Close();
        }
    }
}
