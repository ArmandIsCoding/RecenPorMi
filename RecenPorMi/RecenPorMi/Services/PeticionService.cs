using Microsoft.EntityFrameworkCore;
using RecenPorMi.Data;
using RecenPorMi.Data.Models;
using System.Security.Cryptography;
using System.Text;

namespace RecenPorMi.Services
{
    public interface IPeticionService
    {
        Task<List<Peticion>> ObtenerPeticionesRecientesAsync();
        Task<Peticion?> ObtenerPeticionPorIdAsync(int id);
        Task<Peticion> CrearPeticionAsync(string descripcionBreve, string? contenidoCompleto, string userId, bool publicarAnonimamente, List<string>? rutasImagenes = null);
        Task<bool> RegistrarRezoAsync(int peticionId, string ipAddress);
    }

    public class PeticionService : IPeticionService
    {
        private readonly ApplicationDbContext _context;

        public PeticionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Peticion>> ObtenerPeticionesRecientesAsync()
        {
            return await _context.Peticiones
                .Include(p => p.Usuario) // Incluir usuario para mostrar nombre real
                .Include(p => p.Imagenes) // Incluir imágenes asociadas
                .OrderByDescending(p => p.FechaPublicacion)
                .Take(50)
                .ToListAsync();
        }

        public async Task<Peticion?> ObtenerPeticionPorIdAsync(int id)
        {
            return await _context.Peticiones
                .Include(p => p.Usuario)
                .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Peticion> CrearPeticionAsync(string descripcionBreve, string? contenidoCompleto, string userId, bool publicarAnonimamente, List<string>? rutasImagenes = null)
        {
            var peticion = new Peticion
            {
                DescripcionBreve = descripcionBreve.Trim(),
                ContenidoCompleto = string.IsNullOrWhiteSpace(contenidoCompleto) ? null : contenidoCompleto,
                UserId = userId,
                PublicarAnonimamente = publicarAnonimamente,
                FechaPublicacion = DateTime.Now
            };

            _context.Peticiones.Add(peticion);
            await _context.SaveChangesAsync();

            // Agregar imágenes si existen
            if (rutasImagenes != null && rutasImagenes.Any())
            {
                int orden = 0;
                foreach (var ruta in rutasImagenes)
                {
                    var imagen = new PeticionImagen
                    {
                        PeticionId = peticion.Id,
                        NombreArchivo = Path.GetFileName(ruta),
                        RutaImagen = ruta,
                        Orden = orden++,
                        FechaSubida = DateTime.Now
                    };
                    _context.PeticionImagenes.Add(imagen);
                }
                await _context.SaveChangesAsync();
            }

            return peticion;
        }

        public async Task<bool> RegistrarRezoAsync(int peticionId, string ipAddress)
        {
            var ipHash = ComputeSha256Hash(ipAddress);

            // Anti-spam: Verificar si el IP ya rezó en los últimos 5 minutos
            var limiteSpam = DateTime.Now.AddMinutes(-5);
            var rezoReciente = await _context.Rezos
                .AnyAsync(r => r.PeticionId == peticionId && 
                              r.IpHash == ipHash && 
                              r.Fecha > limiteSpam);

            if (rezoReciente)
            {
                return false; // Ya rezó recientemente
            }

            // Registrar el rezo
            var rezo = new Rezo
            {
                PeticionId = peticionId,
                IpHash = ipHash,
                Fecha = DateTime.Now
            };

            _context.Rezos.Add(rezo);

            // Incrementar el contador
            var peticion = await _context.Peticiones.FindAsync(peticionId);
            if (peticion != null)
            {
                peticion.ContadorRezos++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
