using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using System;

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

    public class PhantomDriverCreator : DriverCreator
    {
        private static readonly log4net.ILog log2 = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override DriverWrapper CreateDriver(string agentString)
        {
            DriverWrapper driver = null;

            var sCaps = new DesiredCapabilities();

            try
            {
                if (string.IsNullOrEmpty(agentString) == false)
                {
                    var options = new PhantomJSOptions();
                    options.AddAdditionalCapability("phantomjs.page.settings.userAgent", agentString);

                    driver = new DriverWrapper(new PhantomJSDriver(options));
                }
                else
                {
                    driver = new DriverWrapper(new ChromeDriver());
                }

            }
            catch (Exception e)
            {
                log2.Error("Exception: " + e);
            }

            return driver;
        }
    }
}
