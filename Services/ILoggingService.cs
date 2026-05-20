using System.Collections.Generic;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public interface ILoggingService
    {
        Task LogAsync(string action, string category, string details = "", string level = "Info");
        Task<List<ActivityLog>> GetAllAsync();
    }
}
