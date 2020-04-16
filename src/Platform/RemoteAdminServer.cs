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
        public IAgent Agent { get; }
        public int Port { get; }
        
        private readonly Process _process;

        private bool _disposed;
        
        public RemoteAdminServer(IAgent agent, int? port = null)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
            
            var path = Path.Combine(Path.GetDirectoryName(agent.Path) ?? "", "ras.exe");
            
            if (!File.Exists(path)) throw new FileNotFoundException("RAS executable not found", path);
            
            if (port.HasValue)
            {
                if (port.Value < 1 || port.Value > 65535) throw new ArgumentOutOfRangeException(nameof(port));
                Port = port.Value;
            }
            else
            {
                var random = new Random();
                Port = random.Next(1024, 49151);
            }

            var arguments = string.Join(" ",
                "cluster",
                $"--port {Port.ToString(CultureInfo.InvariantCulture)}",
                $"{agent.Server}:{Agent.Port.ToString(CultureInfo.InvariantCulture)}");
            
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
        
        public void Dispose()
        {
            if (_disposed)
                return;
            
            // _process.StandardInput.WriteLine("\x3");
            // _process.WaitForExit(1000);
            // if (!_process.HasExited)
            //     _process.Kill();
            
            _process.Dispose();

            _disposed = true;
        }
    }
}