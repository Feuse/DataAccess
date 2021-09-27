using AutoMapper;
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
    public class DataAccessManager : IDataAccessManager
    {
        private readonly IMapper _mapper;
        private readonly IDataAccess _servicesDataAccess;
        private readonly ICacheDataAccess _cacheDataAccess;
        public DataAccessManager(IDataAccess servicesDataAccess, ICacheDataAccess cacheDataAccess, IMapper mapper)
        {
            _servicesDataAccess = servicesDataAccess;
            _cacheDataAccess = cacheDataAccess;
            _mapper = mapper;
        }
        public async Task<UserCredentials> GetUserById(Data data)
        {
            var cachedUser = await _cacheDataAccess.GetUserById(data);
            if (cachedUser is not null)
            {
                return cachedUser;
            }

            var dbUser = await _servicesDataAccess.GetUserById(data);
            if (dbUser is not null)
            {
                await _cacheDataAccess.SetUserById(data, dbUser);
                return dbUser;
            }

            return default;
        }
        public async Task<UserCredentials> AuthenticateUser(Data data)
        {
            return await _servicesDataAccess.AuthenticateUser(data);
        }
        //get service session
        public async Task<ServiceSessions> GetServiceSession(Data data)
        {
            var session = await _cacheDataAccess.GetServiceSession(data);
            if (session is null)
            {
                session = await _servicesDataAccess.CheckForServiceSession(data);
                if (session is null)
                {
                    return session;
                }
                await _cacheDataAccess.SetServiceSession(data, session);
            }
            return session;
        }

        public async Task<UserCredentials> CheckIfUsernameExists(Data data)
        {
            return await _servicesDataAccess.CheckIfUsernameExists(data);
        }
        //get user services
        public async Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
        {
            var services = await _cacheDataAccess.GetUserServices(data);
            if (services is null)
            {
                services = await _servicesDataAccess.GetAllUserServicesById(data);
                if (services.Count ==0)
                {
                    return services;
                }
                await _cacheDataAccess.SetUserServices(data, services);
            }
            return services;
        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            var userService = await _cacheDataAccess.GetUserServiceByServiceNameAndId(data);
            if (userService is null)
            {
                return await _servicesDataAccess.GetUserServiceByServiceNameAndId(data);
            }
            return userService;
        }

        public async Task<UpdateResult> RegisterService(Data data)
        {
            return await _servicesDataAccess.RegisterService(data);
        }

        public async Task<UserCredentials> RegisterUser(Data data)
        {
            return await _servicesDataAccess.RegisterUser(data);
        }

        public async Task<DeleteResult> RemoveServiceFromUser(Data data)
        {
            await _cacheDataAccess.RemoveService(data);
            return await _servicesDataAccess.RemoveServiceFromUser(data);
        }

        public async Task UpdateServiceSession(Data data)
        {
            var session = await _cacheDataAccess.GetServiceSession(data);
            if (session is null)
            {
                session = await _servicesDataAccess.UpdateServiceSession(data);
                if (session is not null)
                {
                    await _cacheDataAccess.SetServiceSession(data, session);
                }
            }
        }

        public async Task<IDictionary<string, string>> GetUserImages(Data data)
        {
            return await _cacheDataAccess.GetUserImages(data);
        }

        public async Task SetUserImages(Data data, IDictionary<string, string> images)
        {
            await _cacheDataAccess.SetUserImages(data, images);
        }
        public async Task RemoveUserImage(Data data, IDictionary<string, string> images)
        {
            await _cacheDataAccess.RemoveAndSetUserImages(data, images);
        }

        public async Task UpdateUser(Data data)
        {
            var user = await GetUserById(data);

            data = _mapper.Map(user, data);

            await _cacheDataAccess.UpdateUserById(data);
            await _servicesDataAccess.UpdateUser(data);
        }
    }
}
