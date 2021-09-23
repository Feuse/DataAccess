using MongoDB.Driver;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDataAccessManager
    {
        public Task<UserCredentials> GetUserById(Data data);
        public Task<UserCredentials> AuthenticateUser(Data data);
        //get service session
        public Task<ServiceSessions> GetServiceSession(Data data);
        public Task<UserCredentials> CheckIfUsernameExists(Data data);
        public Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data);
        public Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data);
        public Task<UpdateResult> RegisterService(Data data);
        public Task<UserCredentials> RegisterUser(Data data);
        public Task<DeleteResult> RemoveServiceFromUser(Data data);
        public Task UpdateServiceSession(Data data);
        public Task<IDictionary<string, string>> GetUserImages(Data data);
        public Task SetUserImages(Data data, IDictionary<string, string> images);
        public Task RemoveUserImage(Data data, IDictionary<string, string> images);
        public Task UpdateUser(Data data);
    }
}
