namespace SqlShootEngine.DatabaseInteraction
{
    internal interface IDatabaseVersionProvider
    {
        DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor);
    }
}
