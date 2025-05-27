using MySql.Data.MySqlClient;
using MySql.Data.Types;
using NetTopologySuite.Geometries;

namespace CV_2025.CristalVision.Database
{
    public class MySQL : CVDatabase
    {
        public string? tableName;
        public List<string> tableNames;
        MySqlConnection connection;

        public MySQL(string DBName)
        {
            string connectionString = "server=localhost;uid=root;database=" + DBName;
            connection = new(connectionString);
            connection.Open();

            string query = "SELECT Table_name as TablesName from information_schema.tables where table_schema = '" + DBName + "'";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();

            tableNames = [];
            while (reader.Read()) tableNames.Add(reader.GetString(0));

            reader.Close();
        }

        public void Insert(List<string> colNames, List<dynamic> values)
        {
            //Check database type vs given type
            List<string> parameters = colNames.Select(parameter => "@" + parameter.Replace(" ", String.Empty)).ToList();
            string query = "INSERT INTO `" + tableName + "` (`" + String.Join("`, `", colNames) + "`) VALUES (" + String.Join(", ", parameters) + ")";

            MySqlCommand command = new MySqlCommand(query, connection);

            for (int index = 0; index < values.Count; index++)
            {
                if (values[index].GetType().Name == "Byte[]")
                    command.Parameters.Add(parameters[index], MySqlDbType.Blob).Value = values[index];
                else
                    command.Parameters.AddWithValue(parameters[index], values[index]);
            }

            command.ExecuteNonQuery();
        }

        public override List<dynamic>? Filter(string uniqueColumn, dynamic uniqueValue)
        {
            string query = "SELECT * FROM `" + tableName + "` WHERE `" + uniqueColumn + "` = '" + uniqueValue + "'";
            MySqlCommand command = new MySqlCommand(query, connection);

            MySqlDataReader reader = command.ExecuteReader();
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
            MySqlCommand command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Get array of bytes for MySql Geometry
        /// </summary>
        public static byte[] GetLineString(LineSegment lineSegment)
        {
            byte[] value1 = MySqlGeometry.Parse("POINT(" + (int)lineSegment.P0.X + ", " + (int)lineSegment.P0.Y + ")").Value;
            byte[] value2 = MySqlGeometry.Parse("POINT(" + (int)lineSegment.P1.X + ", " + (int)lineSegment.P1.Y + ")").Value;

            //UPDATE `line-1px` SET `Positions` = LineString(Point(10, 50), Point(23, 50));
            byte[] value = [0, 0, 0, 0, 1, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, value1[14], value1[15], value1[16], 0, 0, 0, 0, 0, value1[22], value1[23], value1[24], 0, 0, 0, 0, 0, value2[14], value2[15], value2[16], 0, 0, 0, 0, 0, value2[22], value2[23], value2[24]];

            return value;
        }

        /// <summary>
        /// Get line segment from MySql Geometry array og bytes
        /// </summary>
        public static LineSegment GetLineSegment(byte[] value)
        {
            MySqlGeometry sqlGeometry1 = new MySqlGeometry(MySqlDbType.Geometry, [0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, value[18], value[19], value[20], 0, 0, 0, 0, 0, value[26], value[27], value[28]]);
            MySqlGeometry sqlGeometry2 = new MySqlGeometry(MySqlDbType.Geometry, [0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, value[34], value[35], value[36], 0, 0, 0, 0, 0, value[42], value[43], value[44]]);

            return new LineSegment((double)sqlGeometry1.XCoordinate, (double)sqlGeometry1.YCoordinate, (double)sqlGeometry2.XCoordinate, (double)sqlGeometry2.YCoordinate);
        }

        public override void Close()
        {
            connection.Close();
        }
    }
}
