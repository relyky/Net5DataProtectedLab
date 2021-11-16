using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Net5ConsoleBpp.Services
{
    interface IGetProtectedData
    {
        string this[string key] { get; }
        string[] Keys { get; }
    }

    class ProtectedDataService : IGetProtectedData
    {
        readonly object _lockObj = new object();
        readonly IDataProtector _protector;
        readonly IConfiguration _config;
        Dictionary<string, SecureString> _protectedData = null;

        public ProtectedDataService(IConfiguration config, IDataProtectionProvider protectionProvider)
        {
            _config = config;
            _protector = protectionProvider.CreateProtector(config["ProtectedDataService:Purpose"]);
        }

        #region 實作介面 IGetProtectedData

        string[] IGetProtectedData.Keys
        {
            get
            {
                this.ProceedUnprotectData();
                return _protectedData.Keys.ToArray();
            }
        }

        string IGetProtectedData.this[string key] { 
            get {
                this.ProceedUnprotectData();
                return CastAsString(_protectedData[key]);
            } 
        }

        #endregion

        void ProceedUnprotectData()
        {
            lock (_lockObj)
            {
                if (_protectedData == null)
                {
                    FileInfo protectPath = new FileInfo(_config["ProtectedDataService:ProtectedDataFilepath"]);
                    if (!protectPath.Exists) throw new ApplicationException("資料保護檔案不存在！");
                    _protectedData = UnprotectData(protectPath);
                }
            }
        }

        #region helper functions

        Dictionary<string, SecureString> UnprotectData(FileInfo protectPath)
        {
            try
            {
                // 解密保護數據
                byte[] protectBlob2 = File.ReadAllBytes(protectPath.FullName);
                var decodBlob = _protector.Unprotect(protectBlob2);
                var djson = UTF8Encoding.UTF8.GetString(decodBlob);
                var dppr = JsonSerializer.Deserialize<Dictionary<string, string>>(djson);
                // 再用SecureString保護
                var dpprs = new Dictionary<string, SecureString>();
                foreach (var c in dppr) dpprs.Add(c.Key, CastAsSecureString(c.Value));
                return dpprs;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("UnprotectData Fail!", ex);
            }
        }

        public SecureString CastAsSecureString(String str)
        {
            SecureString secstr = new SecureString();
            str.ToList().ForEach(secstr.AppendChar);
            secstr.MakeReadOnly();
            return secstr;
        }

        public String CastAsString(SecureString secstr)
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
        #endregion
    }
}
