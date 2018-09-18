using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otsol.Support.Json;
using System.Configuration;
using System.Threading;
using System.IO;

namespace Mobiera.verifyContent
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Mobiera.VerifyContent");
            BOVerifyContent bo = new BOVerifyContent();
            bo.verifyContentToSend();
           
            //Console.Beep();
            return;
            //Console.WriteLine("Press enter to exit...");
            //Console.ReadLine();
            //Environment.Exit(0);
        }
    }
}
