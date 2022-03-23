using NecklaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NecklaceDB;

namespace NecklaceCRUDReposLib
{
    internal class NecklaceRepository : INecklaceRepository
    {
        NecklaceDbContext _db = null;

        public async Task<Necklace> CreateAsync(Necklace necklace)
        {
            var added = await _db.Necklaces.AddAsync(necklace);

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
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
            var cusDel = await _db.Necklaces.FindAsync(necklaceId);
            _db.Necklaces.Remove(cusDel);

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
                return cusDel;
            else
                return null;
        }
        public NecklaceRepository(NecklaceDbContext db)
        {
            _db = db;
        }

    }
}
