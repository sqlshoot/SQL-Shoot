using SqlShootEngine.DatabaseInteraction;

namespace SqlShootEngine.Databases.SQLite
{
    internal class SQLiteDatabaseVersionProvider : IDatabaseVersionProvider
    {
        public DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor)
        {
            var result = sqlExecutor.ExecuteWithResult("select sqlite_version();");

            if (result is string versionInfo)
            {
                var versionParts = versionInfo.Split(".");

                if (versionParts.Length >= 2)
                {
                    var major = int.Parse(versionParts[0]);
                    var minor = int.Parse(versionParts[1]);

                    if (major == 3 && minor > 9)
                    {
                        return new DatabaseVersion(versionInfo, major, minor, 0, true);
                    }
                }

                return new DatabaseVersion(versionInfo, 0, 0, 0, false);
            }

            return new DatabaseVersion(string.Empty, 0, 0, 0, false);
        }
    }
}
