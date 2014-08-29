using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace UUFile.Infrastructure
{
    public class IniParser
    {
        private String iniFilePath;
        public readonly IDictionary<string, IDictionary<string, string>> IniSections = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

        public IniParser(string iniPath)
        {
            iniFilePath = iniPath;

            TextReader iniFile = null;
            String strLine;

            var currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            IniSections[""] = currentSection;

            if (File.Exists(iniPath))
            {
                try
                {
                    iniFile = new StreamReader(iniPath, Encoding.Default);
                    strLine = iniFile.ReadLine();
                    while (strLine != null)
                    {
                        strLine = strLine.Trim();
                        if (string.IsNullOrWhiteSpace(strLine))
                        {
                            strLine = iniFile.ReadLine();
                            continue;
                        }
                        if (strLine.StartsWith(";"))
                        {
                            strLine = iniFile.ReadLine();
                            continue;
                        }

                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                            IniSections[strLine.Substring(1, strLine.LastIndexOf("]") - 1)] = currentSection;
                            strLine = iniFile.ReadLine();
                            continue;
                        }

                        var idx = strLine.IndexOf("=");
                        if (idx == -1)
                            currentSection[strLine] = "";
                        else
                            currentSection[strLine.Substring(0, idx)] = strLine.Substring(idx + 1);


                        strLine = iniFile.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);
        }


        public void SaveSettings(String newFilePath)
        {
            var content = new StringBuilder();
            foreach (var section in IniSections)
            {
                if (string.IsNullOrWhiteSpace(section.Key)) continue;

                content.AppendLine("[" + section.Key + "]");
                foreach (var setting in section.Value)
                {
                    content.AppendLine(setting.Key + "=" + setting.Value);
                }
                content.AppendLine();
            }
            TextWriter tw = null;
            try
            {
                tw = new StreamWriter(newFilePath, false, Encoding.Default);
                tw.Write(content);
            }
            finally
            {
                if (tw != null)
                    tw.Close();
            }
        }


        public void Save()
        {
            SaveSettings(iniFilePath);
        }

        public List<string> logs = new List<string>();

        public void AddVersion(string clientSettingValue)
        {
            IEnumerable<KeyValuePair<string, IDictionary<string, string>>> sections;
            sections = IniSections.Where(s => s.Value.ContainsKey("Client") && s.Value["Client"].Equals(@"\" + clientSettingValue));
            if (sections.Any())
            {
                foreach (var section in sections)
                {
                    var oldVers = Convert.ToDecimal(section.Value["Vers"]);
                    section.Value["Vers"] = (oldVers + 0.0001M).ToString();
                    logs.Add("  " + string.Format("{0,-50}", section.Value["Client"]) + " 从 ([" + oldVers + "] -> [" +
                             section.Value["Vers"] + "])");
                }
            }
        }

        public IniParser AddIniSectionVers(string filesPath)
        {
            if (IniSections.Where(s => s.Value.ContainsKey("Client")).Select(s => s.Value["Client"].TrimStart('\\'))
                         .Any(c => Directory.GetFiles(filesPath).Select(f => new FileInfo(f).Name).Contains(c)))
            {
                foreach (var file in Directory.GetFiles(filesPath))
                {
                    AddVersion(new FileInfo(file).Name);
                }
                Save();
            }
            return this;
        }
    }
}


