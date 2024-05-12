using KolosMarcin.DTOs;
using KolosMarcin.Models;
using KolosMarcin.Repositories;

namespace KolosMarcin.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;

    public PrescriptionService(IPrescriptionRepository prescriptionRepository)
    {
        _prescriptionRepository = prescriptionRepository;
    }
    


   public async Task<List<Prescription>> GetPrescriptions(string lastname)
   {
       return await _prescriptionRepository.GetPrescriptions(lastname);
   }

   public async Task<int> AddMedicament(List<MedicamentDto> medicamentDtos, int id)
   {
       return await _prescriptionRepository.AddMedicament(medicamentDtos, id);
   }

   public async Task<int> AddMedicamentAsync(List<MedicamentToAddDto> medicamentToAddDtos, int id)
   {
       return await _prescriptionRepository.AddMedicamentAsync(medicamentToAddDtos, id);
   }
}