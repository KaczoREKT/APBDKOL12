using KOL1APP.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace KOL1APP.Repositories
{
    public class ClientsRepository : IClientsRepository
    {
        private readonly IConfiguration _configuration;

        public ClientsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> DoesClientExist(int id)
        {
            var query = "SELECT 1 FROM Client WHERE ID = @ID";
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();
            var res = await command.ExecuteScalarAsync();
            return res != null;
        }

        public async Task<bool> DoesCarExist(int id)
        {
            var query = "SELECT 1 FROM Car WHERE ID = @ID";
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();
            var res = await command.ExecuteScalarAsync();
            return res != null;
        }

        public async Task<ClientDTO> GetClientWithRentalsAsync(int clientId)
        {
            var query = @"SELECT 
                            Client.ID AS ClientID,
                            Client.FirstName,
                            Client.LastName,
                            Client.Address,
                            CarRental.ID AS CarRentalID,
                            CarRental.CarID,
                            CarRental.DateFrom,
                            CarRental.DateTo,
                            CarRental.TotalPrice,
                            CarRental.Discount,
                            Car.ID AS CarID,
                            Car.VIN,
                            Car.Name AS CarName,
                            Car.Seats,
                            Car.PricePerDay,
                            Car.ModelID,
                            Car.ColorID,
                            Model.ID AS ModelID,
                            Model.Name AS ModelName,
                            Color.ID AS ColorID,
                            Color.Name AS ColorName
                          FROM Client
                          JOIN CarRental ON CarRental.ClientID = Client.ID
                          JOIN Car ON Car.ID = CarRental.CarID
                          JOIN Model ON Model.ID = Car.ModelID
                          JOIN Color ON Color.ID = Car.ColorID
                          WHERE Client.ID = @ID";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", clientId);

            await connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync();

            ClientDTO clientDto = null;

            while (await reader.ReadAsync())
            {
                if (clientDto == null)
                {
                    clientDto = new ClientDTO
                    {
                        Id = reader.GetInt32("ClientID"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        Address = reader.GetString("Address"),
                        Rentals = new List<CarRentalDTO>()
                    };
                }

                clientDto.Rentals.Add(new CarRentalDTO
                {
                    Id = reader.GetInt32("CarRentalID"),
                    ClientId = clientId,
                    CarId = reader.GetInt32("CarID"),
                    DateFrom = reader.GetDateTime("DateFrom"),
                    DateTo = reader.GetDateTime("DateTo"),
                    TotalPrice = reader.GetInt32("TotalPrice"),
                    Discount = reader.GetInt32("Discount"),
                    Car = new CarDTO
                    {
                        Id = reader.GetInt32("CarID"),
                        VIN = reader.GetString("VIN"),
                        Name = reader.GetString("CarName"),
                        Seats = reader.GetInt32("Seats"),
                        PricePerDay = reader.GetInt32("PricePerDay"),
                        ModelId = reader.GetInt32("ModelID"),
                        ColorId = reader.GetInt32("ColorID"),
                        Model = new ModelDTO
                        {
                            Id = reader.GetInt32("ModelID"),
                            Name = reader.GetString("ModelName")
                        },
                        Color = new ColorDTO
                        {
                            Id = reader.GetInt32("ColorID"),
                            Name = reader.GetString("ColorName")
                        }
                    }
                });
            }

            return clientDto;
        }

        public async Task<int> AddClient(ClientRequest client)
        {
            var query = "INSERT INTO Client (FirstName, LastName, Address) OUTPUT INSERTED.ID VALUES (@FirstName, @LastName, @Address)";
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@Address", client.Address);

            await connection.OpenAsync();
            var id = await command.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }

        public async Task AddCarRental(int clientId, CarRentalDTO carRental)
        {
            var query = "INSERT INTO CarRental (ClientID, CarID, DateFrom, DateTo, TotalPrice, Discount) VALUES (@ClientID, @CarID, @DateFrom, @DateTo, @TotalPrice, @Discount)";
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClientID", clientId);
            command.Parameters.AddWithValue("@CarID", carRental.CarId);
            command.Parameters.AddWithValue("@DateFrom", carRental.DateFrom);
            command.Parameters.AddWithValue("@DateTo", carRental.DateTo);
            command.Parameters.AddWithValue("@TotalPrice", carRental.TotalPrice);
            command.Parameters.AddWithValue("@Discount", carRental.Discount);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task AddClientWithRental(AddClientRequest newClientWithRental)
{
    var insertClientQuery = @"INSERT INTO Client (FirstName, LastName, Address) 
                              VALUES (@FirstName, @LastName, @Address);
                              SELECT SCOPE_IDENTITY();";

    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
    await using SqlCommand command = new SqlCommand(insertClientQuery, connection);
    command.Parameters.AddWithValue("@FirstName", newClientWithRental.Client.FirstName);
    command.Parameters.AddWithValue("@LastName", newClientWithRental.Client.LastName);
    command.Parameters.AddWithValue("@Address", newClientWithRental.Client.Address);

    await connection.OpenAsync();

    var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
    command.Transaction = transaction;

    try
    {
        var clientId = Convert.ToInt32(await command.ExecuteScalarAsync());

        command.Parameters.Clear();
        command.CommandText = "INSERT INTO CarRental (ClientID, CarID, DateFrom, DateTo, TotalPrice, Discount) VALUES (@ClientID, @CarID, @DateFrom, @DateTo, @TotalPrice, @Discount)";
        command.Parameters.AddWithValue("@ClientID", clientId);
        command.Parameters.AddWithValue("@CarID", newClientWithRental.CarId);
        command.Parameters.AddWithValue("@DateFrom", newClientWithRental.DateFrom);
        command.Parameters.AddWithValue("@DateTo", newClientWithRental.DateTo);

        var rentalDays = (newClientWithRental.DateTo - newClientWithRental.DateFrom).Days;
        var totalPrice = rentalDays * (await GetCarPricePerDay(newClientWithRental.CarId));
        command.Parameters.AddWithValue("@TotalPrice", totalPrice);
        command.Parameters.AddWithValue("@Discount", 0); // Assuming no discount for simplicity

        await command.ExecuteNonQueryAsync();

        await transaction.CommitAsync();
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}

        public async Task<int> GetCarPricePerDay(int carId)
        {
            var query = "SELECT PricePerDay FROM Car WHERE ID = @ID";
            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", carId);

            await connection.OpenAsync();
            var price = await command.ExecuteScalarAsync();
            return Convert.ToInt32(price);
        }
    }
}
