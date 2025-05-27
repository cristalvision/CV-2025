namespace CV_2025.CristalVision.Database
{
    public abstract class CVDatabase
    {
        public abstract void ExecuteNonQuery(string query);
        public abstract List<dynamic>? Filter(string uniqueColumn, dynamic uniqueValue);

        public abstract void Close();
    }
}
