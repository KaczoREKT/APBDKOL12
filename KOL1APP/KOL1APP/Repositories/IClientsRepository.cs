using KOL1APP.DTOs;

namespace KOL1APP.Repositories
{
    public interface IClientsRepository
    {
        Task<bool> DoesClientExist(int id);
        Task<bool> DoesCarExist(int id);
        Task<ClientDTO> GetClientWithRentalsAsync(int clientId);
        
        // Version with implicit transaction
        Task AddClientWithRental(AddClientRequest newClientWithRental);

        // Version with transaction scope
        Task<int> AddClient(ClientRequest client);
        Task AddCarRental(int clientId, CarRentalDTO carRental);
        
        Task<int> GetCarPricePerDay(int carId);
    }
}