using HRS.HealthCheckMonitor.Configuration;
using HRS.HealthCheckMonitor.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRS.HealthCheckMonitor.Services
{
    internal class MonitorDirectoryService : IHostedService, IDisposable
    {
        private readonly HealthMonitorData _data;
        private readonly ConcurrentDictionary<string, IEnumerable<string>> _loaded = new ConcurrentDictionary<string, IEnumerable<string>>();
        private readonly ILogger _logger;
        private readonly string _directoryPath;
        private FileSystemWatcher _watcher;

        public MonitorDirectoryService(HealthCheckMonitorOptions options, HealthMonitorData data, ILogger<MonitorDirectoryService> logger)
        {
            _directoryPath = options.HealthChecksDirectory;
            _data = data;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(!string.IsNullOrWhiteSpace(_directoryPath))
            {
                var files = Directory.GetFiles(_directoryPath).Where(s => Path.GetExtension(s) == ".json");
                foreach (var file in files)
                {
                    var settings = LoadSettings(file);
                    _loaded.TryAdd(file, settings.Select(s => s.Name));
                    _data.AddUpdate(settings);
                }
                CreateFileWatcher();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher?.Dispose();
            _watcher = null;
            return Task.CompletedTask;
        }

        private void CreateFileWatcher()
        {
            _watcher = new FileSystemWatcher(_directoryPath)
            {
                Filter = "*.json",
                EnableRaisingEvents = true
            };

            _watcher.Created += WatchedAddChanged;
            _watcher.Changed += WatchedAddChanged;
            _watcher.Deleted += WatchedDeleted;
        }

        private void WatchedAddChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                var newSettings = LoadSettings(e.FullPath);
                var newNames = newSettings.Select(s => s.Name);

                var oldNames = Enumerable.Empty<string>();
                _loaded.AddOrUpdate(e.FullPath, newNames, (k, v) => { oldNames = v; return newNames; });

                var removeNames = oldNames.Where(old => !newNames.Contains(old));

                _data.Remove(removeNames);
                _data.AddUpdate(newSettings);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"There was an error loading the settings for {e.FullPath}");
            }
        }

        private void WatchedDeleted(object sender, FileSystemEventArgs e)
        {
            if(_loaded.TryRemove(e.FullPath, out var oldNames))
            {
                _data.Remove(oldNames);
            }
        }

        public IEnumerable<HealthCheckSetting> LoadSettings(string path)
        {
            var stringContent = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<HealthCheckSetting[]>(stringContent);
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
