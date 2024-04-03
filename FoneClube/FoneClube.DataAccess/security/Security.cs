using FoneClube.Business.Commons.Entities.FoneClube.security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace FoneClube.DataAccess.security
{
    public class Security
    {
        public SecurityPassword EncryptPassword(string password)
        {
            var saltKey = "newRegister";

            var _encryptionService = new EncryptionService(new SecuritySettings
            {
                EncryptionKey = "273ece6f97dd844d"
            });
            
            var hashedPassword = _encryptionService.CreatePasswordHash(password, saltKey);
            
            return new SecurityPassword{ Password = hashedPassword, SaltKey = saltKey };
        }

    }
}

