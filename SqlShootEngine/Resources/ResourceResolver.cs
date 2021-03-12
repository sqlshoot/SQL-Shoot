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
using System.IO;
using System.Linq;
using DatabaseInteraction;

namespace SqlShootEngine.Resources
{
    internal class ResourceResolver : IResourceResolver
    {
        public ResourceResolverResult ResolveForRun(List<string> scriptPaths)
        {
            return Resolve(scriptPaths, RunScriptPathFilter, false);
        }

        public ResourceResolverResult ResolveForRevert(List<string> scriptPaths)
        {
            return Resolve(scriptPaths, RevertScriptPathFilter, true);
        }

        private bool RunScriptPathFilter(string path)
        {
            return ResourcePathFilter(path) && !path.EndsWith($"{ResourceTypes.RevertScriptSuffix}.sql");
        }

        private bool RevertScriptPathFilter(string path)
        {
            return ResourcePathFilter(path) && path.EndsWith($"{ResourceTypes.RevertScriptSuffix}.sql");
        }

        private bool ResourcePathFilter(string path)
        {
            return path.EndsWith(".sql");
        }

        private static ResourceResolverResult Resolve(List<string> scriptPaths, Func<string, bool> scriptPathFilter, bool reverse)
        {
            var resources = new List<IResource>();
            var expandedScriptPaths = new List<string>();

            foreach (var scriptPath in scriptPaths)
            {
                if (IsDirectoryScriptPath(scriptPath))
                {
                    foreach (var scriptPathInDirectory in GetFilesInScriptPathDirectory(scriptPath))
                    {
                        if (scriptPathFilter(scriptPathInDirectory))
                        {
                            expandedScriptPaths.Add(scriptPathInDirectory);
                        }
                    }
                }
                else
                {
                    if (scriptPathFilter(scriptPath))
                    {
                        expandedScriptPaths.Add(scriptPath);
                    }
                }
            }

            foreach (var scriptPath in expandedScriptPaths)
            {
                if (scriptPathFilter(scriptPath))
                {
                    resources.Add(CreateResourceFromFilepath(scriptPath));
                }
            }

            if (reverse)
            {
                resources.Reverse();
            }

            return new ResourceResolverResult(
                resources.ToList(),
                GetDuplicates(resources),
                GetDuplicates(expandedScriptPaths).ToList());
        }

        private static List<IResource> GetDuplicates(List<IResource> resources)
        {
            var uniqueNameList = new HashSet<string>();
            var duplicates = new HashSet<IResource>();

            foreach (var resource in resources)
            {
                var resourceName = resource.GetName();

                if (uniqueNameList.Contains(resourceName))
                {
                    duplicates.Add(resource);
                }

                uniqueNameList.Add(resourceName);
            }

            return duplicates.ToList();
        }

        private static List<string> GetDuplicates(List<string> list)
        {
            var uniqueList = new HashSet<string>();
            var duplicates = new HashSet<string>();

            foreach (var item in list)
            {
                if (uniqueList.Contains(item))
                {
                    duplicates.Add(item);
                }

                uniqueList.Add(item);
            }

            return duplicates.ToList();
        }

        private static IEnumerable<string> GetFilesInScriptPathDirectory(string scriptPathDirectory)
        {
            scriptPathDirectory = scriptPathDirectory.Trim('*');

            var directories = Directory.GetDirectories(scriptPathDirectory, "*.*", SearchOption.TopDirectoryOnly).ToList();
            var files = Directory.GetFiles(scriptPathDirectory, "*.*", SearchOption.TopDirectoryOnly).ToList();

            directories.Sort();
            files.Sort();

            var list = new List<string>();

            foreach (var entry in directories.Concat(files))
            {
                if (Directory.Exists(entry))
                {
                    list.AddRange(GetFilesInScriptPathDirectory(entry));
                }
                else
                {
                    list.Add(entry);
                }
            }

            return list;
        }

        private static bool IsDirectoryScriptPath(string scriptPath)
        {
            return scriptPath.EndsWith("*");
        }

        private static IResource CreateResourceFromFilepath(string filepath)
        {
            return new FileResource(
                filepath,
                Path.GetFileName(filepath).Replace(".sql", string.Empty),
                filepath);
        }
    }
}