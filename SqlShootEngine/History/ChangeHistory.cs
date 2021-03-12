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
using SqlShootEngine.History;
using System.Collections.Generic;
using System.Linq;

namespace SqlShootEngine.History
{
    public class ChangeHistory
    {
        public IReadOnlyList<Change> Changes { get; }

        public ChangeHistory(List<Change> changes)
        {
            var lst = new List<Change>();

            for (var i = 0; i < changes.Count; i++)
            {
                var change = changes[i];

                var isRevertedLater = changes.Skip(i + 1).Any(c => c.Reverts(change));
                var isAppliedLater = changes.Skip(i + 1).Any(c => c.IsTheSameAs(change) || c.Type.Equals(ResourceTypes.OnChangeScript) && c.Name.Equals(change.Name));

                if (isRevertedLater && !isAppliedLater)
                {
                    lst.Add(new Change(change.Name, change.Checksum, change.Source, change.Type, ChangeStates.Reverted, change.Timestamp));
                }

                if (isRevertedLater && isAppliedLater)
                {
                    lst.Add(new Change(change.Name, change.Checksum, change.Source, change.Type, ChangeStates.ReApplied, change.Timestamp));
                }

                if (isAppliedLater && change.Type.Equals(ResourceTypes.OnChangeScript))
                {
                    lst.Add(new Change(change.Name, change.Checksum, change.Source, change.Type, ChangeStates.ReApplied, change.Timestamp));
                }

                if (!isRevertedLater && !isAppliedLater)
                {
                    lst.Add(change);
                }
            }

            Changes = lst;

        }

        public bool HasCorrespondingScriptChangeForRevertChange(Change revertChange)
        {
            return Changes.Any(change => revertChange.Reverts(change));
        }

        public bool HasEquivalentChange(Change change)
        {
            return Changes.Any(change.IsTheSameAs);
        }

        public bool HasAppliedOrReAppliedChangeWithThisName(string changeName)
        {
            return Changes.Any(c => c.Name.Equals(changeName) && (c.State.Equals(ChangeStates.Applied) || c.State.Equals(ChangeStates.ReApplied)));
        }

        public bool HasAppliedChangeWithThisNameButWithoutMatchingChecksum(string changeName, string checksum)
        {
            return Changes.Any(c => c.Name.Equals(changeName) && c.State.Equals(ChangeStates.Applied) && !c.Checksum.Equals(checksum));
        }
    }
}
