using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Refit;
using RefitConsul;
using RefitSample.RpcServices;

namespace RefitSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //重试策略
            var retryPolicy = Policy.Handle<HttpRequestException>()
                                    .OrResult<HttpResponseMessage>(response => response.StatusCode== System.Net.HttpStatusCode.BadGateway)
                                    .WaitAndRetryAsync(new[]
                                    {
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(5),
                                        TimeSpan.FromSeconds(10)
                                    });
            //超时策略
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);
            //隔离策略
            var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(10, 100);
            //回退策略
            //断路策略
            var circuitBreakerPolicy = Policy.Handle<Exception>()
                           .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
