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
using SchemaSnapshot.DatabaseModel;

namespace SchemaSnapshot.SqlServer
{
    internal class SqlServerConstraint : Constraint
    {
        public bool IsPrimaryKey { get; }
        public bool IsUniqueConstraint { get; }
        public bool IsUnique { get; }
        public string TypeDescription { get; }
        public bool IsIncludedColumn { get; }

        public SqlServerConstraint(
            string tableName,
            string name,
            string columnName,
            bool isPrimaryKey,
            bool isUniqueConstraint,
            bool isUnique,
            string typeDescription,
            bool isIncludedColumn,
            string type) : base(
            tableName,
            name,
            columnName,
            type)
        {
            IsPrimaryKey = isPrimaryKey;
            IsUniqueConstraint = isUniqueConstraint;
            IsUnique = isUnique;
            TypeDescription = typeDescription;
            IsIncludedColumn = isIncludedColumn;
        }
    }
}