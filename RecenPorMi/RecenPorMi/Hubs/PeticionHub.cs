using Microsoft.AspNetCore.SignalR;

namespace RecenPorMi.Hubs
{
    public class PeticionHub : Hub
    {
        public async Task NotificarNuevaPeticion()
        {
            await Clients.All.SendAsync("NuevaPeticionPublicada");
        }

        public async Task NotificarRezoActualizado(int peticionId, int nuevoContador)
        {
            await Clients.All.SendAsync("RezoActualizado", peticionId, nuevoContador);
        }
    }
}
