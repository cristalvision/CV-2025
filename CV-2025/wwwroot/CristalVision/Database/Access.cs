using MySql.Data.MySqlClient;
using System.Data;
using System.Data.OleDb;

namespace CV_2025.CristalVision.Database
{
    public class Access : CVDatabase
    {
        public string tableName;
        public List<string> tableNames;
        MySqlConnection connection;

        public Access(string DBName)
        {
            //The 'Microsoft.ACE.OLEDB.16.0' provider is not registered on the local machine.
            //C:\Users\user>Downloads\accessdatabaseengine_X64.exe /quiet

            string databasePath = Directory.GetCurrentDirectory() + "\\wwwroot\\CristalVision\\Database\\cvcharacters.accdb";
            OleDbConnection connection = new OleDbConnection();
            connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.16.0;Data Source=" + databasePath;
            connection.Open();

            DataTable userTables = connection.GetSchema("Tables", [null, null, null, "Table"]);

            List<string> tableNames = [];
            for (int i = 0; i < userTables.Rows.Count; i++)
                tableNames.Add(userTables.Rows[i][2].ToString());

            connection.Close();
        }

        public override void ExecuteNonQuery(string query)
        {
            throw new NotImplementedException();
        }

        public override List<dynamic> Filter(string uniqueColumn, dynamic uniqueValue, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
