using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotSpace
{
    public class ExpectedBotCondition
    {
        public static Func<IWebDriver, IWebElement> GetDivContainingText(string text)
        {
            return (driver) =>
            {
                IWebElement retVal = null;
                // "//h5[contains(@class, 'ban hot') and text() = 'us states']"));
                var elements = driver.FindElements(By.XPath("//div[text()[contains(.,'" + text +"')]]"));
                var first = elements.SingleOrDefault(x => x.Text.Trim() == text);

                if (first != null)
                {
                    retVal = first;
                }

                return retVal;
            };
        }

        public static Func<IWebDriver, Boolean> ElementCountHasIncreased(int oldElementCount)
        {
            return (driver) =>
            {
                bool retVal = false;

                if (retVal)
                {
                    var elements = driver.FindElements(By.XPath("//*")).Count;

                    if (elements > oldElementCount)
                    {
                        retVal = true;
                    }
                }

                return retVal;
            };
        }

        public static Func<IWebDriver, IWebElement> PageHasClassWithText(string className, string title)
        {
            return (driver) =>
            {
                IWebElement retval = null;
                var iwe = driver.FindElement(By.ClassName(className));
                if (iwe != null)
                {
                    if (iwe.Text == title)
                    {
                        retval = iwe;
                    }
                }

                return retval;
            };
        }

        public static Func<IWebDriver, Boolean> PageHasClassContainingString(string className, string text)
        {
            return (driver) =>
            {
                Boolean retval = false;
                var iwe = driver.FindElement(By.ClassName(className));
                if (iwe != null)
                {
                    if (iwe.Text.Contains(text))
                    {
                        retval = true;
                    }
                }

                return retval;
            };
        }

        public static Func<IWebDriver, String> ThereIsAClock()
        {
            return (driver) =>
            {
                    string retVal = "";
                    var clocks = driver.FindElements(By.Id("mlClock"));
                    if (clocks.Count != 0)
                    {
                        if(clocks[0].Text.Contains(':'))
                        {
                            retVal = clocks[0].Text;
                        }
                    }
                    return retVal;
            };
        }

        public static Func<IWebDriver, Boolean> ThereIsAnIdWithAttributeWithValue(string id, string attribute, string value)
        {
            return (driver) =>
            {
                Boolean retval = false;
                string iwe = "";

                try
                {
                    iwe = driver.FindElement(By.Id(id)).GetAttribute(attribute);
                }
                catch
                { }

                if (iwe != "")
                {
                    if (iwe == value)
                    {
                        retval = true;
                    }
                }

                return retval;
            };
        }

        public static Func<IWebDriver, bool> TitleIs(string title)
        {
            return (driver) => { return title == driver.Title; };
        }

        public static Func<IWebDriver, bool> TitleContains(string title)
        {
            return (driver) => { return driver.Title.Contains(title); };
        }

        public static Func<IWebDriver, IWebElement> ElementExists(By locator)
        {
            return (driver) => { return driver.FindElement(locator); };
        }

        public static Func<IWebDriver, IWebElement> ElementIsVisible(By locator)
        {
            return (driver) =>
            {
                try
                {
                    return ElementIfVisible(driver.FindElement(locator));
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            };
        }

        private static IWebElement ElementIfVisible(IWebElement element)
        {
            if (element.Displayed)
            {
                return element;
            }
            else
            {
                return null;
            }
        }
    }

}
