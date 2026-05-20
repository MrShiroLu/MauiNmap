using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace NmapMaui.Services
{
    // Wraps Quartz.NET to satisfy the "periyodik tarama / Görev Zamanlayıcı"
    // commitment from the project proposal. Jobs run NmapScanJob which scans a
    // host/port-range on the configured cron schedule and writes the result to
    // the database + activity log.
    public class SchedulerService : ISchedulerService
    {
        private IScheduler? _scheduler;
        private readonly IServiceProvider _services;

        public SchedulerService(IServiceProvider services)
        {
            _services = services;
            NmapScanJob.Services = services;
        }

        public async Task StartAsync()
        {
            if (_scheduler != null) return;
            var factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();
            await _scheduler.Start();
        }

        public async Task ScheduleNmapScanAsync(string host, int startPort, int endPort, string cronExpression)
        {
            await StartAsync();

            var job = JobBuilder.Create<NmapScanJob>()
                .WithIdentity($"nmap-{host}-{Guid.NewGuid():N}")
                .UsingJobData("host", host)
                .UsingJobData("startPort", startPort)
                .UsingJobData("endPort", endPort)
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
        public static IServiceProvider? Services { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            if (Services == null) return;
            var host = context.MergedJobDataMap.GetString("host") ?? "";
            var startPort = context.MergedJobDataMap.GetInt("startPort");
            var endPort = context.MergedJobDataMap.GetInt("endPort");

            var scanner = (INetworkScanner)Services.GetService(typeof(INetworkScanner))!;
            var logging = (ILoggingService)Services.GetService(typeof(ILoggingService))!;

            await logging.LogAsync("ScheduledScan", "Scheduler", $"{host} {startPort}-{endPort}");
            var result = await scanner.ScanPortRangeAsync(host, startPort, endPort);
            await logging.LogAsync("ScheduledScan.Complete", "Scheduler", $"success={result.IsSuccess}", result.IsSuccess ? "Info" : "Warning");
        }
    }
}
