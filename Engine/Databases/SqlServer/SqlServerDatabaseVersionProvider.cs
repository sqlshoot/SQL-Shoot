using Engine.DatabaseInteraction;

namespace Engine.Databases.SqlServer
{
    internal class SqlServerDatabaseVersionProvider : IDatabaseVersionProvider
    {
        public DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor)
        {
            var result = sqlExecutor.ExecuteWithResult("SELECT @@VERSION");

            if (result is string versionInfo)
            {
                if (versionInfo.Contains("Microsoft SQL Server 2019"))
                {
                    return new DatabaseVersion("2019", 2019, 0, 0, true);
                }

                if (versionInfo.Contains("Microsoft SQL Server 2017"))
                {
                    return new DatabaseVersion("2017", 2017, 0, 0, true);
                }

                if (versionInfo.Contains("Microsoft SQL Server 2016"))
                {
                    return new DatabaseVersion("2016", 2016, 0, 0, true);
                }

                return new DatabaseVersion(versionInfo, 0, 0, 0, false);
            }

            return new DatabaseVersion(string.Empty, 0, 0, 0, false);
        }
    }
}
