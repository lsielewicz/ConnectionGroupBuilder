using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace ConnectionGroupBuilder.Message
{
    public class ScriptContentMessage : MessageBase
    {
        public string PowerShellScript { get; private set; }

        public ScriptContentMessage(string script)
        {
            PowerShellScript = script;
        }
    }
}
