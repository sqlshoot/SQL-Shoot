namespace Engine.DatabaseInteraction
{
    internal interface IDatabaseVersionProvider
    {
        DatabaseVersion QueryForDatabaseVersion(ISqlExecutor sqlExecutor);
    }
}
