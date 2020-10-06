# RefitConsul
RefitConsul扩展了Refit组件的Consul服务发现功能，能更加便捷调用Restful服务。
consul服务发现实现了随机轮询策略与缓存。

## 如何使用
###
1、定义服务接口
```C#
	public interface IAuthApi
	{
		/// <summary>
		/// 不需要验证的接口
		/// </summary>
		/// <returns></returns>
		[Get("/sys/users")]
		Task <dynamic> GetUsers();

		/// <summary>
		/// 接口采用Bearer方式验证，Token在ConsulDiscoveryDelegatingHandler统一获取
		/// </summary>
		/// <returns></returns>
		[Get("/sys/session")]
		[Headers("Authorization: Bearer")]
		Task<dynamic> GetCurrentUserInfo();

		/// <summary>
		/// 接口采用Bearer方式验证，Token使用参数方式传递
		/// </summary>
		/// <returns></returns>
		[Get("/sys/session")]
		Task<dynamic> GetCurrentUserInfo([Header("Authorization")] string authorization);
	}
 ``` 

2、startup中注册并配置
```C#
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
	    //注册RefitClient
            //用SystemTextJsonContentSerializer替换默认的NewtonsoftJsonContentSerializer序列化组件
            //如果调用接口是使用NewtonsoftJson序列化则不需要替换
            services.AddRefitClient<IAuthApi>(new RefitSettings(new SystemTextJsonContentSerializer()))
                    //设置服务名称，andc-api-sys是系统在Consul注册的服务名
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://andc-api-sys"))
                    //注册ConsulDiscoveryDelegatingHandler,
                    .AddHttpMessageHandler(() =>
                    {
                        //http://12.112.75.55:8550是consul服务器的地址
                        //() => Helper.GetToken() 获取token的方法，是可选参数，如果不需要token验证不需要传递。
                        return new ConsulDiscoveryDelegatingHandler("http://12.112.75.55:8550", () => Helper.GetToken());
                    })
                    //设置httpclient生命周期时间，默认也是2分钟。
                    .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                    //添加polly相关策略
                    .AddPolicyHandler(retryPolicy)
                    .AddPolicyHandler(timeoutPolicy)
                    .AddPolicyHandler(bulkheadPolicy);
        }
```

3、controller中使用
```C#
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
            var result1 = await _authApi.GetUsers();

            //需要验证，token采用参数传递
            var result2 = await _authApi.GetCurrentUserInfo($"Bearer {_token}");

            //需要验证,token在ConsulDiscoveryDelegatingHandler获取。
            var result3 = await _authApi.GetCurrentUserInfo();

            return result3;
        }
```        
## License
MIT
**Free Software, Hell Yeah!**
###
