using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotSpace
{
    public abstract class DriverCreator
    {
        public abstract DriverWrapper CreateDriver(string agentString);
    }
}
