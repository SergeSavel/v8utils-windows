using System;
using SSavel.V8Utils.Windows.Platform.Entities;
using Xunit;

namespace V8Utils.Windows.Tests
{
    public class AgentTests
    {
        [Fact]
        public void Agent_should_be_created_properly()
        {
            const string commandLine =
                @"""C:\Program Files\1cv8\8.3.15.1830\bin\ragent.exe"" -srvc -agent -regport 1541 -port 1540 -range 1560:1591 -d ""C:\Program Files\1cv8\srvinfo"" -debug";

            var agent = new Agent(commandLine);

            Assert.Equal(agent.Path, @"C:\Program Files\1cv8\8.3.15.1830\bin\ragent.exe");
            Assert.Equal(agent.CommonDir, @"C:\Program Files\1cv8\");
            Assert.Equal(agent.VersionDir, @"C:\Program Files\1cv8\8.3.15.1830\");
            Assert.Equal(agent.WorkDir, @"C:\Program Files\1cv8\srvinfo");
            Assert.Equal(agent.VersionString, "8.3.15.1830");
            Assert.Equal(agent.Server, Environment.MachineName);
            Assert.Equal(agent.Port, 1540);
            Assert.True(agent.Debug);
        }
    }
}