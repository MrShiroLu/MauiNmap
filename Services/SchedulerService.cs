using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace NmapMaui.Services
{
    /// <summary>
    /// Quartz.NET tabanlı tarama zamanlayıcısı.
    /// IServiceProvider artık statik property yerine Scheduler.Context'e
    /// koyuluyor → thread-safe ve test edilebilir.
    /// </summary>
    public class SchedulerService : ISchedulerService
    {
        private IScheduler?           _scheduler;
        private readonly IServiceProvider _services;

        public SchedulerService(IServiceProvider services)
        {
            _services = services;
            // Artık NmapScanJob.Services statik ataması yapılmıyor
        }

        public async Task StartAsync()
        {
            if (_scheduler != null) return;

            var factory  = new StdSchedulerFactory();
            _scheduler   = await factory.GetScheduler();

            // Services'i scheduler context'ine koy (statik değil, per-scheduler)
            _scheduler.Context.Put("services", _services);

            await _scheduler.Start();
        }

        public async Task ScheduleNmapScanAsync(string host, int startPort, int endPort, string cronExpression)
        {
            await StartAsync();

            var job = JobBuilder.Create<NmapScanJob>()
                .WithIdentity($"nmap-{host}-{Guid.NewGuid():N}")
                .UsingJobData("host",      host)
                .UsingJobData("startPort", startPort)
                .UsingJobData("endPort",   endPort)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"trig-{Guid.NewGuid():N}")
                .WithCronSchedule(cronExpression)
                .Build();

            await _scheduler!.ScheduleJob(job, trigger);
        }

        public async Task<int> GetActiveJobCountAsync()
        {
            if (_scheduler == null) return 0;
            var keys = await _scheduler.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup());
            return keys.Count;
        }

        public async Task ShutdownAsync()
        {
            if (_scheduler != null) await _scheduler.Shutdown();
        }
    }

    public class NmapScanJob : IJob
    {
        // Artık statik IServiceProvider property yok!
        // Services, scheduler context'inden alınır.

        public async Task Execute(IJobExecutionContext context)
        {
            // Scheduler.Context'ten services'i al
            if (context.Scheduler.Context["services"] is not IServiceProvider services)
                return;

            var host      = context.MergedJobDataMap.GetString("host") ?? "";
            var startPort = context.MergedJobDataMap.GetInt("startPort");
            var endPort   = context.MergedJobDataMap.GetInt("endPort");

            // Scoped scope — Singleton olmayan servislerin doğru lifetime'ı için
            using var scope  = services.CreateScope();
            var scanner  = scope.ServiceProvider.GetRequiredService<INetworkScanner>();
            var logging  = scope.ServiceProvider.GetRequiredService<ILoggingService>();

            await logging.LogAsync("ScheduledScan", "Scheduler", $"{host} {startPort}-{endPort}");
            var result = await scanner.ScanPortRangeAsync(host, startPort, endPort);
            await logging.LogAsync(
                "ScheduledScan.Complete",
                "Scheduler",
                $"success={result.IsSuccess}",
                result.IsSuccess ? "Info" : "Warning");
        }
    }
}
