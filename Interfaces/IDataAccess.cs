using MongoDB.Driver;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDataAccess
    {
        public Task<UpdateResult> RegisterService(Data data);
        public Task UpdateServiceSession(Data data);
        public Task<ServiceSessions> CheckForServiceSession(Data data);
        public Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data);
        public Task<List<UserServiceCredentials>> GetAllUserServicesById(string id);
        public Task<DeleteResult> RemoveServiceFromUser(Data data);
        public Task<UserCredentials> RegisterUser(Data data);
        public Task<UserCredentials> AuthenticateUser(Data data);
        public Task<UserCredentials> CheckIfUsernameExists(Data data);
    }
}
