using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TracingIO;

namespace TracingIO_Tests
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            Logger logger = new Logger(@"C:\Users\Pim Brouwers\Desktop\");

            TestInfo(logger);
            TestError(logger);
            TestException(logger);
            TestExceptionWithMessage(logger);
            Console.ReadKey();
        }

        public static async void TestInfo(Logger log)
        {
            string someTestInfo = "This is some test info line stuff to right.";
            await log.Info(someTestInfo);
        }

        public static async void TestError(Logger log)
        {
            string someError = "This is a scary error!";
            await log.Error(someError);
        }

        public static async void TestException(Logger log)
        {
            try
            {
                try
                {
                    throw new ArgumentNullException("Some random null exception");
                }
                catch (Exception ex)
                {

                    throw new NullReferenceException("It's empty!", ex); ;
                }
            }
            catch (Exception ex)
            {
                await log.Error(ex);
            }
            
        }

        public static async void TestExceptionWithMessage(Logger log)
        {

            
            try
            {
                try
                {
                    throw new ArgumentNullException("Some random null exception");
                }
                catch (Exception ex)
                {

                    throw new NullReferenceException("It's empty!", ex); ;
                }
            }
            catch (Exception ex)
            {
                await log.Error(ex, "This was a totally fake exception");
            }
            
        }
    }
}
