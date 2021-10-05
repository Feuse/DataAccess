using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ServicesInterfaces.DataAccess.Cache;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess.Cache
{
    public class ServicesCacheAccess : IServiceCacheAccess
    {
        private readonly DistributedCacheEntryOptions _options;
        private readonly ILogger<ServicesCacheAccess> _logger;
        private readonly IDistributedCache _distributedCache;
        public ServicesCacheAccess(IDistributedCache distributedCache, ILogger<ServicesCacheAccess> logger, DistributedCacheEntryOptions options)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _options = options;
        }
        public async Task<ServiceSessions> GetServiceSession(Data data)
        {
            try
            {
                try
                {
                    var jsonData = await _distributedCache.GetStringAsync($"serviceSession|{data.UserServiceId}");
                    if (jsonData is null)
                    {
                        return default(ServiceSessions);
                    }
                    return JsonSerializer.Deserialize<ServiceSessions>(jsonData);
                }
                catch (Exception e) { throw; }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }
        }
        public async Task SetServiceSession(Data data, ServiceSessions session)
        {
            try
            {
                try
                {
                    var jsonData = JsonSerializer.Serialize(session);
                    await _distributedCache.SetStringAsync($"serviceSession|{data.UserServiceId}", jsonData, _options);
                }
                catch (Exception e) { throw; }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }

        public async Task<List<UserServiceCredentials>> GetUserServices(Data data)
        {
            try
            {
                try
                {
                    var jsonData = await _distributedCache.GetStringAsync($"userServices|{data.Id}");
                    if (jsonData is null)
                    {
                        return default(List<UserServiceCredentials>);
                    }
                    return JsonSerializer.Deserialize<List<UserServiceCredentials>>(jsonData);
                }
                catch (Exception e) { throw; }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }
        }

        public async Task SetUserServices(Data data, List<UserServiceCredentials> services)
        {
            try
            {
                try
                {
                    var jsonData = JsonSerializer.Serialize(services);
                    await _distributedCache.SetStringAsync($"userServices|{data.Id}", jsonData, _options);
                    foreach (var singleService in services)
                    {
                        var jsonService = JsonSerializer.Serialize(singleService);
                        await _distributedCache.SetStringAsync($"userService|{data.Id}|{singleService.Service}", jsonService);
                    }
                }
                catch (Exception e) { throw; }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }

        public async Task RemoveService(Data data)
        {
            try
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
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }

        private async Task RemoveAndSetFoundService(Data data, List<UserServiceCredentials> userServices, UserServiceCredentials service)
        {
            try
            {
                userServices.Remove(service);
                await _distributedCache.RemoveAsync($"userServices|{data.Id}");
                await SetUserServices(data, userServices);

                await RemoveFoundServiceSession(data, service);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }

        private async Task RemoveFoundServiceSession(Data data, UserServiceCredentials service)
        {
            try
            {
                var jsonSessionsData = await _distributedCache.GetStringAsync($"serviceSession|{service.UserServiceId}");
                if (jsonSessionsData is not null)
                {
                    var serviceSessions = JsonSerializer.Deserialize<ServiceSessions>(jsonSessionsData);

                    await _distributedCache.RemoveAsync($"serviceSession|{service.UserServiceId}");
                    await SetServiceSession(data, serviceSessions);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }

        public async Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data)
        {
            try
            {
                var jsonData = await _distributedCache.GetStringAsync($"userService|{data.Id}|{data.Service}");
                if (jsonData is null)
                {
                    return default(UserServiceCredentials);
                }
                return JsonSerializer.Deserialize<UserServiceCredentials>(jsonData);
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
