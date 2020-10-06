using System.Threading.Tasks;

namespace RefitSample.RpcServices
{
    public class Helper
    {
        public static async Task<string> GetToken()
        {
            return await new ValueTask<string>("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFscGhhMjAwOCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhbHBoYTIwMDgiLCJlbWFpbCI6ImFscGhhMjAwOEB0b20uY29tIiwic3ViIjoiMjMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiIyLCIsIm5iZiI6MTYwMTg4NzgzNywiZXhwIjoxNjAyMjQ3ODM3LCJpc3MiOiIxNzIuMTYuMC40In0.z9fTSm8y0AvhodnT2qL950OZo47QCiS9rZ5HMyfFVLY");
        }
    }
}
