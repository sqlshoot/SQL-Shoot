#region original-work-license
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
using System;
using System.Collections.Generic;
using System.Linq;
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Diff
{
    internal class SchemaComparer
    {
        public SchemaDiff Compare(Schema source, Schema target)
        {
            var tablesOnlyInSource = new List<Table>();
            var tablesOnlyInTarget = new List<Table>();
            var tablesInCommon = new List<Tuple<Table, Table>>();

            foreach (var table in source.Tables)
            {
                var correspondingTable = target.Tables.FirstOrDefault(t => t.Name == table.Name);

                if (correspondingTable == null)
                {
                    tablesOnlyInSource.Add(table);
                }
            }

            foreach (var table in target.Tables)
            {
                var correspondingTable = target.Tables.FirstOrDefault(t => t.Name == table.Name);

                if (correspondingTable == null)
                {
                    tablesOnlyInTarget.Add(table);
                }
            }

            foreach (var sourceTable in source.Tables)
            {
                foreach (var targetTable in target.Tables)
                {
                    if (sourceTable.Name == targetTable.Name)
                    {
                        tablesInCommon.Add(Tuple.Create(sourceTable, targetTable));
                    }
                }
            }

            var tableDiffs = new List<TableDiff>();

            foreach (var tablePair in tablesInCommon)
            {
                var sourceTable = tablePair.Item1;
                var targetTable = tablePair.Item2;
                var tableColumnDiff = CompareColumnsInTables(sourceTable, targetTable);
                var tableConstraintDiff = CompareConstraintsInTables(sourceTable, targetTable);

                tableDiffs.Add(new TableDiff(tableColumnDiff, tableConstraintDiff));
            }

            return new SchemaDiff(tablesOnlyInSource, tablesOnlyInTarget, tablesInCommon, tableDiffs);
        }

        private static TableColumnDiff CompareColumnsInTables(Table sourceTable, Table targetTable)
        {
            var columnsOnlyInSource = new List<Column>();
            var columnsOnlyInTarget = new List<Column>();
            var columnsInCommon = new List<Tuple<Column, Column>>();

            foreach (var column in sourceTable.Columns)
            {
                var correspondingColumn = targetTable.Columns.FirstOrDefault(t => t.Name == column.Name);

                if (correspondingColumn == null)
                {
                    columnsOnlyInSource.Add(column);
                }
            }

            foreach (var column in targetTable.Columns)
            {
                var correspondingColumn = targetTable.Columns.FirstOrDefault(t => t.Name == column.Name);

                if (correspondingColumn == null)
                {
                    columnsOnlyInTarget.Add(column);
                }
            }

            foreach (var sourceTableColumn in sourceTable.Columns)
            {
                foreach (var targetTableColumn in targetTable.Columns)
                {
                    if (sourceTableColumn.Name == targetTableColumn.Name)
                    {
                        columnsInCommon.Add(Tuple.Create(sourceTableColumn, targetTableColumn));
                    }
                }
            }

            return new TableColumnDiff(columnsOnlyInSource, columnsOnlyInTarget, columnsInCommon);
        }

        private static TableConstraintDiff CompareConstraintsInTables(Table sourceTable, Table targetTable)
        {
            var constraintsOnlyInSource = new List<Constraint>();
            var constraintsOnlyInTarget = new List<Constraint>();
            var constraintsInCommon = new List<Tuple<Constraint, Constraint>>();

            foreach (var constraint in sourceTable.Constraints)
            {
                var correspondingConstraint = targetTable.Constraints.FirstOrDefault(t => t.ColumnName == constraint.ColumnName);

                if (correspondingConstraint == null)
                {
                    constraintsOnlyInSource.Add(constraint);
                }
            }

            foreach (var constraint in targetTable.Constraints)
            {
                var correspondingConstraint = targetTable.Constraints.FirstOrDefault(t => t.ColumnName == constraint.ColumnName);

                if (correspondingConstraint == null)
                {
                    constraintsOnlyInTarget.Add(constraint);
                }
            }

            foreach (var sourceTableConstraint in sourceTable.Constraints)
            {
                foreach (var targetTableConstraint in targetTable.Constraints)
                {
                    if (sourceTableConstraint.ColumnName == targetTableConstraint.ColumnName)
                    {
                        constraintsInCommon.Add(Tuple.Create(sourceTableConstraint, targetTableConstraint));
                    }
                }
            }

            return new TableConstraintDiff(constraintsOnlyInSource, constraintsOnlyInTarget, constraintsInCommon);
        }
    }
}
