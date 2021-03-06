﻿#region original-work-license
/*
 * This file is part of SQL Shoot.
 *
 * SQL Shoot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * SQL Shoot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with SQL Shoot. If not, see <https://www.gnu.org/licenses/>.
 */
#endregion
using DatabaseInteraction;
using System.Collections.Generic;
using Constraint = SchemaSnapshot.DatabaseModel.Constraint;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerConstraintLoader : IConstraintLoader
    {
        public List<Constraint> LoadConstraintsAndIndexesInSchema(string schemaName, ISqlExecutor sqlExecutor)
        {
            var constraints = new List<Constraint>();

            var sql = $@"
					select 
						s.name as schemaName,
						t.name as tableName, 
						t.baseType,
						i.name as name, 
						c.name as columnName,
						i.is_primary_key, 
						i.is_unique_constraint,
						i.is_unique, 
						i.type_desc,
						i.filter_definition,
						isnull(ic.is_included_column, 0) as is_included_column,
						ic.is_descending_key,
						i.type
					from (
						select object_id, name, schema_id, 'T' as baseType
						from   sys.tables
						union
						select object_id, name, schema_id, 'V' as baseType
						from   sys.views
						union
						select type_table_object_id, name, schema_id, 'TVT' as baseType
						from   sys.table_types
						) t
						inner join sys.indexes i on i.object_id = t.object_id
						inner join sys.index_columns ic on ic.object_id = t.object_id
							and ic.index_id = i.index_id
						inner join sys.columns c on c.object_id = t.object_id
							and c.column_id = ic.column_id
						inner join sys.schemas s on s.schema_id = t.schema_id
					where i.type_desc != 'HEAP'
                    and s.name = '{schemaName}'
                    and baseType = 'T'
					order by s.name, t.name, i.name, ic.key_ordinal, ic.index_column_id";

            sqlExecutor.Execute(sql, reader =>
            {
                var tableName = reader.ReadString("tableName");
                var name = reader.ReadString("name");
                var columnName = reader.ReadString("columnName");
                var isPrimaryKey = reader.ReadBoolean("is_primary_key");
                var isUniqueConstraint = reader.ReadBoolean("is_unique_constraint");
                var isUnique = reader.ReadBoolean("is_unique");
                var typeDescription = reader.ReadString("type_desc");
                var isIncludedColumn = reader.ReadBoolean("is_included_column");

                var type = "INDEX";

                if (isPrimaryKey)
                {
                    type = "PRIMARY KEY";
                }

                if (isUniqueConstraint)
                {
                    type = "UNIQUE";
                }

                var constraint = new SqlServerConstraint(
                    tableName,
                    name,
                    columnName,
                    isPrimaryKey,
                    isUniqueConstraint,
                    isUnique,
                    typeDescription,
                    isIncludedColumn,
                    type);

                constraints.Add(constraint);
            });

            return constraints;
        }
    }
}