using System.Collections.Generic;
using System.Threading.Tasks;
using SmartGreenhouse.Web.Models; // <--- Додали це, щоб бачити клас з папки Models

namespace SmartGreenhouse.Web.Services
{
    public interface ISensorService
    {
        GreenhouseState GetState();
        void ToggleHeater();
        void ToggleVentilation();
        void WaterPlants();
        void AddLight();
        Task UpdateCoordinatesAsync(double lat, double lon, double volume);
        List<string> GetLogs();
    }
}