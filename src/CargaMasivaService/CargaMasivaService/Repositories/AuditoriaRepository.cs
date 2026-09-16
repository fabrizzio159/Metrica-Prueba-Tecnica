using CargaMasivaService.Data;
using CargaMasivaService.Interfaces;
using CargaMasivaService.Models.Entities;

namespace CargaMasivaService.Repositories;

public class AuditoriaRepository : Repository<AuditoriaFallo>, IAuditoriaRepository
{
    public AuditoriaRepository(CargaMasivaDbContext context) : base(context)
    {
    }
}
