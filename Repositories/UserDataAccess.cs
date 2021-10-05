using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ServicesInterfaces.DataAccess;
using ServicesInterfaces.Global;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class UserDataAccess : IUserDataAccess
    {
        private readonly IMongoCollection<UserCredentials> _userCredentials;
        private readonly ILogger<UserDataAccess> _logger;
        public UserDataAccess(IAppSettings settings, ILogger<UserDataAccess> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _userCredentials = database.GetCollection<UserCredentials>(settings.UserCredentialsCollectionName);
            _logger = logger;
        }
        public async Task<UserCredentials> AuthenticateUser(Data data)
        {
            try
            {
                var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Username == data.Username & ym.Password == data.Password);

                var user = await _userCredentials.Find(filter).SingleOrDefaultAsync();
                return user;

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return null;
            }

        }

        public Task<ServiceSessions> CheckForServiceSession(Data data)
        {
            throw new NotImplementedException();
        }

        public async Task<UserCredentials> CheckIfUsernameExists(Data data)
        {

            try
            {
                var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Username == data.Username);

                var user = await _userCredentials.Find(filter).SingleOrDefaultAsync();
                return user;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }

        }

        public Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
        {
            throw new NotImplementedException();
        }

        public async Task<UserCredentials> GetUserById(Data data)
        {

            try
            {
                var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Id == data.Id);

                var user = await _userCredentials.Find(filter).SingleOrDefaultAsync();
                return user;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }

        }

        public Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateResult> RegisterService(Data data)
        {
            throw new NotImplementedException();
        }

        public async Task<UserCredentials> RegisterUser(Data data)
        {
            try
            {
                UserCredentials user = new UserCredentials()
                {
                    Username = data.Username,
                    Password = data.Password,
                    Name = data.Name,
                    Age = data.Age,
                    Services = new List<UserServiceCredentials>()
                };

                await _userCredentials.InsertOneAsync(user);
                return user;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }
        }

        public Task<DeleteResult> RemoveServiceFromUser(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceSessions> UpdateServiceSession(Data data)
        {
            throw new NotImplementedException();
        }

        public async Task<UpdateResult> UpdateUser(Data data)
        {
            try
            {
                var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Id == data.Id);

                var update1 = Builders<UserCredentials>.Update.Set(a => a.Name, data.Name);
                await _userCredentials.UpdateOneAsync(filter, update1);

                var update2 = Builders<UserCredentials>.Update.Set(a => a.About, data.About);
                await _userCredentials.UpdateOneAsync(filter, update2);

                var update3 = Builders<UserCredentials>.Update.Set(a => a.Age, data.Age);
                await _userCredentials.UpdateOneAsync(filter, update3);

                var update4 = Builders<UserCredentials>.Update.Set(a => a.SeenTutorial, data.SeenTutorial);
                return await _userCredentials.UpdateOneAsync(filter, update4);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }
        }
    }
}
