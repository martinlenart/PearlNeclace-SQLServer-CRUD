using NecklaceModels;

namespace NecklaceCRUDReposLib
{
    public interface IPearlRepository
    {
        //Using a fluent syntax when possible
        Task<Pearl> CreateAsync(Pearl pearl);
        Task<IEnumerable<Pearl>> ReadAllAsync();
        Task<Pearl> ReadAsync(int pearlId);
        Task<Pearl> UpdateAsync(Pearl pearl);
        Task<Pearl> DeleteAsync(int pearlId);
    }
}