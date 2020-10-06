using Refit;
using System.Threading.Tasks;

namespace RefitSample.RpcServices
{
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
}