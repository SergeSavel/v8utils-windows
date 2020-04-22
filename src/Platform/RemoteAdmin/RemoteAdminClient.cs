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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSavel.V8Utils.Platform;

namespace SSavel.V8Utils.Windows.Platform.RemoteAdmin
{
    public class RemoteAdminClient : IAgentConnection
    {
        private const int WaitForExitTimeout = 5000;
        private readonly string _agentRacPath;

        //private readonly string _rasServer;
        //private readonly int _rasPort;
        private readonly string _rasAddr;

        private IDictionary<ICluster, string> _clusterIds;

        public RemoteAdminClient(IAgent agent, string rasServer = "localhost", int rasPort = 1545)
        {
            Agent = agent;

            if (rasServer == null) throw new ArgumentNullException(nameof(rasServer));

            if (rasPort < 1 || rasPort > 65535) throw new ArgumentOutOfRangeException(nameof(rasPort));

            _rasAddr = $"{rasServer}:{rasPort.ToString(CultureInfo.InvariantCulture)}";

            _agentRacPath = Path.Combine(Path.GetDirectoryName(Agent.Path) ?? "", "rac.exe");
            if (!File.Exists(_agentRacPath))
                throw new FileNotFoundException("RAC executable not found", _agentRacPath);
        }

        public IAgent Agent { get; }

        public ICollection<ICluster> GetClusters()
        {
            var result = new List<ICluster>();

            var clusterIds = new Dictionary<ICluster, string>();

            var arguments = $"cluster {_rasAddr} list";

            void Consumer(IDictionary<string, string> values)
            {
                values.TryGetValue(ClusterFields.Host, out var clusterHost);
                if (string.IsNullOrWhiteSpace(clusterHost)) return;

                values.TryGetValue(ClusterFields.Port, out var clusterPort);
                if (string.IsNullOrWhiteSpace(clusterPort)) return;

                values.TryGetValue(ClusterFields.Name, out var clusterName);

                var cluster = new Cluster(Agent)
                {
                    Host = clusterHost,
                    Port = ushort.Parse(clusterPort),
                    Name = clusterName
                };

                result.Add(cluster);

                if (values.TryGetValue(ClusterFields.Cluster, out var uuid))
                    clusterIds.Add(cluster, uuid);
            }

            var exitCode = ExecuteRac(arguments, Consumer, out var error);

            if (exitCode != 0)
                throw new ApplicationException($"Unable to get cluster list.\n{error}");

            _clusterIds = clusterIds;

            return result.ToArray();
        }

        public ICollection<IInfobase> GetInfobases(ICluster cluster)
        {
            return GetInfobaseMap(cluster).Values.ToArray();
        }

        public ICollection<ISession> GetSessions(ICluster cluster)
        {
            if (_clusterIds == null || !_clusterIds.TryGetValue(cluster, out var clusterId))
                throw new ArgumentException("The cluster does not correspond to current connection.");

            var connectionTask = Task.Run(() => GetConnectionMap(clusterId));
            var processTask = Task.Run(() => GetProcessMap(clusterId));
            var infobaseTask = Task.Run(() => GetInfobaseMap(cluster));

            var result = new List<ISession>();

            var arguments = $"session --cluster {clusterId} {_rasAddr} list";

            void Consumer(IDictionary<string, string> values)
            {
                var session = new Session(cluster)
                {
                    Id = TryParseInt(values, SessionFields.SessionId),
                    UserName = TryParseString(values, SessionFields.UserName),
                    Host = TryParseString(values, SessionFields.Host),
                    AppId = TryParseString(values, SessionFields.AppId),
                    StartedAt = TryParseDateTime(values, SessionFields.StartedAt),
                    LastActiveAt = TryParseDateTime(values, SessionFields.LastActiveAt),
                    BlockedByDbms = TryParseUInt(values, SessionFields.BlockedByDbms),
                    BlockedByLs = TryParseUInt(values, SessionFields.BlockedByLs),
                    BytesAll = TryParseULong(values, SessionFields.BytesAll),
                    BytesLast5Min = TryParseULong(values, SessionFields.BytesLast5Min),
                    CallsAll = TryParseULong(values, SessionFields.CallsAll),
                    CallsLast5Min = TryParseULong(values, SessionFields.CallsLast5Min),
                    DbmsBytesAll = TryParseULong(values, SessionFields.DbmsBytesAll),
                    DbmsBytesLast5Min = TryParseULong(values, SessionFields.DbmsBytesLast5Min),
                    DbProcInfo = TryParseString(values, SessionFields.DbProcInfo),
                    DbProcTook = TryParseLong(values, SessionFields.DbProcTook),
                    DurationAll = TryParseULong(values, SessionFields.DurationAll),
                    DurationCurrent = TryParseULong(values, SessionFields.DurationCurrent),
                    DurationLast5Min = TryParseULong(values, SessionFields.DurationLast5Min),
                    DbmsDurationAll = TryParseULong(values, SessionFields.DurationAllDbms),
                    DbmsDurationLast5Min = TryParseULong(values, SessionFields.DurationLast5MinDbms)
                };

                if (values.ContainsKey(SessionFields.DurationCurrentDbms))
                    session.DbmsDurationCurrent = TryParseULong(values, SessionFields.DurationCurrentDbms);
                else if (values.ContainsKey(SessionFields.DurationCurrentDbms2))
                    session.DbmsDurationCurrent = TryParseULong(values, SessionFields.DurationCurrentDbms2);

                var dbProcTookAt = TryParseString(values, SessionFields.DbProcTookAt);
                session.DbProcTookAt =
                    string.IsNullOrEmpty(dbProcTookAt) ? DateTime.MinValue : DateTime.Parse(dbProcTookAt);

                if (Agent.Version >= Versions.V83)
                {
                    session.ReadCurrent = TryParseULong(values, SessionFields.ReadCurrent);
                    session.ReadLast5Min = TryParseULong(values, SessionFields.ReadLast5Min);
                    session.ReadTotal = TryParseULong(values, SessionFields.ReadTotal);

                    session.WriteCurrent = TryParseULong(values, SessionFields.WriteCurrent);
                    session.WriteLast5Min = TryParseULong(values, SessionFields.WriteLast5Min);
                    session.WriteTotal = TryParseULong(values, SessionFields.WriteTotal);

                    session.MemoryCurrent = TryParseLong(values, SessionFields.MemoryCurrent);
                    session.MemoryLast5Min = TryParseLong(values, SessionFields.MemoryLast5Min);
                    session.MemoryTotal = TryParseLong(values, SessionFields.MemoryTotal);
                }

                if (Agent.Version >= Versions.V83_5) session.Hibernate = TryParseBool(values, SessionFields.Hibernate);

                if (Agent.Version >= Versions.V83_12)
                {
                    session.ServiceDurationCurrent = TryParseULong(values, SessionFields.DurationCurrentService);
                    session.ServiceDurationLast5Min = TryParseULong(values, SessionFields.DurationLast5MinService);
                    session.ServiceDurationTotal = TryParseULong(values, SessionFields.DurationAllService);
                    session.ServiceCurrent = TryParseString(values, SessionFields.CurrentServiceName);
                }

                if (Agent.Version >= Versions.V83_13)
                {
                    // cpu time
                }

                var connectionMap = connectionTask.GetAwaiter().GetResult();
                if (connectionMap != null &&
                    connectionMap.TryGetValue(values[SessionFields.Connection], out var connection))
                    session.Connection = int.Parse(connection.Id);

                var processMap = processTask.GetAwaiter().GetResult();
                if (processMap != null && processMap.TryGetValue(values[SessionFields.Process], out var process))
                {
                    session.ProcessPid = process.Pid;
                    session.ProcessHost = process.Host;
                    session.ProcessPort = int.Parse(process.Port);
                }

                var infobaseMap = infobaseTask.GetAwaiter().GetResult();
                if (infobaseMap != null && infobaseMap.TryGetValue(values[SessionFields.Infobase], out var infobase))
                    session.Infobase = infobase.Name;

                result.Add(session);
            }

            var exitCode = ExecuteRac(arguments, Consumer, out var error);

            if (exitCode != 0)
                throw new ApplicationException($"Unable to get session list.\n{error}");

            return result.ToArray();
        }

        public void Dispose()
        {
        }

        private IDictionary<string, Infobase> GetInfobaseMap(ICluster cluster)
        {
            if (_clusterIds == null || !_clusterIds.TryGetValue(cluster, out var clusterId))
                throw new ArgumentException("The cluster does not correspond to current connection.");

            var result = new Dictionary<string, Infobase>();

            var arguments = $"infobase --cluster {clusterId} {_rasAddr} summary list";

            void Consumer(IDictionary<string, string> values)
            {
                values.TryGetValue(InfobaseFields.Name, out var infobaseName);
                if (string.IsNullOrWhiteSpace(infobaseName)) return;

                values.TryGetValue(InfobaseFields.Description, out var infobaseDescription);

                var infobase = new Infobase(cluster)
                {
                    Name = infobaseName,
                    Description = infobaseDescription
                };

                result.Add(values["infobase"], infobase);
            }

            var exitCode = ExecuteRac(arguments, Consumer, out var error);

            if (exitCode != 0)
                throw new ApplicationException($"Unable to get infobase list.\n{error}");

            return result;
        }

        private IDictionary<string, Connection> GetConnectionMap(string clusterId)
        {
            var result = new Dictionary<string, Connection>();

            var arguments = $"connection --cluster {clusterId} {_rasAddr} list";

            void Consumer(IDictionary<string, string> values)
            {
                var connection = new Connection
                {
                    Id = values["conn-id"]
                };
                result.Add(values["connection"], connection);
            }

            var exitCode = ExecuteRac(arguments, Consumer, out var error);

            if (exitCode != 0)
                throw new ApplicationException($"Unable to get connection list.\n{error}");

            return result;
        }

        private IDictionary<string, WorkingProcess> GetProcessMap(string clusterId)
        {
            var result = new Dictionary<string, WorkingProcess>();

            var arguments = $"process --cluster {clusterId} {_rasAddr} list";

            void Consumer(IDictionary<string, string> values)
            {
                var process = new WorkingProcess
                {
                    Host = values["host"],
                    Port = values["port"],
                    Pid = values["pid"]
                };
                result.Add(values["process"], process);
            }

            var exitCode = ExecuteRac(arguments, Consumer, out var error);

            if (exitCode != 0) throw new ApplicationException($"Unable to get working process list.\n{error}");

            return result;
        }

        private int ExecuteRac(string arguments, Action<IDictionary<string, string>> consumer, out string error)
        {
            int exitCode;
            string output;

            var psi = new ProcessStartInfo
            {
                FileName = _agentRacPath,
                Arguments = arguments,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            if (!Environment.UserInteractive)
            {
                psi.StandardOutputEncoding = Encoding.GetEncoding(866);
                psi.StandardErrorEncoding = Encoding.GetEncoding(866);
            }

            using (var process = Process.Start(psi))
            {
                var taskError = Task.Run(() => process.StandardError.ReadToEnd());
                output = process.StandardOutput.ReadToEnd();

                if (!process.WaitForExit(WaitForExitTimeout))
                    process.Kill();

                exitCode = process.ExitCode;
                error = taskError.Result;
            }

            if (exitCode == 0)
                using (var reader = new StringReader(output))
                {
                    IDictionary<string, string> values;
                    while ((values = GetNextValues(reader)) != null)
                        consumer.Invoke(values);
                }

            return exitCode;
        }

        private static IDictionary<string, string> GetNextValues(TextReader reader)
        {
            var values = new Dictionary<string, string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    break;

                var colonIndex = line.IndexOf(':');
                if (colonIndex != -1)
                {
                    var key = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim();
                    if (value.StartsWith("\""))
                        value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
                    values.Add(key, value);
                }
            }

            return values.Count == 0 ? null : values;
        }

        private static string TryParseString(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            return !dict.TryGetValue(key, out var stringValue) ? default : stringValue;
        }

        private static int TryParseInt(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? int.Parse(stringValue) : default;
        }

        private static uint TryParseUInt(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? uint.Parse(stringValue) : default;
        }

        private static long TryParseLong(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? long.Parse(stringValue) : default;
        }

        private static ulong TryParseULong(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? ulong.Parse(stringValue) : default;
        }

        private static DateTime TryParseDateTime(IDictionary<string, string> dict, string key,
            bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? DateTime.Parse(stringValue) : default;
        }

        private static bool TryParseBool(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            return !dict.TryGetValue(key, out var stringValue)
                ? default
                : "yes".Equals(stringValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}