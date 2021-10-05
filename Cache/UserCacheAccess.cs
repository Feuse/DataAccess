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
    public class UserCacheAccess : IUserCacheAccess
    {
        private readonly DistributedCacheEntryOptions _options;
        private readonly ILogger<UserCacheAccess> _logger;
        private readonly IDistributedCache _distributedCache;

        public UserCacheAccess(IDistributedCache distributedCache, ILogger<UserCacheAccess> logger, DistributedCacheEntryOptions options)
        {
            _distributedCache = distributedCache;        
            _logger = logger;
            _options = options;
        }
        public async Task<UserCredentials> GetUserById(Data data)
        {
            try
            {
                try
                {
                    var jsonData = await _distributedCache.GetStringAsync($"user|{data.Id}");
                    if (jsonData is null)
                    {
                        return default(UserCredentials);
                    }
                    return JsonSerializer.Deserialize<UserCredentials>(jsonData);
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
        public async Task SetUserById(Data data, UserCredentials user)
        {
            try
            {
                try
                {
                    var jsonData = JsonSerializer.Serialize(user);
                    await _distributedCache.SetStringAsync($"user|{data.Id}", jsonData, _options);
                }
                catch (Exception e) { throw; }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }
        public async Task UpdateUserById(Data data)
        {
            try
            {
                UserCredentials user = new UserCredentials()
                {
                    Id = data.Id,
                    Password = data.Password,
                    SeenTutorial = data.SeenTutorial,
                    Username = data.Username,
                    Name = data.Name,
                    Age = data.Age,
                    Services = data.Services,
                    About = data.About

                };
                var jsonData = JsonSerializer.Serialize(user);
                await _distributedCache.SetStringAsync($"user|{data.Id}", jsonData, _options);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }
        public async Task<IDictionary<string, string>> GetUserImages(Data data)
        {
            try
            {
                var jsonData = await _distributedCache.GetStringAsync($"{data.Id}|images");
                if (jsonData is null)
                {
                    return default(IDictionary<string, string>);
                }
                return JsonSerializer.Deserialize<IDictionary<string, string>>(jsonData);
            }

            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                return default;
            }
        }
        public async Task SetUserImages(Data data, IDictionary<string, string> images)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(images);
                await _distributedCache.SetStringAsync($"{data.Id}|images", jsonData, _options);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
            }
        }
        public async Task RemoveAndSetUserImages(Data data, IDictionary<string, string> images)
        {
            var jsonResult = await _distributedCache.GetStringAsync($"{data.Id}|images");
            if (jsonResult is not null)
            {
                try
                {
                    var jsonData = JsonSerializer.Serialize(images);
                    await _distributedCache.RemoveAsync($"{data.Id}|images");
                    await _distributedCache.SetStringAsync($"{data.Id}|images", jsonData);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogTrace(e.StackTrace);
                }
            }
        }

    }
}
