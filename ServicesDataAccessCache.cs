using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using ServicesInterfaces;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ServicesDataAccessCache : ICacheDataAccess
    {
        private readonly DistributedCacheEntryOptions options;

        private readonly IDistributedCache _distributedCache;
        public ServicesDataAccessCache(IDistributedCache distributedCache)
        {
            options = new DistributedCacheEntryOptions();

            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
            options.SlidingExpiration = TimeSpan.FromSeconds(60);
            _distributedCache = distributedCache;
        }
        public async Task<UserCredentials> GetUserById(Data data)
        {
            var jsonData = await _distributedCache.GetStringAsync($"user|{data.Id}");

            if (jsonData is null)
            {
                return default(UserCredentials);
            }

            return JsonSerializer.Deserialize<UserCredentials>(jsonData);
        }
        public async Task SetUserById(Data data, UserCredentials user)
        {
            var jsonData = JsonSerializer.Serialize(user);

            await _distributedCache.SetStringAsync($"user|{data.Id}", jsonData, options);
        }
        public async Task UpdateUserById(Data data)
        {
            UserCredentials user = new UserCredentials()
            {
                Id = data.Id,
                Password = data.Password,
                SeenTutorial = data.SeenTutorial,
                Username = data.Username,
                Services = data.Services
               
            };
            var jsonData = JsonSerializer.Serialize(user);
            await _distributedCache.SetStringAsync($"user", jsonData, options);
        }
        public async Task<ServiceSessions> GetServiceSession(Data data)
        {
            var jsonData = await _distributedCache.GetStringAsync($"serviceSession|{data.UserServiceId}");

            if (jsonData is null)
            {
                return default(ServiceSessions);
            }

            return JsonSerializer.Deserialize<ServiceSessions>(jsonData);
        }
        public async Task SetServiceSession(Data data, ServiceSessions session)
        {
            var jsonData = JsonSerializer.Serialize(session);

            await _distributedCache.SetStringAsync($"serviceSession|{data.UserServiceId}", jsonData, options);
        }


        public async Task<List<UserServiceCredentials>> GetUserServices(Data data)
        {
            var jsonData = await _distributedCache.GetStringAsync($"userServices|{data.Id}");

            if (jsonData is null)
            {
                return default(List<UserServiceCredentials>);
            }

            return JsonSerializer.Deserialize<List<UserServiceCredentials>>(jsonData);
        }

        public async Task SetUserServices(Data data, List<UserServiceCredentials> services)
        {
            var jsonData = JsonSerializer.Serialize(services);

            await _distributedCache.SetStringAsync($"userServices|{data.Id}", jsonData, options);

            foreach (var singleService in services)
            {
                var jsonService = JsonSerializer.Serialize(singleService);
                await _distributedCache.SetStringAsync($"userService|{data.Id}|{singleService.Service}", jsonService);
            }

        }
        public async Task<IDictionary<string, string>> GetUserImages(Data data)
        {
            var jsonData = await _distributedCache.GetStringAsync($"{data.Id}|images");

            if (jsonData is null)
            {
                return default(IDictionary<string, string>);
            }

            return JsonSerializer.Deserialize<IDictionary<string, string>>(jsonData);
        }

        public async Task SetUserImages(Data data, IDictionary<string, string> images)
        {
            var jsonData = JsonSerializer.Serialize(images);
            await _distributedCache.SetStringAsync($"{data.Id}|images", jsonData, options);
        }
        public async Task RemoveAndSetUserImages(Data data, IDictionary<string, string> images)
        {
            var jsonResult = await _distributedCache.GetStringAsync($"{data.Id}|images");
            if (jsonResult is not null)
            {
                var jsonData = JsonSerializer.Serialize(images);
                await _distributedCache.RemoveAsync($"{data.Id}|images");
                await _distributedCache.SetStringAsync($"{data.Id}|images", jsonData);
            }
        }
        public async Task RemoveService(Data data)
        {
            var jsonServicesData = await _distributedCache.GetStringAsync($"userServices|{data.Id}");
            if (jsonServicesData is not null)
            {
                var userServices = JsonSerializer.Deserialize<List<UserServiceCredentials>>(jsonServicesData);

                foreach (var service in userServices)
                {
                    if (service.Service == data.Service)
                    {
                        await RemoveAndSetFoundService(data, userServices, service);
                    }
                }
            }
        }

        private async Task RemoveAndSetFoundService(Data data, List<UserServiceCredentials> userServices, UserServiceCredentials service)
        {
            userServices.Remove(service);
            await _distributedCache.RemoveAsync($"userServices|{data.Id}");
            await SetUserServices(data, userServices);

            await RemoveFoundServiceSession(data, service);
        }

        private async Task RemoveFoundServiceSession(Data data, UserServiceCredentials service)
        {
            var jsonSessionsData = await _distributedCache.GetStringAsync($"serviceSession|{service.UserServiceId}");
            if (jsonSessionsData is not null)
            {
                var serviceSessions = JsonSerializer.Deserialize<ServiceSessions>(jsonSessionsData);

                await _distributedCache.RemoveAsync($"serviceSession|{service.UserServiceId}");
                await SetServiceSession(data, serviceSessions);
            }
        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            var jsonData = await _distributedCache.GetStringAsync($"userService|{data.Id}|{data.Service}");

            if (jsonData is null)
            {
                return default(UserServiceCredentials);
            }

            return JsonSerializer.Deserialize<UserServiceCredentials>(jsonData);

        }


    }
}
