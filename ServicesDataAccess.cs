using BadooAPI.Utills;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQScheduler.Models;
using ServicesInterfaces;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ServicesDataAccess : IDataAccess
    {
        private readonly IMongoCollection<UserCredentials> _userCredentials;
        private readonly IMongoCollection<ServiceSessions> _serviceSessions;
        public ServicesDataAccess(IAutoLoverDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _userCredentials = database.GetCollection<UserCredentials>(settings.UserCredentialsCollectionName);
            _serviceSessions = database.GetCollection<ServiceSessions>(settings.ServiceSessionsCollectionName);
        }
        public async Task<UpdateResult> RegisterService(Data data)
        {
            var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));
            ///hash password before insert
            UserServiceCredentials userServiceCredentials = new UserServiceCredentials()
            {
                UserServiceId = data.UserServiceId,
                Username = data.UserName,
                Password = data.Password,
                Service = data.Service,
                Hash = ""
            };

            var result = await _userCredentials.UpdateOneAsync(filter,
                  Builders<UserCredentials>.Update.AddToSet(u => u.Services, userServiceCredentials));
            if (result.IsAcknowledged)
            {
                await UpdateServiceSession(data);
            }

            return result;
        }

        public async Task<ServiceSessions> CheckForServiceSession(Data data)
        {
            try
            {
                var filter = Builders<ServiceSessions>.Filter.Eq("SerivceId", data.UserServiceId);

                return await (await _serviceSessions.FindAsync(filter)).FirstAsync();
            }
            catch (Exception)
            {

                return default(ServiceSessions);
            }
        }

        public async Task<ServiceSessions> UpdateServiceSession(Data data)
        {
            var today = DateTime.Today;
            ServiceSessions session = new ServiceSessions()
            {
                Id = data.Id,
                SerivceId = data.UserServiceId,
                SessionId = data.SessionId,
                HiddenUrl = data.HiddenUrl,
                XPing = data.XPing,
                ExpireAt = today.AddDays(1)
            };

            await _serviceSessions.InsertOneAsync(session);
            return session;

        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
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

        public async Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
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

        public async Task<DeleteResult> RemoveServiceFromUser(Data data)
        {
            // var userCredentialsCollection = _database.GetCollection<UserCredentials>("UserCredentials");
            var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Id == data.Id);
            var update = Builders<UserCredentials>.Update.PullFilter(ym => ym.Services, Builders<UserServiceCredentials>.Filter.Where(nm => nm.Service == data.Service));
            await _userCredentials.UpdateOneAsync(filter, update);

            //var userServiceCredentialsCollection = _database.GetCollection<ServiceSessions>("test");
            var filterService = Builders<ServiceSessions>.Filter.Where(ym => ym.Id == data.Id && ym.SessionId == data.SessionId);

            return await _serviceSessions.DeleteOneAsync(filterService);

        }

        public async Task<UserCredentials> AuthenticateUser(Data data)
        {
            try
            {
                var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Username == data.UserName & ym.Password == data.Password);

                var user = await _userCredentials.Find(filter).SingleOrDefaultAsync();
                return user;

            }
            catch (Exception)
            {

                return null;
            }
            //  var userServiceCredentialsCollection = _database.GetCollection<UserCredentials>("UserCredentials");

        }
        public async Task<UserCredentials> CheckIfUsernameExists(Data data)
        {

            //    var userServiceCredentialsCollection = _database.GetCollection<UserCredentials>("UserCredentials");
            var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Username == data.UserName);

            var user = await _userCredentials.Find(filter).SingleOrDefaultAsync();
            return user;

        }
        public async Task<UserCredentials> GetUserById(Data data)
        {

            //    var userServiceCredentialsCollection = _database.GetCollection<UserCredentials>("UserCredentials");
            var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Id == data.Id);

            var user = await _userCredentials.Find(filter).SingleOrDefaultAsync();
            return user;

        }
        public async Task<UserCredentials> RegisterUser(Data data)
        {
            // var collection = _database.GetCollection<UserCredentials>("UserCredentials");

            UserCredentials user = new UserCredentials()
            {
                Username = data.UserName,
                Password = data.Password,
                Services = new List<UserServiceCredentials>()
            };

            await _userCredentials.InsertOneAsync(user);
            return user;
        }
        public async Task<UserCredentials> UpdateUser(Data data)
        {
            UserCredentials user = new UserCredentials()
            {
                Id = data.Id,
                Username = data.UserName,
                Password = data.Password,
                SeenTutorial = data.SeenTutorial,
                Services = data.Services
            };
            await _userCredentials.ReplaceOneAsync(doc => doc.Id == user.Id, user);

            return user;
        }

    }
}
