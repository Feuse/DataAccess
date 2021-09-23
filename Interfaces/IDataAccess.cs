using Microsoft.AspNetCore.Mvc;
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
        public Task<ServiceSessions> UpdateServiceSession(Data data);
        public Task<ServiceSessions> CheckForServiceSession(Data data);
        public Task<UserServiceCredentials> GetUserServiceByServiceNameAndId(Data data);
        public Task<List<UserServiceCredentials>> GetAllUserServicesById(Data data);
        public Task<DeleteResult> RemoveServiceFromUser(Data data);
        public Task<UserCredentials> RegisterUser(Data data);
        public Task<UserCredentials> AuthenticateUser(Data data);
        public Task<UserCredentials> CheckIfUsernameExists(Data data);
        public Task<UserCredentials> GetUserById(Data data);
        public Task<UserCredentials> UpdateUser(Data data);
    }
}
