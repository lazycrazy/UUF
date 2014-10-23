using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UUFile.Infrastructure;

namespace UUFile.Controllers
{
    public class HomeController : ApplicationController
    {
        private Action[] _ProcessUpload = null;

        private Action[] ProcessUpload
        {
            get
            {
                if (_ProcessUpload == null)
                    _ProcessUpload = new Action[] { Begin, SaveUplodFiles, CopyUploadFilesToWorkSpace, UpzipUploadFilesToWorkSpace, BakFtpFiles, UpdateToFtp, AddUploadFilesVers, DeleteTempFiles, DeleteExpiryFiles, End };
                return _ProcessUpload;
            }
        }

        public ActionResult Index()
        {
            ViewBag.Message = "上传更新文件！";
            return View();
        }


        private StringBuilder log = new StringBuilder();
        private string CurUploadPath;
        private string CurFtpBakPath
        {
            get { return CurUploadPath + "FTP备份\\"; ; }
        }



        [Authorize]
        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.NoFiles())
            {
                TempData.Add("message", "请选择要上传的文件!");
                return Redirect("/");
            }
            CurUploadPath = ConfigInfo.WorkSpace + DateTime.Now.ToString("yyyyMMddHHmmss") + "\\";
            Array.ForEach(ProcessUpload, f =>
                {
                    var desc = Attribute.GetCustomAttribute(f.Method, typeof(StepInfoAttribute));
                    log.BeginLogSection((Array.IndexOf(ProcessUpload, f) + 1) + ". " + (desc as StepInfoAttribute).Desc);
                    f();
                });
            return Redirect("/");
        }


        [StepInfo("开始处理")]
        private void Begin()
        {
            if (!Directory.Exists(ConfigInfo.WorkSpace))
            {
                Directory.CreateDirectory(ConfigInfo.WorkSpace);
            }
        }

        [StepInfo("保存更新文件")]
        private void SaveUplodFiles()
        {
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase f = Request.Files[i];
                if (f == null || f.ContentLength == 0) continue;

                if (!Directory.Exists(CurUploadPath))
                    Directory.CreateDirectory(CurUploadPath);
                string filename = Path.GetFileName(f.FileName);
                f.SaveAs(Path.Combine(CurUploadPath, filename));
                log.AppendLine("    " + f.FileName);
            }
        }

        [StepInfo("复制更新文件到临时目录")]
        private void CopyUploadFilesToWorkSpace()
        {
            foreach (var file in Directory.GetFiles(CurUploadPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".dll") || s.EndsWith(".exe")))
            {
                var fileName = new FileInfo(file).Name;
                System.IO.File.Copy(file, ConfigInfo.WorkSpace + fileName, true);
                log.AppendLine("    " + fileName);
            }
        }

        [StepInfo("解压更新文件到临时目录")]
        private void UpzipUploadFilesToWorkSpace()
        {
            foreach (var file in Directory.GetFiles(CurUploadPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".zip") || s.EndsWith(".rar")))
            {
                var fileInfo = new FileInfo(file);
                log.AppendLine(string.Join("\r\n", Helper.Decompress(fileInfo, ConfigInfo.WorkSpace)));
            }
        }



        [StepInfo("备份FTP旧文件")]
        private void BakFtpFiles()
        {
            foreach (var file in Directory.GetFiles(ConfigInfo.WorkSpace))
            {
                if (!Directory.Exists(CurFtpBakPath))
                    Directory.CreateDirectory(CurFtpBakPath);
                var newFileInfo = new FileInfo(file);
                var ftpFileName = ConfigInfo.FtpDLLPath + newFileInfo.Name;
                if (System.IO.File.Exists(ftpFileName))
                    System.IO.File.Copy(ftpFileName, CurFtpBakPath + newFileInfo.Name, true);
                log.AppendLine("    " + newFileInfo.Name);
            }
            if (System.IO.File.Exists(ConfigInfo.ClientIniPath))
                System.IO.File.Copy(ConfigInfo.ClientIniPath, CurFtpBakPath + new FileInfo(ConfigInfo.ClientIniPath).Name, true);
            if (System.IO.File.Exists(ConfigInfo.ServerIniPath))
                System.IO.File.Copy(ConfigInfo.ServerIniPath, CurFtpBakPath + new FileInfo(ConfigInfo.ServerIniPath).Name, true);
        }

        [StepInfo("更新文件覆盖到FTP")]
        private void UpdateToFtp()
        {
            foreach (var file in Directory.GetFiles(ConfigInfo.WorkSpace))
            {
                var newFileInfo = new FileInfo(file);
                System.IO.File.Copy(file, ConfigInfo.FtpDLLPath + newFileInfo.Name, true);
                log.AppendLine("    " + newFileInfo.Name);
            }
        }

        private void AddUploadFilesVers(string iniFilePath)
        {
            var fileInfo = new FileInfo(iniFilePath);
            log.BeginLogSection(string.Format("增加 {0} 版本号", fileInfo.Name));
            log.AppendLine(string.Join("\r\n", new IniParser(fileInfo.FullName).AddIniSectionVers(ConfigInfo.WorkSpace).logs));

        }

        [StepInfo("增加版本号")]
        private void AddUploadFilesVers()
        {
            if (System.IO.File.Exists(ConfigInfo.ClientIniPath))
                AddUploadFilesVers(ConfigInfo.ClientIniPath);
            if (System.IO.File.Exists(ConfigInfo.ServerIniPath))
                AddUploadFilesVers(ConfigInfo.ServerIniPath);
        }

        [StepInfo("删除临时文件")]
        private void DeleteTempFiles()
        {
            foreach (var file in Directory.GetFiles(ConfigInfo.WorkSpace))
            {
                var newFileInfo = new FileInfo(file);
                newFileInfo.Delete();
                log.AppendLine("    " + newFileInfo.Name);
            }
        }

        [StepInfo("删除历史更新文件")]
        private void DeleteExpiryFiles()
        {
            foreach (var dirName in Directory.GetDirectories(ConfigInfo.WorkSpace))
            {
                var dir = new DirectoryInfo(dirName);
                if (dir.CreationTime < DateTime.Now.AddMonths(-Convert.ToInt32(ConfigInfo.ExpiryMonth)))
                {
                    dir.Delete(true);
                    log.AppendLine("    " + dir.Name);
                }
            }
        }

        [StepInfo("更新完成")]
        private void End()
        {
            TempData.Add("message", log.ToString());
        }

        public ActionResult About()
        {
            return View();
        }

    }
}
