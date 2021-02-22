using SqlShootEngine.DatabaseInteraction;

namespace SqlShootEngine.Databases.PostgreSQL
{
    internal class PostgreSQLDatabaseVersionProvider : IDatabaseVersionProvider
    {
        public DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor)
        {

            var result = sqlExecutor.ExecuteWithResult("SELECT version();");

            if (result is string versionInfo)
            {
                if (versionInfo.Contains("PostgreSQL 13", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 13, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 12", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 12, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 11", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 11, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 10", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 10, 0, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 9.6", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 9, 6, 0, true);
                }

                if (versionInfo.Contains("PostgreSQL 9.5", System.StringComparison.OrdinalIgnoreCase))
                {
                    return new DatabaseVersion(versionInfo, 9, 5, 0, true);
                }

                return new DatabaseVersion(versionInfo, 0, 0, 0, false);
            }

            return new DatabaseVersion(string.Empty, 0, 0, 0, false);
        }
    }
}
