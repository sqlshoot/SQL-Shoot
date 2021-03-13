# Contributing

SQL Shoot is an open source tool, and welcomes contributions.

If you would like to contribute, read through this document, and create a Pull Request with your code.

You'll need to sign the SQL Shoot Contributor License Agreement for your code to be merged.

## Contributing database support

This is a brief guide on how to add support for a new database in SQL Shoot.

It's recommended to look at how an existing database has been implemented by way of example.

In the `SqlParser` project:
  - Create a subclass of `Parser` for your database
    - You won't necessarily need to override any of the methods to begin with

In the `DatabaseInteraction` project:
  - Create a new folder for your database
  - Implement the relevant classes
    - `IDatabaseInteractor`, `ISchemaNuker`, and so on
  - You'll need an ADO.Net NuGet package for your database also

In the `SqlShootEngine` project:
  - Update `ConnectionStringUtils` for your database
  - Update `ValidateConnectionString` to fail-fast on invalid strings
  - Update `AppendCredentialsToConnectionString` to append `Username` and `Password` configuration options to the connection string if relevant
    - Connection string parameters take precedence. Therefore, only append credentials from configuration if not already supplied in the connection string.
  - Update `DatabaseEngineUtils`
    - Add your database's name
    - Update `IsValidDatabaseEngine()`
    - Update `DoesDatabaseEngineHaveConceptOfSchemas` if necessary
  - Add new block in `SetConfiguration` to new up all your classes
  - Hook it up following the pattern of other databases

This is not necessarily an exhaustive list. However it tells you most of the places you'll need to look in order for SQL Shoot to support your database.

Once you've tested it manually and you're satisfied, create a PR with your work.