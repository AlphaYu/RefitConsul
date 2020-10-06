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

            //���Բ���
            var retryPolicy = Policy.Handle<HttpRequestException>()
                                    .OrResult<HttpResponseMessage>(response => response.StatusCode== System.Net.HttpStatusCode.BadGateway)
                                    .WaitAndRetryAsync(new[]
                                    {
                                        TimeSpan.FromSeconds(1),
                                        TimeSpan.FromSeconds(5),
                                        TimeSpan.FromSeconds(10)
                                    });
            //��ʱ����
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);
            //�������
            var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(10, 100);
            //���˲���
            //��·����
            var circuitBreakerPolicy = Policy.Handle<Exception>()
                           .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
            //ע��RefitClient
            //��SystemTextJsonContentSerializer�滻Ĭ�ϵ�NewtonsoftJsonContentSerializer���л����
            //������ýӿ���ʹ��NewtonsoftJson���л�����Ҫ�滻
            services.AddRefitClient<IAuthApi>(new RefitSettings(new SystemTextJsonContentSerializer()))
                    //���÷������ƣ�andc-api-sys��ϵͳ��Consulע��ķ�����
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://andc-api-sys"))
                    //ע��ConsulDiscoveryDelegatingHandler,
                    .AddHttpMessageHandler(() =>
                    {
                        //http://12.112.75.55:8550��consul�������ĵ�ַ
                        //() => Helper.GetToken() ��ȡtoken�ķ������ǿ�ѡ�������������Ҫtoken��֤����Ҫ���ݡ�
                        return new ConsulDiscoveryDelegatingHandler("http://12.112.75.55:8550", () => Helper.GetToken());
                    })
                    //����httpclient��������ʱ�䣬Ĭ��Ҳ��2���ӡ�
                    .SetHandlerLifetime(TimeSpan.FromMinutes(2))
                    //���polly��ز���
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
