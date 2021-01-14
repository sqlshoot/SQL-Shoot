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
using Engine.DatabaseInteraction;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Engine.Resources
{
    internal class FileResource : IResource
    {
        private readonly string _source;
        private readonly string _name;
        private readonly string _filepath;

        public FileResource(string source, string name, string filepath)
        {
            _source = source;
            _name = name;
            _filepath = filepath;
        }

        public string GetSource()
        {
            return _source;
        }

        public string GetName()
        {
            return _name;
        }

        public string Read()
        {
            return File.ReadAllText(_filepath);
        }

        public string GetChecksum()
        {
            using MD5 md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(Read()))).Replace("-", string.Empty);
        }

        private Stream GetStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}