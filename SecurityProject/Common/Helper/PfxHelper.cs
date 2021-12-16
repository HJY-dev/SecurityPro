using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Common.Helper
{
    public class PfxHelper
    {
        /// <summary>
        /// 获取私钥
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="password">文件秘钥</param>
        /// <returns></returns>
        public static string GetPrivateKey(string path, string password)
        {
            try
            {
                X509Certificate2 cert = new X509Certificate2(path, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                var keyPair = DotNetUtilities.GetKeyPair(cert.PrivateKey);
                var priKey = @$"-----BEGIN PRIVATE KEY----- {Base64.ToBase64String(PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private).ParsePrivateKey().GetEncoded())} -----END PRIVATE KEY-----";
                return priKey;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="password">文件秘钥</param>
        /// <returns></returns>
        public static string GetPublicKey(string path, string password)
        {
            try
            {
                X509Certificate2 cert = new X509Certificate2(path, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                var keyPair = DotNetUtilities.GetKeyPair(cert.PrivateKey);
                return Base64.ToBase64String(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public).GetEncoded());
            }
            catch
            {
                return "";
            }
        }

        public static string GetDN(string path, string password)
        {
            try
            {
                X509Certificate2 cert = new X509Certificate2(path, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                return cert.Subject;
            }
            catch
            {
                return "";
            }
        }

        public static RSA GetCerPubKey(string path)
        {
            try
            {
                string base64X509Cert = "";//"公钥证书"字符串

                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        base64X509Cert = sr.ReadToEnd().Trim();
                    }
                }
                //提取纯字符串
                base64X509Cert = base64X509Cert.Replace("-----BEGIN CERTIFICATE-----", "").Replace("-----END CERTIFICATE-----", "").Replace("\r", "").Replace("\n", "");
                //转换为“公钥”对象
                var derCert = Convert.FromBase64String(base64X509Cert);
                X509Certificate2 cert = new X509Certificate2(derCert);


                return cert.GetRSAPublicKey();
            }
            catch
            {
                return null;
            }
        }

    }
}
