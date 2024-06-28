using KOL1APP.DTOs;
using KOL1APP.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace KOL1APP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsRepository _clientsRepository;

        public ClientsController(IClientsRepository clientsRepository)
        {
            _clientsRepository = clientsRepository;
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetClient(int clientId)
        {
            if (!await _clientsRepository.DoesClientExist(clientId))
                return NotFound($"Client with given ID - {clientId} doesn't exist");

            var client = await _clientsRepository.GetClientWithRentalsAsync(clientId);
            return Ok(client);
        }

        // Version with implicit transaction
        [HttpPost]
        public async Task<IActionResult> AddClient(AddClientRequest request)
        {
            if (!await _clientsRepository.DoesCarExist(request.CarId))
                return NotFound($"Car with given ID - {request.CarId} doesn't exist");

            await _clientsRepository.AddClientWithRental(request);
            return Created(Request.Path.Value ?? "api/clients", request);
        }

        // Version with transaction scope
        [HttpPost]
        [Route("with-scope")]
        public async Task<IActionResult> AddClientWithScope(AddClientRequest request)
        {
            if (!await _clientsRepository.DoesCarExist(request.CarId))
                return NotFound($"Car with given ID - {request.CarId} doesn't exist");

            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var clientId = await _clientsRepository.AddClient(request.Client);
                
                var carRental = new CarRentalDTO
                {
                    CarId = request.CarId,
                    DateFrom = request.DateFrom,
                    DateTo = request.DateTo,
                    TotalPrice = (request.DateTo - request.DateFrom).Days * (await _clientsRepository.GetCarPricePerDay(request.CarId)),
                    Discount = 0 // Assuming no discount for simplicity
                };

                await _clientsRepository.AddCarRental(clientId, carRental);

                scope.Complete();
            }

            return Created(Request.Path.Value ?? "api/clients", request);
        }
    }
}
