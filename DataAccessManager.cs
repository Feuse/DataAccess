using AutoMapper;
using MongoDB.Driver;
using ServicesInterfaces;
using ServicesInterfaces.DataAccess;
using ServicesInterfaces.DataAccess.Cache;
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
        private readonly IUserDataAccess _userDataAccess;
        private readonly IServiceDataAccess _serviceDataAccess;
        private readonly IServiceCacheAccess _serviceCacheAccess;
        private readonly IUserCacheAccess _userCacheAccess;
        public DataAccessManager(IMapper mapper, IUserDataAccess userDataAccess, IServiceDataAccess serviceDataAccess, IUserCacheAccess userCacheAccess, IServiceCacheAccess serviceCacheAccess)
        {
            _mapper = mapper;
            _userDataAccess = userDataAccess;
            _serviceDataAccess = serviceDataAccess;
            _userCacheAccess = userCacheAccess;
            _serviceCacheAccess = serviceCacheAccess;
        }

        public async void UpdateService(Data data)
        {
            var session = await _serviceDataAccess.UpdateService(data);
            if (session is not null)
            {
               // await _serviceCacheAccess.SetServiceSession(data, session);
            }

        }
        public async Task<UserCredentials> GetUserById(Data data)
        {
            var cachedUser = await _userCacheAccess.GetUserById(data);
            if (cachedUser is not null)
            {
                return cachedUser;
            }

            var dbUser = await _userDataAccess.GetUserById(data);
            if (dbUser is not null)
            {
                await _userCacheAccess.SetUserById(data, dbUser);
                return dbUser;
            }

            return default;
        }
        public async Task<UserCredentials> AuthenticateUser(Data data)
        {
            return await _userDataAccess.AuthenticateUser(data);
        }
        //get service session
        public async Task<ServiceSessions> GetServiceSession(Data data)
        {
            var session = await _serviceCacheAccess.GetServiceSession(data);
            if (session is null)
            {
                session = await _serviceDataAccess.CheckForServiceSession(data);
                if (session is null)
                {
                    return session;
                }
                await _serviceCacheAccess.SetServiceSession(data, session);
            }
            return session;
        }

        public async Task<UserCredentials> CheckIfUsernameExists(Data data)
        {
            return await _userDataAccess.CheckIfUsernameExists(data);
        }
        //get user services
        public async Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data)
        {
            var services = await _serviceCacheAccess.GetUserServices(data);
            if (services is null)
            {
                services = await _serviceDataAccess.GetAllUserServicesById(data);
                if (services.Count == 0)
                {
                    return services;
                }
                await _serviceCacheAccess.SetUserServices(data, services);
            }
            return services;
        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            var userService = await _serviceCacheAccess.GetUserServiceByServiceNameAndId(data);
            if (userService is null)
            {
                return await _serviceDataAccess.GetUserServiceByServiceNameAndId(data);
            }
            return userService;
        }

        public async Task<UpdateResult> RegisterService(Data data)
        {
            await _userCacheAccess.UpdateUserById(data);
            return await _serviceDataAccess.RegisterService(data);

        }

        public async Task<UserCredentials> RegisterUser(Data data)
        {
            return await _userDataAccess.RegisterUser(data);
        }

        public async Task<DeleteResult> RemoveServiceFromUser(Data data)
        {
            await _serviceCacheAccess.RemoveService(data);
            return await _serviceDataAccess.RemoveServiceFromUser(data);
        }

        public async Task UpdateServiceSession(Data data)
        {
            var session = await _serviceCacheAccess.GetServiceSession(data);
            if (session is null)
            {
                session = await _serviceDataAccess.UpdateServiceSession(data);
                if (session is not null)
                {
                    await _serviceCacheAccess.SetServiceSession(data, session);
                }
            }
        }

        public async Task<IDictionary<string, string>> GetUserImages(Data data)
        {
            return await _userCacheAccess.GetUserImages(data);
        }

        public async Task SetUserImages(Data data, IDictionary<string, string> images)
        {
            await _userCacheAccess.SetUserImages(data, images);
        }
        public async Task RemoveUserImage(Data data, IDictionary<string, string> images)
        {
            await _userCacheAccess.RemoveAndSetUserImages(data, images);
        }

        public async Task UpdateUser(Data data)
        {
            await _userCacheAccess.UpdateUserById(data);
            await _userDataAccess.UpdateUser(data);
        }
    }
}
