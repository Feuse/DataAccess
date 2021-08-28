using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQScheduler.Models;
using ServicesInterfaces;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class DataAccess : IDataAccess
    {
        //GET FROM CONFIGURATION FILE

        const string URI = "mongodb+srv://roman:1212@cluster0.voo5h.mongodb.net/myFirstDatabase?retryWrites=true&w=majority";
        const string NAME = "AUTOLOVER";

        private IMongoDatabase database;
        public DataAccess()
        {
            try
            {
                var client = new MongoClient(URI);
                database = client.GetDatabase(NAME);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public async Task<UpdateResult> RegisterService(Data data)
        {
            UpdateResult result;

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

            var userCredentials = database.GetCollection<UserCredentials>("UserCredentials");

            try
            {
                result = await userCredentials.UpdateOneAsync(filter,
                      Builders<UserCredentials>.Update.AddToSet(u => u.Services, userServiceCredentials));
            }
            catch (Exception e)
            {
                throw e;
            }

            await UpdateServiceSession(data);

            return result;

        }

        public async Task<ServiceSessions> CheckForServiceSession(Data data)
        {
            var serviceSessions = database.GetCollection<ServiceSessions>("test");
            var filter = Builders<ServiceSessions>.Filter.Eq("_id", data.UserServiceId);
            try
            {
                return await serviceSessions.Find(filter).FirstAsync();

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task UpdateServiceSession(Data data)
        {
            var serviceSessions = database.GetCollection<ServiceSessions>("test");

            ServiceSessions session = new ServiceSessions()
            {
                Id = data.UserServiceId,
                SessionId = data.SessionId,
                HiddenUrl = data.HiddenUrl,
                expireAt = DateTime.UtcNow
            };
            try
            {
                await serviceSessions.InsertOneAsync(session);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceName(Data data)
        {
            var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));
            var userServiceCredentialsCollection = database.GetCollection<UserCredentials>("UserCredentials");
            try
            {
                var collection = await userServiceCredentialsCollection.Find(filter).SingleOrDefaultAsync();
                var service = collection.Services.Where(a => a.Service == data.Service).First();
                if (service != null)
                {
                    return service;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {

                throw e;
            }


        }

        public async Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
        {
            var filter = Builders<UserCredentials>.Filter.Eq("_id", new ObjectId(data.Id));
            var userServiceCredentialsCollection = database.GetCollection<UserCredentials>("UserCredentials");
            try
            {
                var collection = await userServiceCredentialsCollection.Find(filter).SingleOrDefaultAsync();

                var services = collection.Services.ToList();

                return services;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public async Task<UpdateResult> RemoveServiceFromUser(Data data)
        {
            var userServiceCredentialsCollection = database.GetCollection<UserCredentials>("UserCredentials");
            var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Id == data.Id);
            var update = Builders<UserCredentials>.Update.PullFilter(ym => ym.Services, Builders<UserServiceCredentials>.Filter.Where(nm => nm.Service == data.Service));
            try
            {
                return await userServiceCredentialsCollection.UpdateOneAsync(filter, update);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<UserCredentials> AuthenticateUser(Data data)
        {

            var userServiceCredentialsCollection = database.GetCollection<UserCredentials>("UserCredentials");
            var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Username == data.UserName & ym.Password == data.Password);
            try
            {
                var user = await userServiceCredentialsCollection.Find(filter).SingleOrDefaultAsync();
                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<UserCredentials> CheckIfUsernameExists(Data data)
        {

            var userServiceCredentialsCollection = database.GetCollection<UserCredentials>("UserCredentials");
            var filter = Builders<UserCredentials>.Filter.Where(ym => ym.Username == data.UserName);
            try
            {
                var user = await userServiceCredentialsCollection.Find(filter).SingleOrDefaultAsync();
                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<UserCredentials> RegisterUser(Data data)
        {
            var collection = database.GetCollection<UserCredentials>("UserCredentials");

            UserCredentials user = new UserCredentials()
            {
                Username = data.UserName,
                Password = data.Password
            };

            try
            {
                await collection.InsertOneAsync(user);
                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
