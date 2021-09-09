using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using ServicesInterfaces;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ServicesDataAccessCache : IDataAccess 
    {

        private readonly IDistributedCache _distributedCache;
        public ServicesDataAccessCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            
        }

        public async Task<ServiceSessions> CheckForServiceSession(Data data)
        {
            var result = await _distributedCache.GetStringAsync(data.UserServiceId);
            
            if (result is null)
            {
                return default;
            }

            throw new NotImplementedException();
        }

        public async Task<UserCredentials> CheckIfUsernameExists(Data data)
        {
            var result = await _distributedCache.GetStringAsync(data.UserName);
            throw new NotImplementedException();
        }

        public async Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
        {
            var result = await _distributedCache.GetStringAsync(data.UserServiceId);
            throw new NotImplementedException();
        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            var result = await _distributedCache.GetStringAsync(data.Service.ToString());
            throw new NotImplementedException();
        }
        public Task<UserCredentials> AuthenticateUser(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateResult> RegisterService(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<UserCredentials> RegisterUser(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<DeleteResult> RemoveServiceFromUser(Data data)
        {
            throw new NotImplementedException();
        }

        public Task UpdateServiceSession(Data data)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserServiceCredentials>> GetAllUserServicesById(string id)
        {
            throw new NotImplementedException();
        }
    }
}
