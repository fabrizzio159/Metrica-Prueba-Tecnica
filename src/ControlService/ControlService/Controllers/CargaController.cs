using System.Security.Claims;
using ControlService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CargaController : ControllerBase
{
    private readonly ICargaService _cargaService;

    public CargaController(ICargaService cargaService)
    {
        _cargaService = cargaService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se recibió ningún archivo." });

        var usuario = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "unknown";

        using var stream = file.OpenReadStream();
        var response = await _cargaService.UploadFileAsync(stream, file.FileName, usuario);
        return Ok(response);
    }

    [HttpGet("historial")]
    public async Task<IActionResult> GetHistorial()
    {
        var usuario = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "unknown";

        var historial = await _cargaService.GetHistorialAsync(usuario);
        return Ok(historial);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var carga = await _cargaService.GetCargaByIdAsync(id);
        if (carga == null) return NotFound(new { message = "Carga no encontrada." });
        return Ok(carga);
    }

    [HttpGet("{id}/detalle")]
    public async Task<IActionResult> GetDetalle(int id)
    {
        var detalle = await _cargaService.GetDetalleAsync(id);
        if (detalle == null) return NotFound(new { message = "Carga no encontrada." });
        return Ok(detalle);
    }

    [HttpPost("detalle/{detalleId}/reemplazar")]
    public async Task<IActionResult> Reemplazar(int detalleId)
    {
        var result = await _cargaService.ReemplazarDetalleAsync(detalleId);
        if (!result) return NotFound(new { message = "Detalle no encontrado o no tiene error." });
        return Ok(new { message = "Registro reemplazado exitosamente." });
    }
}
