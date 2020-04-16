﻿// Copyright 2020 Sergey Savelev
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
using System.Runtime.InteropServices;
using SSavel.V8Utils.Platform;
using V83;

namespace SSavel.V8Utils.Windows.Platform.Com
{
    public sealed class AgentComConnection83 : IAgentConnection
    {
        private IDictionary<ICluster, IClusterInfo> _clusterInfos;
        private IServerAgentConnection _connection;

        public AgentComConnection83(IAgent agent, ComConnector83 connector)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));

            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            Agent = agent;
            _connection = connector.ComConnector.ConnectAgent(agent.ConnectionString);
        }

        public IAgent Agent { get; }

        public ICollection<ICluster> GetClusters()
        {
            if (_disposed)
                throw new ObjectDisposedException(ToString());

            ReleaseClusterInfos();

            var items = _connection.GetClusters();

            var result = new List<ICluster>(items.Length);
            _clusterInfos = new Dictionary<ICluster, IClusterInfo>(items.Length);

            foreach (var item in items)
            {
                var clusterInfo = (IClusterInfo) item;
                var clusterHost = clusterInfo.HostName;
                var clusterPort = clusterInfo.MainPort;

                if (string.IsNullOrWhiteSpace(clusterHost))
                {
                    Marshal.ReleaseComObject(item);
                    continue;
                }

                try
                {
                    _connection.Authenticate(clusterInfo, string.Empty, string.Empty);
                }
                catch (Exception)
                {
                    Marshal.ReleaseComObject(item);
                    continue;
                }

                var cluster = new Cluster(Agent)
                    {Host = clusterHost, Port = clusterPort, Name = clusterInfo.ClusterName};
                result.Add(cluster);

                _clusterInfos.Add(cluster, clusterInfo);
            }

            return result;
        }

        public ICollection<IInfobase> GetInfobases(ICluster cluster)
        {
            if (_disposed)
                throw new ObjectDisposedException(ToString());

            if (_clusterInfos == null || !_clusterInfos.TryGetValue(cluster, out var clusterInfo))
                throw new ArgumentException("The cluster does not correspond with current connection.");

            var items = _connection.GetInfoBases(clusterInfo);
            var result = new List<IInfobase>(items.Length);

            foreach (var item in items)
            {
                var infobaseShort = (IInfoBaseShort) item;
                var infobase = new Infobase(cluster) {Name = infobaseShort.Name, Description = infobaseShort.Descr};
                Marshal.ReleaseComObject(item);
                result.Add(infobase);
            }

            return result;
        }

        public ICollection<ISession> GetSessions(ICluster cluster)
        {
            if (_disposed)
                throw new ObjectDisposedException(ToString());

            if (_clusterInfos == null || !_clusterInfos.TryGetValue(cluster, out var clusterInfo))
                throw new ArgumentException("The cluster does not correspond with current connection.");

            var items = _connection.GetSessions(clusterInfo);
            var result = new List<ISession>(items.Length);

            foreach (var item in items)
            {
                var session = new Session(cluster);
                result.Add(session);

                var sessionInfo = (ISessionInfo) item;

                session.Infobase = sessionInfo.infoBase.Name;
                session.Id = sessionInfo.SessionID;
                session.UserName = sessionInfo.userName;
                session.Host = sessionInfo.Host;
                session.AppId = sessionInfo.AppID;
                session.StartedAt = sessionInfo.StartedAt;
                session.LastActiveAt = sessionInfo.LastActiveAt;
                session.BlockedByDbms = sessionInfo.blockedByDBMS;
                session.BlockedByLs = sessionInfo.blockedByLS;
                session.BytesAll = sessionInfo.bytesAll;
                session.BytesLast5Min = sessionInfo.bytesLast5Min;
                session.CallsAll = sessionInfo.callsAll;
                session.CallsLast5Min = sessionInfo.callsLast5Min;
                session.DbmsBytesAll = sessionInfo.dbmsBytesAll;
                session.DbmsBytesLast5Min = sessionInfo.dbmsBytesLast5Min;
                session.DbProcInfo = sessionInfo.dbProcInfo;
                session.DbProcTook = sessionInfo.dbProcTook;
                session.DbProcTookAt = sessionInfo.dbProcTookAt;
                session.DurationAll = sessionInfo.durationAll;
                session.DurationCurrent = sessionInfo.durationCurrent;
                session.DurationLast5Min = sessionInfo.durationLast5Min;
                session.DbmsDurationAll = sessionInfo.durationAllDBMS;
                session.DbmsDurationCurrent = sessionInfo.durationCurrentDBMS;
                session.DbmsDurationLast5Min = sessionInfo.durationLast5MinDBMS;

                var connection = (IConnectionShort) sessionInfo.connection;
                if (connection != null)
                {
                    session.Connection = connection.ConnID;
                    Marshal.ReleaseComObject(connection); // ???
                }

                var process = (IWorkingProcessInfo) sessionInfo.process;
                if (process != null)
                {
                    session.ProcessHost = process.HostName;
                    session.ProcessPort = process.MainPort;
                    session.ProcessPid = process.PID;
                    Marshal.ReleaseComObject(process); // ???
                }

                // since 8.3.1
                session.MemoryTotal = sessionInfo.MemoryAll;
                session.MemoryLast5Min = sessionInfo.MemoryLast5Min;
                session.MemoryCurrent = sessionInfo.MemoryCurrent;
                session.ReadTotal = sessionInfo.InBytesAll;
                session.ReadLast5Min = sessionInfo.InBytesLast5Min;
                session.ReadCurrent = sessionInfo.InBytesCurrent;
                session.WriteTotal = sessionInfo.OutBytesAll;
                session.WriteLast5Min = sessionInfo.OutBytesLast5Min;
                session.WriteCurrent = sessionInfo.OutBytesCurrent;

                if (Agent.Version >= Versions.V83_5) session.Hibernate = sessionInfo.Hibernate;

                if (Agent.Version >= Versions.V83_12)
                {
                    session.ServiceDurationTotal = sessionInfo.durationAllService;
                    session.ServiceDurationLast5Min = sessionInfo.durationLast5MinService;
                    session.ServiceDurationCurrent = sessionInfo.durationCurrentService;
                    session.ServiceCurrent = sessionInfo.CurrentServiceName;
                }

                if (Agent.Version >= Versions.V83_13)
                {
                    session.CpuTimeTotal = sessionInfo.cpuTimeAll;
                    session.CpuTimeLast5Min = sessionInfo.cpuTimeLast5Min;
                    session.CpuTimeCurrent = sessionInfo.cpuTimeCurrent;
                }

                Marshal.ReleaseComObject(item);
            }

            return result;
        }

        #region Dispose

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ReleaseClusterInfos()
        {
            if (_clusterInfos == null) return;

            foreach (var clusterInfo in _clusterInfos.Values)
                Marshal.ReleaseComObject(clusterInfo);

            _clusterInfos = null;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            ReleaseClusterInfos();

            if (_connection != null)
            {
                Marshal.ReleaseComObject(_connection);
                _connection = null;
            }

            _disposed = true;
        }

        ~AgentComConnection83()
        {
            Dispose(false);
        }

        #endregion
    }
}