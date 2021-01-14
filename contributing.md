# Contributing

SQL Shoot is an open source tool, and welcomes contributions.

If you would like to contribute, read through this document, and create a Pull Request with your code.

You'll need to sign the SQL Shoot Contributor License Agreement for your code to be merged.

## Contributing database support

This is a brief guide on how to add support for a new database in SQL Shoot.

It's recommended to look at how an existing database has been implemented by way of example.

In the `Engine` project, under the `Databases` folder, create a new sub-folder for your database support.

Create classes which implement the following interfaces:
- `IChangeHistoryStore`
- `IDatabaseInteractor`
- `ISchemaNuker`
- `ISqlExecutor`
- `IDatabaseVersionProvider`

Tips for implementing `IChangeHistoryStore`:
- Inject the `ScriptTemplateProvider` to read in scripts (see existing code by way of example)
- Inject the `ITimestampProvider` to get timestamps

Create a subclass of `Parser` for your database. You won't necessarily need to override any of the methods to begin with.

Update `ConnectionStringUtils` for your database.

Update `ValidateConnectionString` to fail-fast on invalid strings.

Update `AppendCredentialsToConnectionString` to append `Username` and `Password` configuration options to the connection string if relevant.

Connection string parameters take precedence. Therefore, only append credentials from configuration if not already supplied in the connection string.

Update `DatabaseEngineUtils`
  - Add your database's name
 - Update `IsValidDatabaseEngine()`
 - Update `DoesDatabaseEngineHaveConceptOfSchemas` if necessary

Add new block in `SetConfiguration` to new up all your classes

Test by running with `DatabaseEngine=` using the new `ConnectionStringUtils` constant you added.