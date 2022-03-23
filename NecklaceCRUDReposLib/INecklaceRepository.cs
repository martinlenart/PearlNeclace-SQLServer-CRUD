using NecklaceModels;

namespace NecklaceCRUDReposLib
{
    internal interface INecklaceRepository
    {
        //Using a fluent syntax when possible
        Task<Necklace> CreateAsync(Necklace necklace);
        Task<IEnumerable<Necklace>> ReadAllAsync();
        Task<Necklace> ReadAsync(int necklaceId);
        Task<Necklace> UpdateAsync(Necklace necklace);
        Task<Necklace> DeleteAsync(int necklaceId);
    }
}
