using System.Collections.Generic;
using System.Threading.Tasks;
using SmartGreenhouse.Web.Models; // <--- Додали це, щоб бачити клас з папки Models

namespace SmartGreenhouse.Web.Services
{
    public interface ISensorService
    {
        GreenhouseState GetState(string username);
        void ToggleHeater(string username);
        void ToggleVentilation(string username);
        void WaterPlants(string username);
        void AddLight(string username);
        Task UpdateCoordinatesAsync(double lat, double lon, double volume, string username);
        List<string> GetLogs(string username);
    }
}