using System;
using System.Collections;
using System.Collections.Generic;

using System.Web.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UUFile.Controllers;
using Ionic.Zip;

namespace UUFile.Infrastructure
{
    public static class Helper
    {

        public static void BeginLogSection(this StringBuilder sb, string info)
        {
            sb.AppendLine();
            sb.AppendLine(info);
        }

        public static bool NoFiles(this HttpRequestBase request)
        {
            bool empty = true;
            for (int i = 0; i < request.Files.Count; i++)
            {
                if (request.Files[i].ContentLength > 0)
                {
                    empty = false;
                    break;
                }
            }
            return empty;
        }

        public static string GetAppSetting(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }

        public static void SetAppSetting(string key, string value)
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings[key].Value = value;
            config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            //WebConfigurationManager.RefreshSection("appSettings");
        }


        public static List<string> Decompress(FileInfo fi, string path)
        {
            List<string> logs = new List<string>();
            string strzipPath = string.Format(@"""{0}""", fi.FullName);
            System.Diagnostics.Process Process1 = new System.Diagnostics.Process();
            Process1.StartInfo.FileName = "Winrar.exe";
            Process1.StartInfo.CreateNoWindow = true;

            //x 幺妹.rar f:\\幺妹 -y
            Process1.StartInfo.Arguments = " x  -ibck -inul " + strzipPath + " " + string.Format(@"""{0}"" -y", path);
            Process1.Start();
            Process1.WaitForExit();
            if (Process1.HasExited)
            {
                int iExitCode = Process1.ExitCode;
                if (iExitCode == 0)
                {
                    //正常完成
                    logs.Add("    解压：" + fi.Name + "成功");
                }
                else
                {
                    //有错
                    logs.Add("    解压：" + fi.Name + "失败     **************");
                }
            }
            Process1.Close();
            return logs;
        }
    }

    public static class ConfigInfo
    {
        public static string ExpiryMonth
        {
            get
            {
                return Helper.GetAppSetting("ExpiryMonth");
            }
        }

        public static string FtpDLLPath
        {
            get
            {
                return Helper.GetAppSetting("FtpDLLPath");
            }
        }


        public static string WorkSpace
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + Helper.GetAppSetting("WorkSpace") + "\\";
            }
        }


        //public static string CurUploadPath
        //{
        //    get
        //    {
        //        return WorkSpace + DateTime.Now.ToString("yyyyMMddhhmmss") + "\\";
        //    }
        //}

        //public static string CurFtpBakPath
        //{
        //    get
        //    {
        //        return CurUploadPath + "FTP备份\\";
        //    }
        //}


        public static string ClientIniPath
        {
            get
            {
                return Helper.GetAppSetting("ClientIniPath");
            }
        }


        public static string ServerIniPath
        {
            get
            {
                return Helper.GetAppSetting("ServerIniPath");
            }
        }


        public static string UserName
        {
            get
            {
                return Helper.GetAppSetting("UserName");
            }
        }

        public static string Password
        {
            get
            {
                return Helper.GetAppSetting("Password");
            }
        }

        public static string Token
        {
            get
            {
                return Helper.GetAppSetting("Token");
            }
            set
            {
                Helper.SetAppSetting("Token", value);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class StepInfoAttribute : Attribute
    {
        public StepInfoAttribute(string desc)
        {
            Desc = desc;
        }

        public string Desc { get; private set; }

    }



    internal sealed class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //            base.OnActionExecuting(filterContext);
            var controller = (ApplicationController)filterContext.Controller;

            //user logged in?
            //if (!controller.IsLoggedIn)
            //{
            //    if (filterContext.RequestContext.HttpContext.Request.ContentType == "application/json")
            //        filterContext.Result = new JsonResult() { Data = "Unauthorized" };
            //    else
            //        filterContext.Result = new HttpUnauthorizedResult();
            //    return;
            //}

            ////is the user an admin?
            //var adminEmails = new string[] { "lazycrazy@live.cn" };
            //string userEmail = controller.CurrentUser.Email;
            //if (!adminEmails.Contains(userEmail))
            //{
            //    //DecideResponse(filterContext.HttpContext);
            //    if (filterContext.RequestContext.HttpContext.Request.ContentType == "application/json")
            //        filterContext.Result = new JsonResult() { Data = "Unauthorized" };
            //    else
            //        filterContext.Result = new HttpUnauthorizedResult();
            //    //filterContext.Result = new RedirectResult("/account/logon");

            //    return;
            //}
        }
    }
}

