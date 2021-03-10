using System;
using Microsoft.Data.SqlClient;
using Npgsql;
using SchemaSnapshot.DatabaseModel;
using SchemaSnapshot.Diff;
using SchemaSnapshot.Postgres;
using SchemaSnapshot.SqlServer;

namespace SchemaSnapshot
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintSqlServerDatabase();
            CompareSqlServerDatabases();
            //PrintPostgresDatabase();
        }

        private static void CompareSqlServerDatabases()
        {
            var conn1 = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=MyDatabase;Integrated Security=true;";
            var conn2 = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=MyOtherDatabase;Integrated Security=true;";
            var schema1 = LoadSqlServerSchema(conn1, "dbo");
            var schema2 = LoadSqlServerSchema(conn2, "dbo");

            PrintSchema(schema1);
            PrintSchema(schema2);

            var schemaComparer = new SchemaComparer();
            var schemaDiff = schemaComparer.Compare(schema1, schema2);

            foreach (var tableInCommon in schemaDiff.TablesInCommon)
            {
                Console.WriteLine($"Tables in common: '{tableInCommon.Item1.Name}'");
            }

            foreach (var tableDiff in schemaDiff.TableDiffs)
            {
                Console.WriteLine("Columns in common:");
                foreach (var column in tableDiff.TableColumnDiff.ColumnsInCommon)
                {
                    Console.WriteLine($"\t{column.Item1.Name} | {column.Item1.DataType}");
                }

                Console.WriteLine("Columns only in source:");
                foreach (var column in tableDiff.TableColumnDiff.ColumnsOnlyInSource)
                {
                    Console.WriteLine($"\t{column.Name} | {column.DataType}");
                }

                Console.WriteLine("Columns only in target:");
                foreach (var column in tableDiff.TableColumnDiff.ColumnsOnlyInTarget)
                {
                    Console.WriteLine($"\t{column.Name} | {column.DataType}");
                }
            }
        }

        private static void PrintSqlServerDatabase()
        {
            var conn = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=Northwind;Integrated Security=true;";
            var schema = LoadSqlServerSchema(conn, "dbo");
            PrintSchema(schema);
        }

        private static void PrintPostgresDatabase()
        {
            var conn = "User ID=postgres;Password=postgres;Host=localhost;Port=62081;Database=myDb;"; 
            var schema = LoadPostgresSchema("mySchema", conn);
            PrintSchema(schema);
        }


        private static void PrintSchema(Schema schema)
        {
            foreach (var table in schema.Tables)
            {
                Console.WriteLine($"Table '{table.Name}' owned by {table.ParentSchema}'");

                foreach (var column in table.Columns)
                {
                    Console.WriteLine($"\tColumn {column.Name} | {column.DataType}");
                }

                foreach (var constraint in table.Constraints)
                {
                    Console.WriteLine($"\tConstraint {constraint.IndexName} | {constraint.Type}");
                }
            }
        }

        static Schema LoadSqlServerSchema(string connectionString, string schemaName)
        {
            Console.WriteLine("Loading SQL Server Schema");
            Schema schema = null;
            var schemaLoader = new SchemaLoader(
                new SqlServerTableLoader(),
                new SqlServerColumnLoader(),
                new SqlServerConstraintLoader(),
                schemaName);
            var connection = new SqlConnection(connectionString);
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                schema = schemaLoader.Load(command);
            }

            connection.Close();

            return schema;
        }

        static Schema LoadPostgresSchema(string schemaName, string connectionString)
        {
            Console.WriteLine("Loading Postgres schema");
            Schema schema = null;
            var schemaLoader = new SchemaLoader(
                new PostgresTableLoader(),
                new PostgresColumnLoader(),
                new PostgresConstraintLoader(),
                schemaName);
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                schema = schemaLoader.Load(command);
            }

            connection.Close();

            return schema;
        }
    }
}
