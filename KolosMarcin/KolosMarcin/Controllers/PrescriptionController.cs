using KolosMarcin.DTOs;
using KolosMarcin.Exceptions;
using KolosMarcin.Services;
using Microsoft.AspNetCore.Mvc;

namespace KolosMarcin.Controllers;
[Route("api/prescription")]
[ApiController]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPrescriptions(string lastname)
    {
        return Ok(await _prescriptionService.GetPrescriptions(lastname));
    }

    [HttpPost]
    public async Task<IActionResult> AddMedicament(List<MedicamentToAddDto> medicamentDtos, int id)
    {
        try
        {
            return Ok(await _prescriptionService.AddMedicamentAsync(medicamentDtos, id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Wystąpił błąd: {e.Message}");
        }
    }
}