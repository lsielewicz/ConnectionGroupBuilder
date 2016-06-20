using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConnectionGroupBuilder.Core.Model
{
    public class ConnectionGroup
    {
        public string SchemaName { get; private set; }
        public string AppConnectionGroupdId { get; private set; }
        public string VersionId { get; private set; }
        public string DisplayName { get; private set; }
        public string Priority { get; private set; }

        public ConnectionGroup(string versionId, string displayName, string priority, bool servicePack3 = false)
        {
            if (servicePack3)
                SchemaName = @"http://schemas.microsoft.com/appv/2014/virtualapplicationconnectiongroup";
            else
                SchemaName = @"http://schemas.microsoft.com/appv/2010/virtualapplicationconnectiongroup";
            AppConnectionGroupdId = Guid.NewGuid().ToString();
            VersionId = versionId;
            DisplayName = displayName;
            Priority = priority;
        }
    }
}
