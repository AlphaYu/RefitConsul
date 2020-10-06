using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Refit;
using RefitConsul;
using RefitSample.RpcServices;

namespace RefitSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IAuthApi _authApi;
        private readonly string _token = Helper.GetToken().Result;

        /// <summary>
        /// RefitConsul测试
        /// </summary>
        /// <param name="authApi">IAuthApi服务</param>
        public HomeController(IAuthApi authApi)
        {
            _authApi = authApi;
        }

        [HttpGet]
        public async Task<dynamic> GetAsync()
        {
            //不需要验证的服务
            //var result1 = await _authApi.GetUsers();

            //需要验证，token采用参数传递
            //var result2 = await _authApi.GetCurrentUserInfo($"Bearer {_token}");

            //需要验证,token在ConsulDiscoveryDelegatingHandler获取。
            var result3 = await _authApi.GetCurrentUserInfo();

            return result3;
        }
    }
}
