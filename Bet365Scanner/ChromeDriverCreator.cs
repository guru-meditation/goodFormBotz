using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebDriver;

namespace BotSpace
{
    public class ChromeDriverCreator : DriverCreator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override DriverWrapper CreateDriver(string agentString)
        {
            DriverWrapper driver = null;

            try
            {
                if (string.IsNullOrEmpty(agentString) == false)
                {
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument(agentString);
                    driver = new DriverWrapper(new ChromeDriver(options));
                }
                else
                {
                    driver = new DriverWrapper(new ChromeDriver());
                }

            }
            catch (Exception e)
            {
                log.Error("Exception: " + e);
            }

            return driver;
        }
    }

}
