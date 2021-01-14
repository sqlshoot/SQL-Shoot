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
using System;
using System.Runtime.Serialization;

namespace Engine.Exceptions
{
    [Serializable]
    public class SqlShootInvalidProjectException : Exception
    {
        public SqlShootInvalidProjectException()
        {
        }

        public SqlShootInvalidProjectException(string message) : base(message)
        {
        }

        public SqlShootInvalidProjectException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqlShootInvalidProjectException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}