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
using SSavel.V8Utils.Platform;

namespace SSavel.V8Utils.Windows.Platform.RemoteAdmin
{
    public class Ras : IAgentConnector
    {
        public Ras(string server, int port)
        {
            if (string.IsNullOrWhiteSpace(server)) throw new ArgumentException(nameof(server));
            Server = server;

            if (port < 1 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            Port = port;
        }

        public Ras(string serverPort)
        {
            if (string.IsNullOrWhiteSpace(serverPort)) throw new ArgumentException(nameof(serverPort));

            var parts = serverPort.Split(':');
            if (parts.Length > 2) throw new ArgumentException(nameof(serverPort));

            Server = parts[0].Trim();

            if (parts.Length > 1)
            {
                var port = int.Parse(parts[1].Trim());
                if (port < 1 || port > 65535) throw new ArgumentException(nameof(serverPort));
                Port = port;
            }
            else
            {
                Port = 1545;
            }
        }

        public string Server { get; }

        public int Port { get; }

        // public Ras(IAgent agent, int port)
        // {
        //     Agent = agent ?? throw new ArgumentNullException(nameof(agent));
        //
        //     if (port < 1 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
        //     Port = port;
        //
        //     Init();
        // }
        //
        // public Ras(IAgent agent)
        // {
        //     Agent = agent ?? throw new ArgumentNullException(nameof(agent));
        //
        //     var random = new Random();
        //     Port = random.Next(1024, 49151);
        //
        //     Init();
        // }

        public IAgentConnection ConnectAgent(IAgent agent)
        {
            return new Rac(agent, Server, Port);
        }
    }
}