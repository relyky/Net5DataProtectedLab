using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net5ConaoleApp.Services;
using Microsoft.AspNetCore.DataProtection;
using System.Collections.Specialized;
using System.Text.Json;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Net5ConaoleApp
{
    class App
    {
        readonly IConfiguration _config;
        readonly RandomService _randSvc;
        readonly IDataProtector _protector;

        public App(IConfiguration config, RandomService randSvc, IDataProtectionProvider protectionProvider)
        {
            _config = config;
            _randSvc = randSvc;
            _protector = protectionProvider.CreateProtector(config["ProtectedDataService:Purpose"]);
        }

        /// <summary>
        /// 取代原本 Program.Main() 函式的效用。
        /// </summary>
        public void Run(string[] args)
        {
            try
            {
                FileInfo plainPath = new FileInfo(_config["ProtectedDataService:PlainDataFilepath"]);
                FileInfo protectPath = new FileInfo(_config["ProtectedDataService:ProtectedDataFilepath"]);

                ProtectData(plainPath, protectPath);
                Console.WriteLine("ProtectData.");

                var pprs = UnprotectData(protectPath);
                Console.WriteLine("UnprotectData.");

                string value = pprs["CONN_FOO"].AsString();
                Console.WriteLine($"value: {value}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"= = = = = = = = = = = = = = = = = = >");
                Console.WriteLine($"Exception => {ex.Message}\r\n{ex}");
            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        class CommandArgs { 
            public string cfgPath = null;
            public string outPath = null;
            public string dataPath = null;
        }

        void ProtectData(FileInfo plainPath, FileInfo protectPath)
        {
            Dictionary<string, string> ppr;
            try
            {
                // 取得數據
                string plainJson = File.ReadAllText(plainPath.FullName);
                ppr = JsonSerializer.Deserialize<Dictionary<string, string>>(plainJson);
            }
            catch(Exception ex)
            {
                throw new ApplicationException("數據來源格式錯誤！", ex);
            }

            try
            {
                // 加密保護數據
                File.WriteAllBytes(protectPath.FullName, _protector.Protect(UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ppr))));
            }
            catch 
            {
                throw;
            }
        }

        Dictionary<string, SecureString> UnprotectData(FileInfo protectPath)
        {
            try
            {
                // 解密保護數據
                byte[] protectBlob2 = File.ReadAllBytes(protectPath.FullName);
                var decodBlob = _protector.Unprotect(protectBlob2);
                var dppr = JsonSerializer.Deserialize<Dictionary<string, string>>(UTF8Encoding.UTF8.GetString(decodBlob));
                var dpprs = new Dictionary<string, SecureString>();
                dppr.ToList().ForEach(kv =>
                {
                    dpprs.Add(kv.Key, kv.Value.AsSecureString());
                });

                return dpprs;
            }
            catch
            {
                throw;
            }
        }
    }

    static class SecureStringExtension
    {
        public static SecureString AsSecureString(this String str)
        {
            SecureString secstr = new SecureString();
            str.ToList().ForEach(secstr.AppendChar);
            secstr.MakeReadOnly();
            return secstr;
        }

        public static String AsString(this SecureString secstr)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secstr);
                return Marshal.PtrToStringUni(valuePtr);
            }
            catch
            {
                return null;
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

    }

}
