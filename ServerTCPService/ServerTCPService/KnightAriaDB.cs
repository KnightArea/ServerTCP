using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ServerTCPService
{
    [Table(Name = "UserIdentifier")]
    public class UserIdentifier
    {
        public string id;
        public string Latitude;
        public string Longitude;
        public string TeamColor;
        public string TeamTitle;
        public string Name;
        public string EmailAdress;
        public string Phonenumber;
        public string UserLevel;
    }

    public partial class KnightAriaDB : DataContext
    {
        public KnightAriaDB(string connection) : base(connection) { }
        public Table<UserIdentifier> users;
    }
}
