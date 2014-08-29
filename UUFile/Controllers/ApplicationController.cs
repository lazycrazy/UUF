using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UUFile.Infrastructure;
using UUFile.Models;

namespace UUFile.Controllers
{

    public class ApplicationController : Controller
    {
        public ITokenHandler TokenStore;

        public ApplicationController(ITokenHandler tokenStore)
        {
            TokenStore = tokenStore;
            ViewBag.CurrentUser = CurrentUser ?? new LogOnModel() { UserName = "" };

        }
        public ApplicationController()
            : this(new FormsAuthTokenStore())
        {
        }
        LogOnModel _currentUser;

        public LogOnModel CurrentUser
        {
            get
            {
                var token = TokenStore.GetToken();
                if (!String.IsNullOrEmpty(token))
                {
                    _currentUser = Users.FindByToken(token);

                    if (_currentUser == null)
                    {
                        //force the current user to be logged out...
                        TokenStore.RemoveClientAccess();
                    }
                }

                //Hip to be null...
                return _currentUser;
            }
        }

        public bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }
    }
}
