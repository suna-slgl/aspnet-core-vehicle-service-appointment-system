using Microsoft.EntityFrameworkCore;
using VehicleServiceApp.Data;
using VehicleServiceApp.Models;
using VehicleServiceApp.Services.Interfaces;

namespace VehicleServiceApp.Services
{
    /// <summary>
    /// ServiceType Service Implementation - Scoped Lifetime
    /// </summary>
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly ApplicationDbContext _context;

        public ServiceTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceType>> GetAllServiceTypesAsync()
        {
            return await _context.ServiceTypes
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceType>> GetActiveServiceTypesAsync()
        {
            return await _context.ServiceTypes
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<ServiceType?> GetServiceTypeByIdAsync(int id)
        {
            return await _context.ServiceTypes
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<ServiceType> CreateServiceTypeAsync(ServiceType serviceType)
        {
            serviceType.CreatedAt = DateTime.Now;
            serviceType.IsActive = true;
            _context.ServiceTypes.Add(serviceType);
            await _context.SaveChangesAsync();
            return serviceType;
        }

        public async Task<ServiceType> UpdateServiceTypeAsync(ServiceType serviceType)
        {
            _context.ServiceTypes.Update(serviceType);
            await _context.SaveChangesAsync();
            return serviceType;
        }

        public async Task<bool> DeleteServiceTypeAsync(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null) return false;

            // Check if there are any appointments using this service type
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceTypeId == id);
            if (hasAppointments)
            {
                // Soft delete if there are existing appointments
                serviceType.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null) return false;

            serviceType.IsActive = !serviceType.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    /// <summary>
    /// Technician Service Implementation - Scoped Lifetime
    /// </summary>
    public class TechnicianService : ITechnicianService
    {
        private readonly ApplicationDbContext _context;

        public TechnicianService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Technician>> GetAllTechniciansAsync()
        {
            return await _context.Technicians
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Technician>> GetActiveTechniciansAsync()
        {
            return await _context.Technicians
                .Where(t => t.IsActive)
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();
        }

        public async Task<Technician?> GetTechnicianByIdAsync(int id)
        {
            return await _context.Technicians
                .Include(t => t.Appointments)
                    .ThenInclude(a => a.ServiceType)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Technician> CreateTechnicianAsync(Technician technician)
        {
            technician.CreatedAt = DateTime.Now;
            technician.IsActive = true;
            _context.Technicians.Add(technician);
            await _context.SaveChangesAsync();
            return technician;
        }

        public async Task<Technician> UpdateTechnicianAsync(Technician technician)
        {
            _context.Technicians.Update(technician);
            await _context.SaveChangesAsync();
            return technician;
        }

        public async Task<bool> DeleteTechnicianAsync(int id)
        {
            var technician = await _context.Technicians.FindAsync(id);
            if (technician == null) return false;

            // Check if there are any appointments assigned to this technician
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.TechnicianId == id);
            if (hasAppointments)
            {
                // Soft delete if there are existing appointments
                technician.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Technicians.Remove(technician);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var technician = await _context.Technicians.FindAsync(id);
            if (technician == null) return false;

            technician.IsActive = !technician.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Technician>> GetAvailableTechniciansAsync(DateTime date, TimeSpan time)
        {
            var busyTechnicianIds = await _context.Appointments
                .Where(a => a.AppointmentDate == date.Date)
                .Where(a => a.AppointmentTime == time)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Where(a => a.TechnicianId.HasValue)
                .Select(a => a.TechnicianId!.Value)
                .ToListAsync();

            return await _context.Technicians
                .Where(t => t.IsActive)
                .Where(t => !busyTechnicianIds.Contains(t.Id))
                .Where(t => t.WorkStartTime <= time && t.WorkEndTime > time)
                .OrderBy(t => t.FirstName)
                .ToListAsync();
        }
    }
}
