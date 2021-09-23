using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ICacheDataAccess
    {
        public Task<ServiceSessions> GetServiceSession(Data data);
        public Task SetServiceSession(Data data, ServiceSessions session);
        public Task<List<UserServiceCredentials>> GetUserServices(Data data);
        public Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data);
        public Task SetUserServices(Data data, List<UserServiceCredentials> services);
        public Task<IDictionary<string, string>> GetUserImages(Data data);
        public Task SetUserImages(Data data, IDictionary<string, string> images);
        public Task RemoveService(Data data);
        public Task RemoveAndSetUserImages(Data data, IDictionary<string, string> images);
        public Task<UserCredentials> GetUserById(Data data);
        public Task SetUserById(Data data, UserCredentials user);
        public Task UpdateUserById(Data data);
    }
}
