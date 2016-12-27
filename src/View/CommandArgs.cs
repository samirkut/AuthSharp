
using System;

namespace AuthSharp.View
{
    public class CommandArgs: EventArgs
    {
        public CommandType Command{get;private set;}
        public object Details{get; private set;}        

        public CommandArgs(CommandType cmd, object details = null)
        {
            Command = cmd;
            Details = details;
        }
    }

}