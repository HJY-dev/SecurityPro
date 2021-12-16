using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PwnedPasswords.Client;
using System.Threading.Tasks;

namespace PasswordCheck.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPwnedPasswordsClient pwnedPasswords;

        public HomeController(ILogger<HomeController> logger, IPwnedPasswordsClient pwnedPasswords)
        {
            _logger = logger;
            this.pwnedPasswords = pwnedPasswords;
        }

        /// <summary>
        /// 检测密码强度
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetAsync(string password)
        {
            if (await pwnedPasswords.HasPasswordBeenPwned(password))
            {
                return "弱密码";
            }

            return "强密码";
        }
    }
}
