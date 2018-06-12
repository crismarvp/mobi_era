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

            Console.WriteLine("verifyContent");
            BOVerifyContent bo = new BOVerifyContent();
            bo.verifyContentToSend();
        
            Console.Beep();
            return;

            //Environment.Exit(0);
        }
    }
}
