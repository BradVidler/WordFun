using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;
using WordFunLibrary;

namespace WordFunService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Create service host 
                ServiceHost servHost = new ServiceHost(typeof(WordList));

                // Start the service
                servHost.Open();

                // Keep the server running until <Enter> is pressed
                Console.WriteLine("WordFun service is activated. Press <Enter> to quit.");
                Console.ReadKey();

                // Shut down the service
                servHost.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey(); // wait for the user to press enter
            }
        }
    }
}
