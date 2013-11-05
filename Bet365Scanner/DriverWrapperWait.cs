using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebDriver;

namespace BotSpace
{
    public class DriverWrapperWait : DriverWrapper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int waitTimeSeconds = 20;

        public DriverWrapperWait(IWebDriver dr)
            : base(dr)
        {
        }

        public override void DirtySleep(int time)
        {
            // don't sleep
        }

        public override bool Wait(Func<bool> f)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(waitTimeSeconds));
            return wait.Until((drv) =>
            {
                return f();

            });
        }

        private IWebElement FindElement(By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until((drv) =>
                {
                    var element = drv.FindElement(by);

                    return element;
                }
                        );
            }
            return base.FindElement(by);
        }

        private System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> FindElements(By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv =>
                {
                    var elements = drv.FindElements(by);
                    if (elements.Count == 0)
                    {
                        return null;
                    }
                    return elements;
                }
                    );
            }
            return base.FindElements(by);
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return FindElements(by, waitTimeSeconds);
        }

        public override IWebElement FindElement(By by)
        {
            return FindElement(by, waitTimeSeconds);
        }



        public override IWebElement GetWebElementFromClassAndDivText(string classType, string findString)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            return wait.Until(drv =>
            {
                return base.GetWebElementFromClassAndDivText(classType, findString);
            }
            );
        }

        //public override List<string> GetValuesById(string searchId, int attempts, int expected, string seperator)
        //{
        //    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        //    try
        //    {
        //        return wait.Until(drv =>
        //        {
        //            while (attempts-- != 0)
        //            {
        //                var data = Regex.Split(driver.FindElement(By.Id(searchId)).Text, seperator);
        //                var dataList = data.ToList();
        //                dataList.RemoveAll(x => String.IsNullOrWhiteSpace(x));

        //                if (dataList.Count() == expected || expected == 0)
        //                {
        //                    return dataList;
        //                }
        //                return null;
        //            }
        //            throw new Exception("Enough Attempts");
        //        }
        //            );
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        public override List<string> GetValuesByClassName(string searchId, int attempts, int expected, char[] seperators)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
            try
            {
                return wait.Until(drv =>
                {
                    while (attempts-- != 0)
                    {
                        var data = Driver.FindElement(By.ClassName(searchId)).Text.Split(seperators);
                        var dataList = data.ToList();
                        dataList.RemoveAll(x => String.IsNullOrEmpty(x));

                        if (dataList.Count() == expected)
                        {
                            return dataList;
                        }
                        return null;
                    }
                    throw new Exception("Enough Attempts");
                }
                );
            }
            catch (Exception)
            {
                return null;
            }
        }

    }

}
