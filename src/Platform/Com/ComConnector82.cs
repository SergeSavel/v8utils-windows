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
using System.Runtime.InteropServices;
using SSavel.V8Utils.Platform;
using V82;

namespace SSavel.V8Utils.Windows.Platform.Com
{
    public sealed class ComConnector82 : IComConnector

    {
        private bool _disposed;
        internal COMConnector ComConnector { get; private set; } = new COMConnector();

        public IAgentConnection ConnectAgent(IAgent agent)
        {
            if (_disposed)
                throw new ObjectDisposedException(ToString());

            return new AgentConnection82(agent, this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (ComConnector != null)
            {
                Marshal.ReleaseComObject(ComConnector);
                ComConnector = null;
            }

            _disposed = true;
        }

        ~ComConnector82()
        {
            Dispose(false);
        }
    }
}