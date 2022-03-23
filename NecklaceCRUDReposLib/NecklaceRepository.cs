using NecklaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NecklaceDB;

namespace NecklaceCRUDReposLib
{
    public class NecklaceRepository : INecklaceRepository
    {
        NecklaceDbContext _db = null;

        public async Task<Necklace> CreateAsync(Necklace necklace)
        {
            var added = await _db.Necklaces.AddAsync(necklace);

            int affected = await _db.SaveChangesAsync();
            if (affected == necklace.Count()+1)
                return necklace;
            else
                return null;
        }
        public async Task<IEnumerable<Necklace>> ReadAllAsync()
        {
            return await Task.Run(() => _db.Necklaces);
        }
        public async Task<Necklace> ReadAsync(int necklaceId)
        {
            return await _db.Necklaces.FindAsync(necklaceId);
        }
        public async Task<Necklace> UpdateAsync(Necklace necklace)
        {
            _db.Necklaces.Update(necklace); //No db interaction until SaveChangesAsync
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
                return necklace;
            else
                return null;
        }
        public async Task<Necklace> DeleteAsync(int necklaceId)
        {
            var delNecklace = await _db.Necklaces.FindAsync(necklaceId);
            _db.Necklaces.Remove(delNecklace);

            int affected = await _db.SaveChangesAsync();
            if (affected == delNecklace.Count() + 1)
                return delNecklace;
            else
                return null;
        }
        public NecklaceRepository(NecklaceDbContext db)
        {
            _db = db;
        }

    }
}
