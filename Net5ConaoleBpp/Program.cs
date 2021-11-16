using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Net5ConsoleBpp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = AppStartup();

            var app = ActivatorUtilities.CreateInstance<App>(host.Services);

            app.Run(args);
        }

        static IHost AppStartup()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            return host;
        }

        static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            IConfiguration config = context.Configuration;

            //## 在此註冊 services 

            var cert = GetCert();
            var certD = GetCert2();

            #region Data Protection: B 端
            services.AddDataProtection()
                .SetApplicationName(config["ProtectedDataService:ApplicationName"])
                .DisableAutomaticKeyGeneration() // 只需放在Ｂ端
                .PersistKeysToFileSystem(new DirectoryInfo(config["ProtectedDataService:PersistKeysToFileSystem"]))
                .UnprotectKeysWithAnyCertificate(certD, cert); // 輪替下來的憑證。若不指定憑證則會自動到憑證庫『My』去找對應的憑證。

            services.AddSingleton<Services.IGetProtectedData, Services.ProtectedDataService>();
            #endregion
        }

        static X509Certificate2 GetCert()
        {
            using (X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, "aaTestCert5", true);
                if (cers.Count > 0)
                {
                    return cers[0];
                };
            }

            throw new ApplicationException("找不到目標憑證！");
            return null;
        }

        static X509Certificate2 GetCert2()
        {
            using (X509Store store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, "aaTestCert6", true);
                if (cers.Count > 0)
                {
                    return cers[0];
                };
            }

            throw new ApplicationException("找不到目標憑證２！");
            return null;
        }
    }
}
