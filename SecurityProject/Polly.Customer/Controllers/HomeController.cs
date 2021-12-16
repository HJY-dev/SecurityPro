using Common.Helper;
using Common.PingAnBank;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Polly.Customer.Controllers
{
    /// <summary>
    /// Rsa
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// 发送 Hello
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SendMsg")]
        public ActionResult SendMsg()
        {
            IDictionary<string, object> request_body = new Dictionary<string, object>();
            request_body.Add("Content", "Hello");

            string val = DictionaryHelper.DictionaryToFormString(request_body, DictionaryHelper.SortOrder.Ascending) + "&";
            X509Certificate2 cert = new X509Certificate2("workCustomer.pfx", "admin", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            string expertDate = cert.GetExpirationDateString();
            if (Convert.ToDateTime(expertDate) <= DateTime.Now)
                return Content("证书过期，请联系管理员");

            var sign = cert.GetRSAPrivateKey().SignData(Encoding.UTF8.GetBytes(val), HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
            var signStr = Convert.ToBase64String(sign);

            request_body.Add("RsaSign", signStr.Replace("\r", "").Replace("\n", ""));

            var request_data = JsonConvert.SerializeObject(request_body);
            var tokenBody = HttpHelper.HttpPostAsync(PartnerConfig.PUBLIC_URL + "/api/Home/ReceiveMsg", request_data).Result;
            Console.WriteLine(tokenBody);

            #region 验签
            IDictionary<string, object> response_body = new Dictionary<string, object>();
            var obj = JObject.Parse(tokenBody);
            response_body.Add("Content", obj.Value<string>("Content"));
            string str = DictionaryHelper.DictionaryToFormString(response_body, DictionaryHelper.SortOrder.Ascending) + "&";

            var rsaPublicKey = PfxHelper.GetCerPubKey("workOrder.cer");
            bool yanqian = rsaPublicKey.VerifyData(Encoding.UTF8.GetBytes(str), Convert.FromBase64String(obj.Value<string>("RsaSign")), HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
            if (yanqian)
            {
                Console.WriteLine($"验签成功,返回结果：{obj.Value<string>("Content")}");
            }
            else
            {
                Console.WriteLine("验签失败");
            }
            #endregion

            return Content(JsonConvert.SerializeObject(tokenBody));
        }
    }
}
