using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Net5ConaoleApp
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
            services.AddScoped<Services.RandomService>();

            /// 暫不使用憑證
            //var cert1 = FindCertInStore(StoreName.TrustedPeople, StoreLocation.CurrentUser, "aaTestCert5");
            //var cert2 = FindCertInStore(StoreName.TrustedPeople, StoreLocation.CurrentUser, "aaTestCert6");
            //var cert3 = FindCertInStore(StoreName.My, StoreLocation.CurrentUser, "aaTestCert19");

            //## Data Protection: A 端
            // 程式一執行就會建立加密金鑰XML檔案
            //  * 預設的過期時間為90天，致少一星期。
            //  * 預設加密(encryption)的演算法為AES_256_CBC。
            //  * 預設驗證(validation)的演算法為HMACSHA256。
            services.AddDataProtection()
                .SetApplicationName(config["ProtectedDataService:ApplicationName"])
                .PersistKeysToFileSystem(new DirectoryInfo(config["ProtectedDataService:PersistKeysToFileSystem"]))
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
                //.ProtectKeysWithCertificate(cert3) // 現在使用的憑證。
                //.UnprotectKeysWithAnyCertificate(cert2, cert1) // 輪替下來的憑證，只會用來解密。
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_128_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256,
                });
        }

        static X509Certificate2 FindCertInStore(StoreName storeName, StoreLocation location, string subject, bool validOnly = true)
        {
            using (X509Store store = new X509Store(storeName, location))
            {
                store.Open(OpenFlags.ReadOnly);
                var result = store.Certificates.Find(X509FindType.FindBySubjectName, subject, validOnly);
                if (result.Count > 0)
                {
                    return result[0];
                };
            }

            //throw new ApplicationException($@"找不到目標憑證[Subject = {subject}]！");
            return null;
        }

      
    }
}
