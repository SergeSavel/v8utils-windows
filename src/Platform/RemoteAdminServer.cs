using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using SSavel.V8Utils.Platform;

namespace SSavel.V8Utils.Windows.Platform
{
    public class RemoteAdminServer : IDisposable
    {
        private bool _disposed;

        private Process _process;

        public RemoteAdminServer(IAgent agent, int port)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));

            if (port < 1 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            Port = port;

            Init();
        }

        public RemoteAdminServer(IAgent agent)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));

            var random = new Random();
            Port = random.Next(1024, 49151);

            Init();
        }

        public IAgent Agent { get; }
        public int Port { get; }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }

            _disposed = true;
        }

        private void Init()
        {
            var path = Path.Combine(Path.GetDirectoryName(Agent.Path) ?? "", "ras.exe");
            if (!File.Exists(path)) throw new FileNotFoundException("RAS executable not found", path);

            var arguments = string.Join(" ",
                "cluster",
                $"--port {Port.ToString(CultureInfo.InvariantCulture)}",
                $"{Agent.Server}:{Agent.Port.ToString(CultureInfo.InvariantCulture)}");

            var psi = new ProcessStartInfo
            {
                FileName = path,
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

            _process = Process.Start(psi);
        }

        public void Shutdown()
        {
            if (_process == null) return;
            _process.StandardInput.WriteLine("\x3");
            _process.WaitForExit(1000);
            if (!_process.HasExited)
                _process.Kill();
        }
    }
}