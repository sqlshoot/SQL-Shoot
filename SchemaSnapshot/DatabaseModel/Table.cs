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
using System.Collections.Generic;

namespace SchemaSnapshot.DatabaseModel
{
    public abstract class Table
    {
        public string ParentSchema { get; }
        public string Name { get; }
        public List<Column> Columns { get; } = new List<Column>();
        public List<Constraint> Constraints { get; } = new List<Constraint>();

        protected Table(string parentSchema, string name)
        {
            ParentSchema = parentSchema;
            Name = name;
        }
    }
}