using CargaMasivaService.Models.Messages;

namespace CargaMasivaService.Interfaces;

public interface ICargaMasivaService
{
    Task ProcesarCargaAsync(CargaMensaje mensaje);
}
