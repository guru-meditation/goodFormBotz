using BotSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrowserAutomation
{
    public class DriverFactory
    {
        public enum Browser
        {
            Chrome,
            Firefox,
            Phantom
        }

        public static DriverWrapper getDriverWaiter(Browser type, string agentString)
        {
            if (type == Browser.Chrome)
            {
                return new ChromeDriverCreator().CreateDriver(agentString);
            }

            return null;
        }
    }
}
