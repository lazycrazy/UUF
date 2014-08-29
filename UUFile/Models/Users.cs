using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using UUFile.Infrastructure;

namespace UUFile.Models
{

    public static class Users
    {
        public static LogOnModel FindByToken(string token)
        {
            if (ConfigInfo.Token == token)
            {
                return new LogOnModel() { UserName = ConfigInfo.UserName };
            }
            return null;
        }

        public static LogOnModel Login(string username, string password)
        {
            if (username == ConfigInfo.UserName && password == ConfigInfo.Password)
            {
                return new LogOnModel() { UserName = username, Password = password };
            }
            return null;
        }

        public static void SetToken(string token, LogOnModel user)
        {
            ConfigInfo.Token = token;
        }
    }
}
