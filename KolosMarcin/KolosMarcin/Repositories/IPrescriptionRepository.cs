using KolosMarcin.DTOs;
using KolosMarcin.Models;
using KolosMarcin.DTOs;

namespace KolosMarcin.Repositories;

public interface IPrescriptionRepository
{
    Task<List<Prescription>> GetPrescriptions(string lastname);
    Task<int> AddMedicament(List<MedicamentDto> medicamentDtos, int id);
    Task<int> AddMedicamentAsync(List<MedicamentToAddDto> medicamentToAddDtos, int id);
}