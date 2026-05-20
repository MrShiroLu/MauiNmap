using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public interface ISchedulerService
    {
        Task StartAsync();
        Task ScheduleNmapScanAsync(string host, int startPort, int endPort, string cronExpression);
        Task<int> GetActiveJobCountAsync();
        Task ShutdownAsync();
    }
}
