using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Consul;


namespace RefitConsul
{
    public class ConsulDiscoveryDelegatingHandler : DelegatingHandler
    {
        private readonly ConsulClient _consulClient;
        private readonly Func<Task<string>> _token;
        public ConsulDiscoveryDelegatingHandler(string consulAddress
            , Func<Task<string>> token = null)
        {
            _consulClient = new ConsulClient(x =>
            {
                x.Address = new Uri(consulAddress);
            });

            _token = token;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request
            , CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            var cacheKey = $"service_consul_url_{current.Host }";
            try
            {
                var auth = request.Headers.Authorization;
                if (auth != null)
                {
                    if (_token == null) throw new ArgumentNullException(nameof(_token));

                    var tokenTxt = await _token();
                    request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, tokenTxt);
                }

                var serverUrl = CacheManager.GetOrCreate<string>(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);
                    return LookupService(current.Host);
                });

                request.RequestUri = new Uri($"{current.Scheme}://{serverUrl}{current.PathAndQuery}");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                CacheManager.Remove(cacheKey);
                throw;
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        private string LookupService(string serviceName)
        {
            var servicesEntry = _consulClient.Health.Service(serviceName, string.Empty, true).Result.Response;
            if (servicesEntry != null && servicesEntry.Any())
            {
                int index = new Random().Next(servicesEntry.Count());
                var entry = servicesEntry.ElementAt(index);
                return $"{entry.Service.Address}:{entry.Service.Port}";
            }
            return null;
        }
    }
}
