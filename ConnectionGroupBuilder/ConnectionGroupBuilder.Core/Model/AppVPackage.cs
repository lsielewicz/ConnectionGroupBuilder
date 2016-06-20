using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConnectionGroupBuilder.Core.Model
{
    public class AppVPackage
    {
        public string DisplayName { get; set; }
        public string PackageId { get; set; }
        public string VersionId { get; set; }
        public bool IsOptional { get; set; }
        public string PhisicalPathToPackage { get; set; }

        public AppVPackage() { }

        public AppVPackage(string packageId, string versionId, string displayName, bool isOptional, string phiscialPathToPackage)
        {
            PackageId = packageId;
            VersionId = versionId;
            DisplayName = displayName;
            IsOptional = isOptional;
            PhisicalPathToPackage = phiscialPathToPackage;
        }
    }
}
