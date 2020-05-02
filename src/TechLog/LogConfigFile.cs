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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SSavel.V8Utils.Platform;
using SSavel.V8Utils.TechLog;
using SSavel.V8Utils.Windows.Platform.Entities;

namespace SSavel.V8Utils.Windows.TechLog
{
    public class LogConfigFile : ILogConfigFile
    {
        private static readonly Regex RegexConfLocation = new Regex(@"^\s*ConfLocation\s*=\s*(.+)\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private LogConfigFile(string path, Version version)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        public string Path { get; }

        public Version Version { get; }

        public static ICollection<LogConfigFile> GetFiles(IEnumerable<Agent> agents)
        {
            if (agents == null) throw new ArgumentNullException(nameof(agents));

            var result = new List<LogConfigFile>();

            var agentsArray = agents.ToArray();

            var versions = agentsArray
                .Select(service => Versions.GetMajor(service.Version))
                .Distinct();

            foreach (var version in versions)
            {
                var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var versionServices = agentsArray
                    .Where(service => Versions.GetMajor(service.Version) == version);

                foreach (var service in versionServices)
                {
                    var confLocation = System.IO.Path.Combine(service.CommonDir, "conf");
                    try
                    {
                        var confCfgText =
                            File.ReadAllText(System.IO.Path.Combine(service.VersionDir, "bin", "conf", "conf.cfg"));
                        var match = RegexConfLocation.Match(confCfgText);
                        if (match.Success)
                            confLocation = match.Groups[1].Value;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    var path = System.IO.Path.Combine(confLocation, "logcfg.xml");
                    paths.Add(path);
                }

                result.AddRange(paths.Select(path => new LogConfigFile(path, version)));
            }

            return result;
        }
    }
}