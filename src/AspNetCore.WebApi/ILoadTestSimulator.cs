using System.Threading.Tasks;

namespace AspNetCore.Controllers
{
    public interface ILoadTestSimulator
    {
        Task<string> SimulateLoadTestAsync();
    }
}