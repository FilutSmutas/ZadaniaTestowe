using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;

namespace Zadania
{
    static public class ExtentBuilder
    {
        static ExtentReports extent;

        public static ExtentReports GetExtent()
        {
            if (extent == null)
            {
                string desktopPath =
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\")) + @"\Reports";
                string reportPath = desktopPath +  @"\report_" + DateTime.Now.ToString().Replace(':','-') + ".html";
                
                var htmlReporter = new ExtentV3HtmlReporter(reportPath);
                extent = new ExtentReports();
                extent.AttachReporter(htmlReporter);
            }

            return extent;


        }
    }
}
