using Common.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly.Order.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Polly.Order.Controllers
{
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
        /// 解析 Hello 返回 World !
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ReceiveMsg")]
        public ActionResult ReceiveMsg([FromBody] ReceiveDto req)
        {
            #region 验签
            IDictionary<string, object> response_body = new Dictionary<string, object>();
            response_body.Add("Content", req.Content);
            string str = DictionaryHelper.DictionaryToFormString(response_body, DictionaryHelper.SortOrder.Ascending) + "&";

            // 验签
            var rsaPublicKey = PfxHelper.GetCerPubKey("workCustomer.cer");
            bool yanqian = rsaPublicKey.VerifyData(Encoding.UTF8.GetBytes(str), Convert.FromBase64String(req.RsaSign), HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
            if (yanqian)
            {
                Console.WriteLine("验签成功");

                IDictionary<string, object> result_body = new Dictionary<string, object>();
                result_body.Add("Content", "World !");

                string val = DictionaryHelper.DictionaryToFormString(result_body, DictionaryHelper.SortOrder.Ascending) + "&";
                X509Certificate2 cert = new X509Certificate2("workOrder.pfx", "admin", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
                string expertDate = cert.GetExpirationDateString();
                if (Convert.ToDateTime(expertDate) <= DateTime.Now)
                    return Content("证书过期，请联系管理员");

                var sign = cert.GetRSAPrivateKey().SignData(Encoding.UTF8.GetBytes(val), HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
                var signStr = Convert.ToBase64String(sign);

                result_body.Add("RsaSign", signStr.Replace("\r", "").Replace("\n", ""));
                var request_data = JsonConvert.SerializeObject(result_body);

                return Content(request_data);
            }
            else
            {
                Console.WriteLine("验签失败");
                return Content("");
            }
            #endregion
        }
    }
}
