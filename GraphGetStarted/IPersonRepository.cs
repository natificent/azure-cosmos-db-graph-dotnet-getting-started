using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GraphGetStarted
{
    public interface IPersonRepository
    {
        Task AddAsync(Person person);
        Task<Person> GetAsync(int id);
        Task DeleteAsync(int id);
    }
}