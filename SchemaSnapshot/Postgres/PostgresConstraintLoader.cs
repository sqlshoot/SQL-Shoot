using DatabaseInteraction;
using System.Collections.Generic;
using System.Linq;
using Constraint = SchemaSnapshot.DatabaseModel.Constraint;

namespace SchemaSnapshot.Postgres
{
    internal class PostgresConstraintLoader : IConstraintLoader
    {
        public List<Constraint> LoadConstraintsAndIndexesInSchema(string schemaName, ISqlExecutor sqlExecutor)
        {
            var constraintsFirstPass = new List<PostgresConstraint>();

            var constraintsToColumnsSql = $@"
					select pgc.conname as constraint_name,
                       ccu.table_schema as table_schema,
                       ccu.table_name,
                       ccu.column_name,
                       pg_get_constraintdef(pgc.oid)
                from pg_constraint pgc
                join pg_namespace nsp on nsp.oid = pgc.connamespace
                join pg_class  cls on pgc.conrelid = cls.oid
                left join information_schema.constraint_column_usage ccu
                          on pgc.conname = ccu.constraint_name
                          and nsp.nspname = ccu.constraint_schema
                and table_schema = '{schemaName}'";

            sqlExecutor.Execute(constraintsToColumnsSql, reader =>
            {
                var tableName = reader.ReadString("table_name");
                var name = reader.ReadString("constraint_name");
                var columnName = reader.ReadString("column_name");
                var definition = reader.ReadString("pg_get_constraintdef");

                var constraint = new PostgresConstraint(
                    tableName,
                    name,
                    columnName,
                    string.Empty,
                    definition);

                constraintsFirstPass.Add(constraint);
            });

            var constraints = new List<Constraint>();

            var constraintsSql = $@"
                SELECT
                    table_name,
                    constraint_name,
                    constraint_type
                FROM
                  INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                WHERE
                    table_schema = '{schemaName}'";

            sqlExecutor.Execute(constraintsSql, reader =>
            {
                var tableName = reader.ReadString("table_name");
                var name = reader.ReadString("constraint_name");
                var type = reader.ReadString("constraint_type");

                var constraint = constraintsFirstPass.FirstOrDefault(c => c.TableName == tableName && c.Name == name);

                // No matching constraint? Who knows...
                if (constraint != null)
                {
                    constraints.Add(
                        new PostgresConstraint(
                        constraint.TableName,
                        constraint.Name,
                        constraint.ColumnName,
                        type,
                        constraint.Definition));
                }
            });

            var indexSql = $@"SELECT
                                *
                            FROM
                                pg_indexes
                            WHERE
                                schemaname = '{schemaName}'";

            sqlExecutor.Execute(indexSql, reader =>
            {
                var tableName = reader.ReadString("tablename");
                var name = reader.ReadString("indexname");
                var type = "INDEX";
                var definition = reader.ReadString("indexdef");

                var constraint = constraints.FirstOrDefault(c => c.TableName == tableName && c.Name == name);

                if (constraint != null)
                {
                    constraints.Add(
                        new PostgresConstraint(
                        tableName,
                        name,
                        constraint.ColumnName,
                        type,
                        definition));
                }
            });

            return constraints;
        }
    }
}