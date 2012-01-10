using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiredPrairieUS.Devices;
using System.IO;

namespace TestAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            Nest nest = new Nest("test@example", "password");
            // you'll need to create your own sample files!
            string body = File.ReadAllText(@"d:\Aaron\Documents\nest-login1.txt");
            nest.ParseAuthenticationResponse(body);
            // you'll need to create your own sample files!
            body = File.ReadAllText(@"d:\Aaron\Documents\nest-status1.txt");            
            nest.DeconstructStatus(body);


            foreach (var structure in nest.Structures)
            {
                Console.WriteLine("===== Structure {0}", structure.Id);
                foreach (var device in structure.Devices)
                {
                    Console.WriteLine("Device {0}", device.AllProperties.name);
                    Console.WriteLine("-- Current Temp: {0} ({1})", device.AllProperties.current_temperature, Nest.CelsiusToFohrenheit((double)device.AllProperties.current_temperature));
                }
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
