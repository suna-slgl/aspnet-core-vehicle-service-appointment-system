using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// Vehicle Service Implementation - Scoped Lifetime
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesByUserIdAsync(string userId)
        {
            return await _context.Vehicles
                .Include(v => v.Appointments)
                .Where(v => v.UserId == userId && v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Appointments)
                    .ThenInclude(a => a.ServiceType)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Vehicle?> GetVehicleByIdAndUserAsync(int id, string userId)
        {
            return await _context.Vehicles
                .Include(v => v.Appointments)
                    .ThenInclude(a => a.ServiceType)
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            vehicle.CreatedAt = DateTime.UtcNow;
            vehicle.IsActive = true;
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return false;

            // Soft delete
            vehicle.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsLicensePlateExistsAsync(string licensePlate, int? excludeId = null)
        {
            var normalizedPlate = licensePlate.Replace(" ", "").ToUpper();
            return await _context.Vehicles
                .AnyAsync(v => v.LicensePlate.Replace(" ", "").ToUpper() == normalizedPlate 
                            && v.IsActive 
                            && (excludeId == null || v.Id != excludeId));
        }

        public async Task<int> GetVehicleCountByUserAsync(string userId)
        {
            return await _context.Vehicles
                .CountAsync(v => v.UserId == userId && v.IsActive);
        }
    }
}

