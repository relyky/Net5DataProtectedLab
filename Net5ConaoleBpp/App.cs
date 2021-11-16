using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net5ConsoleBpp.Services;
using Microsoft.AspNetCore.DataProtection;
using System.Collections.Specialized;
using System.Text.Json;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Net5ConsoleBpp
{
    class App
    {
        readonly IGetProtectedData _pdata;

        public App(IGetProtectedData pdata)
        {
            _pdata = pdata;
        }

        /// <summary>
        /// 取代原本 Program.Main() 函式的效用。
        /// </summary>
        public void Run(string[] args)
        {
            try
            {
                Console.WriteLine($"CONN_DB: {_pdata["CONN_DB"]}");

                Console.WriteLine("Dump the protected data:");
                foreach (string k in _pdata.Keys)
                {
                    Console.WriteLine($"{k}: {_pdata[k]}");
                }
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"= = = = = = = = = = = = = = = = = = >");
                Console.WriteLine($"Exception => {ex.Message}\r\n{ex}");
            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

    }

}
