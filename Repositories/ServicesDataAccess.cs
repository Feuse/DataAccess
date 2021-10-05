using BadooAPI.Utills;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using ServicesInterfaces;
using ServicesInterfaces.DataAccess;
using ServicesInterfaces.Global;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public partial class ServicesDataAccess : IServiceDataAccess
    {
        private readonly IMongoCollection<UserCredentials> _userCredentials;
        private readonly IMongoCollection<ServiceSessions> _serviceSessions;
        private readonly ILogger<ServicesDataAccess> _logger;
        private readonly IUserDataAccess _userDataAccess;

        public ServicesDataAccess(IAppSettings settings, ILogger<ServicesDataAccess> logger, IUserDataAccess userDataAccess)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _userCredentials = database.GetCollection<UserCredentials>(settings.UserCredentialsCollectionName);
            _serviceSessions = database.GetCollection<ServiceSessions>(settings.ServiceSessionsCollectionName);
            _logger = logger;
            _userDataAccess = userDataAccess;
        }

        public async Task<UpdateResult> RegisterService(Data data)
        {
            try
            {
                var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));
                ///hash password before insert
                UserServiceCredentials userServiceCredentials = new UserServiceCredentials()
                {
                    UserServiceId = data.UserServiceId,
                    Username = data.Username,
                    Password = data.Password,
                    Service = data.Service,
                    Hash = "",
                    Premium = data.Premium
                };

                var result = await _userCredentials.UpdateOneAsync(filter,
                      Builders<UserCredentials>.Update.AddToSet(u => u.Services, userServiceCredentials));
                if (result.IsAcknowledged)
                {
                    await UpdateServiceSession(data);
                }

                //data.Services.Add(userServiceCredentials);
                await _userDataAccess.UpdateUser(data);


                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }
        }

        public async Task<ServiceSessions> CheckForServiceSession(Data data)
        {
            try
            {
                var filter = Builders<ServiceSessions>.Filter.Eq("SerivceId", data.UserServiceId);

                return await (await _serviceSessions.FindAsync(filter)).FirstAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default(ServiceSessions);
            }
        }

        public async Task<ServiceSessions> UpdateServiceSession(Data data)
        {
            try
            {
                var today = DateTime.Today;
                ServiceSessions session = new ServiceSessions()
                {
                    Id = data.Id,
                    SerivceId = data.UserServiceId,
                    SessionId = data.SessionId,
                    HiddenUrl = data.HiddenUrl,
                    XPing = data.XPing,
                    ExpireAt = today
                };

                await _serviceSessions.InsertOneAsync(session);

                var indexKeysDefinition = Builders<ServiceSessions>.IndexKeys.Ascending("expireAt");
                var indexOptions = new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 2, 0) };
                var indexModel = new CreateIndexModel<ServiceSessions>(indexKeysDefinition, indexOptions);
                _serviceSessions.Indexes.CreateOne(indexModel);
                return session;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }

        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            try
            {
                var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));

                var collection = await _userCredentials.Find(filter).SingleOrDefaultAsync();

                if (collection is not null)
                {
                    var service = collection.Services.Where(a => a.Service == data.Service).First();
                    return service;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return null;
            }
        }

        public async Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
        {
            try
            {
                var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));
                // var userServiceCredentialsCollection = _database.GetCollection<UserCredentials>("UserCredentials");
                var collection = await _userCredentials.Find(filter).SingleOrDefaultAsync();

                if (collection is not null)
                {
                    var services = collection.Services.ToList();
                    return services;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return null;
            }

        }

        public async Task<DeleteResult> RemoveServiceFromUser(Data data)
        {
            try
            {
                // var userCredentialsCollection = _database.GetCollection<UserCredentials>("UserCredentials");
                var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Id == data.Id);
                var update = Builders<UserCredentials>.Update.PullFilter(ym => ym.Services, Builders<UserServiceCredentials>.Filter.Where(nm => nm.Service == data.Service));
                await _userCredentials.UpdateOneAsync(filter, update);

                //var userServiceCredentialsCollection = _database.GetCollection<ServiceSessions>("test");
                var filterService = Builders<ServiceSessions>.Filter.Where(ym => ym.Id == data.Id && ym.SessionId == data.SessionId);

                return await _serviceSessions.DeleteOneAsync(filterService);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }

        }
        public async Task<UpdateResult> UpdateService(Data data)
        {
            var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));

            var collection = await _userCredentials.Find(filter).SingleOrDefaultAsync();

            if (collection is not null)
            {
                var service = collection.Services.Where(a => a.Service == data.Service).First();
                var filter3 = Builders<UserCredentials>.Filter.Where(x => x.Id == data.Id&& x.Services.Any(i => i.UserServiceId == service.UserServiceId));
                var arrayFilter = Builders<UserCredentials>.Update.Set(x => service.Premium, data.Premium);
                var update = Builders<UserCredentials>.Update.Set(x => x.Services[-1].Premium, data.Premium);
               return await _userCredentials.UpdateOneAsync(filter3, update);
            }
            return default;
        }
        public Task<UserCredentials> GetUserById(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UserCredentials> AuthenticateUser(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UserCredentials> RegisterUser(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UserCredentials> CheckIfUsernameExists(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateResult> UpdateUser(Data data)
        {
            throw new NotImplementedException();
        }


    }
}
