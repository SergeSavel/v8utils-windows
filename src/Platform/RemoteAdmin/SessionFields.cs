// Copyright 2020 Sergey Savelev
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SSavel.V8Utils.Windows.Platform.RemoteAdmin
{
    internal static class SessionFields
    {
        internal const string Session = "session";
        internal const string SessionId = "session-id";
        internal const string Infobase = "infobase";
        internal const string Connection = "connection";
        internal const string Process = "process";
        internal const string ProcessPid = "process-pid";
        internal const string ProcessHost = "process-host";
        internal const string ProcessPort = "process-port";
        internal const string UserName = "user-name";
        internal const string Host = "host";
        internal const string AppId = "app-id";
        internal const string Locale = "locale";
        internal const string StartedAt = "started-at";
        internal const string LastActiveAt = "last-active-at";
        internal const string Hibernate = "hibernate";
        internal const string PassiveSessionHibernateTime = "passive-session-hibernate-time";
        internal const string HibernateSessionTerminateTime = "hibernate-session-terminate-time";
        internal const string BlockedByDbms = "blocked-by-dbms";
        internal const string BlockedByLs = "blocked-by-ls";
        internal const string BytesAll = "bytes-all";
        internal const string BytesLast5Min = "bytes-last-5min";
        internal const string CallsAll = "calls-all";
        internal const string CallsLast5Min = "calls-last-5min";
        internal const string DbmsBytesAll = "dbms-bytes-all";
        internal const string DbmsBytesLast5Min = "dbms-bytes-last-5min";
        internal const string DbProcInfo = "db-proc-info";
        internal const string DbProcTook = "db-proc-took";
        internal const string DbProcTookAt = "db-proc-took-at";
        internal const string DurationAll = "duration-all";
        internal const string DurationAllDbms = "duration-all-dbms";
        internal const string DurationCurrent = "duration-current";
        internal const string DurationCurrentDbms = "duration-current-dbms";
        internal const string DurationCurrentDbms2 = "duration current-dbms";
        internal const string DurationLast5Min = "duration-last-5min";
        internal const string DurationLast5MinDbms = "duration-last-5min-dbms";
        internal const string MemoryCurrent = "memory-current";
        internal const string MemoryLast5Min = "memory-last-5min";
        internal const string MemoryTotal = "memory-total";
        internal const string ReadCurrent = "read-current";
        internal const string ReadLast5Min = "read-last-5min";
        internal const string ReadTotal = "read-total";
        internal const string WriteCurrent = "write-current";
        internal const string WriteLast5Min = "write-last-5min";
        internal const string WriteTotal = "write-total";
        internal const string DurationCurrentService = "duration-current-service";
        internal const string DurationLast5MinService = "duration-last-5min-service";
        internal const string DurationAllService = "duration-all-service";
        internal const string CurrentServiceName = "current-service-name";
    }
}