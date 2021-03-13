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
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.Diff
{
    public class SchemaDiff
    {
        public SchemaDiff(List<Table> tablesOnlyInSource, List<Table> tablesOnlyInTarget, List<Tuple<Table, Table>> tablesInCommon, List<TableDiff> tableDiffs)
        {
            TablesOnlyInSource = tablesOnlyInSource;
            TablesOnlyInTarget = tablesOnlyInTarget;
            TablesInCommon = tablesInCommon;
            TableDiffs = tableDiffs;
        }

        public List<Table> TablesOnlyInSource { get; }
        public List<Table> TablesOnlyInTarget { get; }
        public List<Tuple<Table, Table>> TablesInCommon { get; }
        public List<TableDiff>  TableDiffs { get; }
    }
}