using MySql.Data.MySqlClient;
using MySql.Data.Types;
using NetTopologySuite.Geometries;

namespace CV_2025.CristalVision.Database
{
    public abstract class CVDatabase
    {
        public abstract void ExecuteNonQuery(string query);
        public abstract List<dynamic> Filter(string uniqueColumn, dynamic uniqueValue, byte[] data);
    }
}
