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
namespace SqlShootEngine.History
{
    public class Change
    {
        public string Name { get; }
        public string Checksum { get; }
        public string Source { get; }
        public string Type { get; }
        public string State { get; }
        public string Timestamp { get; }

        public Change(string name, string checksum, string source, string type, string state, string timestamp)
        {
            Name = name;
            Checksum = checksum;
            Source = source;
            Type = type;
            State = state;
            Timestamp = timestamp;
        }

        public bool IsTheSameAs(Change other)
        {
            return Name.Equals(other.Name) && Checksum.Equals(other.Checksum);
        }

        public bool Reverts(Change other)
        {
            if (!Type.Equals(ResourceTypes.RevertScript))
            {
                return false;
            }

            if (!other.Type.Equals(ResourceTypes.Script))
            {
                return false;
            }

            var nameWithoutRevertSuffix = Name.Replace(ResourceTypes.RevertScriptSuffix, string.Empty);

            return nameWithoutRevertSuffix.Equals(other.Name);
        }
    }
}
