using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using ConnectionGroupBuilder.Core.Model;
using ICSharpCode.SharpZipLib.Zip;

namespace ConnectionGroupBuilder.Core.Services
{
    public class AppVPackageService
    {
        //public string Path { get; private set; }
        public ConnectionGroup ConnectionGroup { get; set; }
        public List<AppVPackage> PackagesList { get; private set; }
        public string PowerShellScript { get; private set; }
        private string _fileName;

        public AppVPackageService()
        {
            PackagesList = new List<AppVPackage>();           
        }
        
        private string GetManifestFromAppVFile(string path)
        {
            _fileName = System.IO.Path.GetFileName(path);
            string manifest = String.Empty;
            try
            { 
                string directoryPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppVSources");
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                
                string appVFilePath = System.IO.Path.Combine(directoryPath, _fileName);
                if(!File.Exists(appVFilePath))
                    File.Copy(path, appVFilePath);
                _fileName = System.IO.Path.ChangeExtension(appVFilePath, ".zip");

                if (!File.Exists(_fileName))
                {
                    FileInfo fileInfo = new FileInfo(appVFilePath);
                    fileInfo.MoveTo(System.IO.Path.ChangeExtension(appVFilePath, ".zip"));
                }

                FastZip fastZip = new FastZip();

                string pathToUnZip = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    System.IO.Path.GetFileNameWithoutExtension(_fileName));

                if (!Directory.Exists(pathToUnZip))
                    Directory.CreateDirectory(pathToUnZip);
                fastZip.ExtractZip(_fileName,pathToUnZip,null);

                string manifestPath = System.IO.Path.Combine(pathToUnZip, "AppxManifest.xml");

                using (StreamReader sr = new StreamReader(manifestPath))
                {
                    manifest = sr.ReadToEnd();
                }
                DeleteFilesAfterDataReading(directoryPath);
                DeleteFilesAfterDataReading(pathToUnZip);
            }
            catch (Exception ex)
            {
                //TODO exception handling
            }
            return manifest;
        }

        public void ExtractDataFromManifest(string path, bool isOptional=false)
        {
            string manifest = GetManifestFromAppVFile(path);
            string packageId = GetPackageId(manifest);
            string versionId = GetVersionId(manifest);
            string displayName = GetDisplayName(manifest);

            PackagesList.Add(new AppVPackage(packageId, versionId, displayName, isOptional, path));
        }

        private string GetPackageId(string manifest)
        {
            string packageId = String.Empty;
            string pattern = "appv:PackageId=\"(.{8}-.{4}-.{4}-.{4}-.{12})\"";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(manifest);
            if (match.Success && match.Groups.Count > 1)
                packageId = match.Groups[1].Value;
           
            return packageId;
        }

        private string GetVersionId(string manifest)
        {
            string versionId = String.Empty;
            string pattern = "appv:VersionId=\"(.{8}-.{4}-.{4}-.{4}-.{12})\"";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(manifest);
            if (match.Success && match.Groups.Count > 1)
                versionId = match.Groups[1].Value;

            return versionId;
        }

        private string GetDisplayName(string manifest)
        {
            string displayName = String.Empty;
            string pattern = "<DisplayName>(.*)</DisplayName>";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(manifest);
            if (match.Success && match.Groups.Count > 1)
                displayName = match.Groups[1].Value;

            return displayName;
        }

        public void GenerateXmlConnectionGroup(string versionId, string displayName, string priority, string pathToSave="")
        {
            if (!PackagesList.Any())
                return;

            ConnectionGroup = new ConnectionGroup(versionId,displayName,priority);

            XNamespace appvNamespace =ConnectionGroup.SchemaName;
            
            XDocument connectionGroup = new XDocument(
                    new XDeclaration("1.0","utf-16","yes"),
                    new XElement(
                        appvNamespace + "AppConnectionGroup",
                        new XAttribute("xmlns", ConnectionGroup.SchemaName),
                        new XAttribute(XNamespace.Xmlns + "appv",ConnectionGroup.SchemaName),
                        new XAttribute("AppConnectionGroupId", ConnectionGroup.AppConnectionGroupdId),
                        new XAttribute("VersionId",ConnectionGroup.VersionId),
                        new XAttribute("Priority", ConnectionGroup.Priority),
                        new XAttribute("DisplayName", ConnectionGroup.DisplayName),
                        new XElement(
                            appvNamespace + "Packages",
                            from package in PackagesList select new XElement(
                                appvNamespace + "Package",
                                    new XAttribute("DisplayName", package.DisplayName),
                                    new XAttribute("PackageId", package.PackageId),
                                    new XAttribute("VersionId", package.VersionId)
                                )
                            )
                        )
               );
            if (pathToSave == String.Empty)
            {
                pathToSave =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        ConnectionGroup.DisplayName + ".xml");
            }
            connectionGroup.Save(pathToSave);
            PowerShellScript = GeneratePowerShellScriptForPublishing(pathToSave);
            CleanUp();
        }

        private void CleanUp()
        {
            PackagesList = new List<AppVPackage>();
        }

        private void DeleteFilesAfterDataReading(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public string GeneratePowerShellScriptForPublishing(string connectionGroupPath)
        {
            string scriptContent = String.Empty;
            PackagesList.ForEach(p =>
            {
                scriptContent += String.Format("Add-AppvClientPackage -path \"{0}\" | Publish-AppvClientPackage\n",p.PhisicalPathToPackage);
            });
            scriptContent += String.Format("Add-AppvClientConnectionGroup -path \"{0}\" \n", connectionGroupPath);
            scriptContent += String.Format("Enable-AppvClientConnectionGroup -name \"{0}\"",ConnectionGroup.DisplayName);

            return scriptContent;
        }

    }
}
