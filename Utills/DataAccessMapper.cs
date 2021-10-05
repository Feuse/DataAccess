using AutoMapper;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Utills
{
    public class DataAccessMapper : Profile
    {
        public DataAccessMapper()
        {
            CreateMap<UserCredentials, Data>().ForMember(x => x.About, opt => opt.Ignore());
            CreateMap<UserServiceCredentials, Data>();
            CreateMap<Data, UserCredentials>();
        }
    }
}
