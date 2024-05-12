using KolosMarcin.DTOs;
using KolosMarcin.Models;

namespace KolosMarcin.Services;

public interface IPrescriptionService
{
    Task<List<Prescription>> GetPrescriptions(string lastname);
    Task<int> AddMedicament(List<MedicamentDto> medicamentDtos, int id);
    Task<int> AddMedicamentAsync(List<MedicamentToAddDto> medicamentToAddDtos, int id);
}