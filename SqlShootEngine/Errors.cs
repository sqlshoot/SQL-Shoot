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
using System.Linq;
using SqlShootEngine.DatabaseInteraction;
using SqlShootEngine.DatabaseInteraction.ChangeHistory;

namespace SqlShootEngine
{
    /// <summary>
    /// Errors detected in the SQL Shoot configuration
    /// </summary>
    public class Errors
    {
        /// <summary>
        /// Resources that are resolved by multiple Script Paths
        /// </summary>
        public List<IResource> DuplicateResources { get; }

        /// <summary>
        /// Script Paths that have been specified multiple times
        /// </summary>
        public List<string> DuplicateScriptPaths { get; }

        /// <summary>
        /// Changes that have been applied to the database where the corresponding script file has been modified
        /// </summary>
        public List<Change> ModifiedAppliedChanges { get; }

        /// <summary>
        /// Changes that have been applied to the database but have no corresponding script file
        /// </summary>
        public List<Change> MissingChanges { get; }

        internal Errors(
            List<IResource> duplicateResources,
            List<string> duplicateScriptPaths,
            List<Change> modifiedAppliedChanges,
            List<Change> missingChanges)
        {
            DuplicateResources = duplicateResources;
            DuplicateScriptPaths = duplicateScriptPaths;
            ModifiedAppliedChanges = modifiedAppliedChanges;
            MissingChanges = missingChanges;
        }

        public bool HasAnyErrors()
        {
            return DuplicateResources.Any() || DuplicateScriptPaths.Any() || ModifiedAppliedChanges.Any() || MissingChanges.Any();
        }
    }
}