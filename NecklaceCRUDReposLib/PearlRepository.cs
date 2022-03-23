using NecklaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using NecklaceDB;

namespace NecklaceCRUDReposLib
{
    internal class PearlRepository : IPearlRepository
    {
        NecklaceDbContext _db = null;
        public async Task<Pearl> CreateAsync(Pearl pearl)
        {
            var added = await _db.Pearls.AddAsync(pearl);

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
                return pearl;
            else
                return null;
        }
        public async Task<IEnumerable<Pearl>> ReadAllAsync()
        {
            return await Task.Run(() => _db.Pearls);
        }
        public async Task<Pearl> ReadAsync(int pearlId)
        {
            return await _db.Pearls.FindAsync(pearlId);
        }
        public async Task<Pearl> UpdateAsync(Pearl pearl)
        {
            _db.Pearls.Update(pearl); //No db interaction until SaveChangesAsync
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
                return pearl;
            else
                return null;
        }
        public async Task<Pearl> DeleteAsync(int pearlId)
        {
            var cusDel = await _db.Pearls.FindAsync(pearlId);
            _db.Pearls.Remove(cusDel);

            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
                return cusDel;
            else
                return null;
        }
        public PearlRepository(NecklaceDbContext db)
        {
            _db = db;
        }
    }
}
