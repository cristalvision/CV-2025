using System.Data;
using System.Data.OleDb;
using System.Runtime.Versioning;

namespace CV_2025.CristalVision.Database
{
    [SupportedOSPlatform("windows")]
    public class Access : CVDatabase
    {
        OleDbConnection connection;

        public Access()
        {
            //The 'Microsoft.ACE.OLEDB.16.0' provider is not registered on the local machine.
            //C:\Users\user>Downloads\accessdatabaseengine_X64.exe /quiet

            connection = new()
            {
                ConnectionString = CVDatabase.connectionString
            };
            connection.Open();

            DataTable userTables = connection.GetSchema("Tables", [null, null, null, "Table"]);

            tableNames = [];
            for (int i = 0; i < userTables.Rows.Count; i++)
            {
                tableNames.Add((string)userTables.Rows[i][2]);
            }
        }

        public void Insert(List<string> colNames, List<dynamic> values)
        {
            //Check database type vs given type
            List<string> parameters = colNames.Select(parameter => "@" + parameter.Replace(" ", String.Empty)).ToList();
            string query1 = "INSERT INTO `" + tableName + "` (`" + String.Join("`, `", colNames) + "`) VALUES (" + String.Join(", ", parameters) + ")";

            OleDbCommand command = new(query1, connection);

            for (int index = 0; index < values.Count; index++)
                command.Parameters.AddWithValue(parameters[index], values[index]);

            command.ExecuteNonQuery();
        }

        public override List<dynamic>? Filter(string uniqueColumn, dynamic uniqueValue)
        {
            string query = "SELECT * FROM [" + tableName + "] WHERE [" + uniqueColumn + "] = " + uniqueValue;
            OleDbCommand command = new(query, connection);

            OleDbDataReader reader = command.ExecuteReader();
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
            OleDbCommand oleDbCommand = new(query, connection);
            oleDbCommand.ExecuteNonQuery();
        }

        public override void Close()
        {
            connection.Close();
        }
    }
}
