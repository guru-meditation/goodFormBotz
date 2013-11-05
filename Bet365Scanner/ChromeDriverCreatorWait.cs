using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebDriver;

namespace BotSpace
{
    public class ChromeDriverCreatorWait : DriverCreator
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
                    //options.AddArgument("user-data-dir=c:\temp");
                    driver = new DriverWrapperWait(new ChromeDriver(options));
                }
                else
                {
                    driver = new DriverWrapperWait(new ChromeDriver());
                }

            }
            catch (Exception e)
            {
                log.Error("Exception: " + e);
            }

            return driver;
        }
    }



    public class PhantomDriverCreatorCreatorWait : DriverCreator
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

                    driver = new DriverWrapperWait(new PhantomJSDriver(options));
                }
                else
                {
                    driver = new DriverWrapperWait(new ChromeDriver());
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
