﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BotSpace
{
    public class DriverWrapper : IWebDriver
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected IWebDriver driver = null;

        public System.Collections.ObjectModel.ReadOnlyCollection<string> WindowHandles
        {
            get { return driver.WindowHandles; }
        }
        public string Title
        {
            get { return driver.Title; }
        }
        public string Url
        {
            get
            {
                return driver.Url;
            }
            set
            {
                driver.Url = value;
            }
        }
        public DriverWrapper(IWebDriver dr)
        {
            driver = dr;
        }
        public virtual void Close()
        {
            driver.Close();
        }
        public string CurrentWindowHandle
        {
            get { return driver.CurrentWindowHandle; }
        }
        public virtual IOptions Manage()
        {
            return driver.Manage();
        }
        public virtual INavigation Navigate()
        {
            return driver.Navigate();
        }
        public virtual string PageSource
        {
            get { return driver.PageSource; }
        }
        public virtual void Quit()
        {
            driver.Quit();
        }
        public virtual ITargetLocator SwitchTo()
        {
            return driver.SwitchTo();
        }
        public virtual void Dispose()
        {
            driver.Dispose();
        }

        public virtual IWebElement FindElement(By by)
        {
            return driver.FindElement(by);
        }
        public virtual System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> FindElements(By by)
        {
            return driver.FindElements(by);
        }

        public virtual String GetElementText(string xpath)
        {
            String result = String.Empty;

            try
            {
                IWebElement iwe = driver.FindElement(By.XPath(xpath));
                result = iwe.Text;
            }
            catch (Exception)
            {
                log.Error("=========> Exception thrown trying to find element: " + xpath);
            }

            return result;
        }
        public virtual bool ClickElement(IWebElement iwe)
        {
            bool result = false;

            try
            {
                if (iwe != null)
                {
                    iwe.Click();
                    result = true;
                }
                else
                {
                    log.Error("Couldn't click NULL web element");
                }
            }
            catch (Exception ce)
            {
                log.Error("=========> Exception thrown trying to click element: " + iwe.TagName + " [" + ce + "]");
            }

            System.Threading.Thread.Sleep(2000);

            return result;
        }
        public virtual bool ClickElement(string xpath)
        {
            bool result = false;

            try
            {
                IWebElement iwe = driver.FindElement(By.XPath(xpath));
                if (iwe != null)
                {
                    iwe.Click();
                    result = true;
                }
                else
                {
                    log.Error("Couldn't find " + xpath + " to click");
                }
            }
            catch (Exception ce)
            {
                log.Error("=========> Exception thrown trying to click element: " + xpath + "[" + ce + "]");
            }

            return result;
        }
        public virtual IWebElement GetWebElementFromClassAndDivText(string classType, string findString)
        {
            IWebElement retVal = null;
            var thisTypes = driver.FindElements(By.ClassName(classType));

            foreach (var level1 in thisTypes)
            {
                if (level1.Text.Trim().Equals(findString))
                {
                    retVal = level1;
                    break;
                }
            }

            return retVal;
        }
        public virtual List<string> GetValuesById(string searchId, int attempts, int expected, string seperator)
        {
            while (attempts-- != 0)
            {
                var data = Regex.Split(driver.FindElement(By.Id(searchId)).Text, seperator);
                var dataList = data.ToList();
                dataList.RemoveAll(x => String.IsNullOrWhiteSpace(x));

                if (dataList.Count() == expected || expected == 0)
                {
                    return dataList;
                }
            }

            return null;
        }
        public virtual List<string> GetValuesByClassName(string searchId, int attempts, int expected, char[] seperators)
        {
                    while (attempts-- != 0)
                    {
                        var data = driver.FindElement(By.ClassName(searchId)).Text.Split(seperators);
                        var dataList = data.ToList();
                        dataList.RemoveAll(x => String.IsNullOrEmpty(x));

                        if (dataList.Count() == expected)
                        {
                            return dataList;
                        }
                        
                    }
                    return null;
         
        }

        public virtual void DirtySleep(int time)
        {
            System.Threading.Thread.Sleep(time);
        }
    }

    public abstract class DriverCreator
    {
        public abstract DriverWrapper CreateDriver(string agentString);
    }

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