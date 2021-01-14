#region derivation-license
/* This file contains work derived from another open source tool (https://github.com/flyway/flyway).
 * Note this file has been modified after derivation by the author of SQL Shoot.
 * To comply with the original license, the notice as presented in the original work
 * at the time of derivation, is reproduced here:
 *
 * Copyright © Red Gate Software Ltd 2010-2020
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *         http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion
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
using System.Text.RegularExpressions;

namespace Engine.Databases.PostgreSQL
{
    internal class PostgreSQLNonTransactionalSqlDetector : INonTransactionalSqlDetector
    {
        private readonly Regex COPY_FROM_STDIN_REGEX = new Regex("^COPY( .*)? FROM STDIN");
        private readonly Regex CREATE_DATABASE_TABLESPACE_SUBSCRIPTION_REGEX = new Regex("^(CREATE|DROP) (DATABASE|TABLESPACE|SUBSCRIPTION)");
        private readonly Regex ALTER_SYSTEM_REGEX = new Regex("^ALTER SYSTEM");
        private readonly Regex CREATE_INDEX_CONCURRENTLY_REGEX = new Regex("^(CREATE|DROP)( UNIQUE)? INDEX CONCURRENTLY");
        private readonly Regex REINDEX_REGEX = new Regex("^REINDEX( VERBOSE)? (SCHEMA|DATABASE|SYSTEM)");
        private readonly Regex VACUUM_REGEX = new Regex("^VACUUM");
        private readonly Regex DISCARD_ALL_REGEX = new Regex("^DISCARD ALL");
        private readonly Regex ALTER_TYPE_ADD_VALUE_REGEX = new Regex("^ALTER TYPE( .*)? ADD VALUE");

        private readonly DatabaseVersion _databaseVersion;

        public PostgreSQLNonTransactionalSqlDetector(DatabaseVersion databaseVersion)
        {
            _databaseVersion = databaseVersion;
        }

        public bool IsSqlNonTransactional(string sql)
        {
            if (COPY_FROM_STDIN_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (CREATE_DATABASE_TABLESPACE_SUBSCRIPTION_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (ALTER_SYSTEM_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (CREATE_INDEX_CONCURRENTLY_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (REINDEX_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (VACUUM_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (DISCARD_ALL_REGEX.IsMatch(sql))
            {
                return true;
            }

            if (_databaseVersion.Major < 12 && ALTER_TYPE_ADD_VALUE_REGEX.IsMatch(sql))
            {
                return true;
            }

            return false;
        }
    }
}
