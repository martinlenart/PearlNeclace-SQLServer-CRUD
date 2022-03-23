using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NecklaceDB;
using NecklaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NecklaceCRUDReposLib;

namespace NecklaceApplication
{
    static class MyLinqExtensions
    {
        public static void Print<T>(this IEnumerable<T> collection)
        {
            collection.ToList().ForEach(item => Console.WriteLine(item));
        }
    }
    class Program
    {
        private static DbContextOptionsBuilder<NecklaceDbContext> _optionsBuilder;
        static void Main(string[] args)
        {
            if (!BuildOptions())
                return; //Terminate if not build correctly

            #region Uncomment to seed and query the Database
 
            //SeedDataBase();
            QueryDatabaseAsync().Wait();
            QueryDatabase_Linq();
            QueryDatabase_DataModel_Linq();

            QueryDatabasePearlCRUDEAsync().Wait();
            QueryDatabaseNecklaceCRUDEAsync().Wait();
            #endregion

            Console.WriteLine("\nPress any key to terminate");
            Console.ReadKey();
        }

        private static bool BuildOptions()
        {
            _optionsBuilder = new DbContextOptionsBuilder<NecklaceDbContext>();

            #region Ensuring appsettings.json is in the right location
            Console.WriteLine($"DbConnections Directory: {DBConnection.DbConnectionsDirectory}");

            var connectionString = DBConnection.ConfigurationRoot.GetConnectionString("SQLServer_necklace");
            if (!string.IsNullOrEmpty(connectionString))
                Console.WriteLine($"Connection string to Database: {connectionString}");
            else
            {
                Console.WriteLine($"Please copy the 'DbConnections.json' to this location");
                return false;
            }
            #endregion

            _optionsBuilder.UseSqlServer(connectionString);
            return true;
        }

        #region Uncomment to seed and query the Database

        private static void SeedDataBase()
        {
            using (var db = new NecklaceDbContext(_optionsBuilder.Options))
            {
                //Create some customers
                var rnd = new Random();
                var NecklaceList = new List<Necklace>();
                for (int i = 0; i < 1000; i++)
                {
                    NecklaceList.Add(Necklace.Factory.CreateRandomNecklace(rnd.Next(5, 50)));
                }

                //Add Pearls and Necklaces to the Database
                foreach (var n in NecklaceList)
                {
                    db.Necklaces.Add(n);
                }

                db.SaveChanges();
            }
        }
        private static async Task QueryDatabaseAsync()
        {
            Console.WriteLine("\nQuery Database Async");
            using (var db = new NecklaceDbContext(_optionsBuilder.Options))
            {
                var necklaceCount = await db.Necklaces.CountAsync();
                var pearlCount = await db.Pearls.CountAsync();

                Console.WriteLine($"Nr of Necklaces: {necklaceCount}");
                Console.WriteLine($"Nr of Pearls: {pearlCount}");
            }
        }
        private static void QueryDatabase_Linq()
        {
            Console.WriteLine("\nQuery Database using Linq");
            using (var db = new NecklaceDbContext(_optionsBuilder.Options))
            {
                //Use .AsEnumerable() to make sure the Db request is fully translated to be managed by Linq.
                var necklaces = db.Necklaces.AsEnumerable().ToList();
                var pearls = db.Pearls.AsEnumerable().ToList();

                Console.WriteLine($"\nOuterJoin: Necklace - Pearls via GroupJoin: Descending by Necklace Price");
                var list1 = necklaces.GroupJoin(pearls, n => n.NecklaceID, pList => pList.NecklaceID, (n, pList) => new { n.NecklaceID, pList });
                foreach (var pearlGroup in list1.OrderByDescending(pg => pg.pList.Sum(p => p.Price)))
                {
                    Console.WriteLine($"Necklace: {pearlGroup.NecklaceID}, Nr Of Pearls: {pearlGroup.pList.Count()}, Price: {pearlGroup.pList.Sum(p => p.Price):C2}");
                }

                Console.WriteLine($"\nOuterJoin: Customer - Order via GroupJoin: Customer with highest ordervalue");
                var MostExpensive = list1.OrderByDescending(pg => pg.pList.Sum(p => p.Price)).First();

                Console.WriteLine($"Most expensive Necklace: {MostExpensive.NecklaceID}, Nr Of Pearls: {MostExpensive.pList.Count()}, Price: {MostExpensive.pList.Sum(p => p.Price):C2}");
            }
        }
        private static void QueryDatabase_DataModel_Linq()
        {
            Console.WriteLine("\nQuery Database using fully loaded datamodels");
            using (var db = new NecklaceDbContext(_optionsBuilder.Options))
            {
                //Use .AsEnumerable() to make sure the Db request is fully translated to be managed by Linq.
                //Use ToList() to ensure the Model is fully loaded
                var necklaces = db.Necklaces.ToList();
                var pearls = db.Pearls.ToList();

                var MostExpensiveNecklace = necklaces.OrderByDescending(n => n.Price).First();
                Console.WriteLine($"Most expensive Necklace: {MostExpensiveNecklace.NecklaceID}, Nr Of Pearls: {MostExpensiveNecklace.Pearls.Count()}, Price: {MostExpensiveNecklace.Pearls.Sum(p => p.Price):C2}");

                Console.WriteLine($"Most expensive Pearls");
                foreach (var pearl in pearls.OrderByDescending(p => p.Price).Take(5))
                {
                    Console.WriteLine($"Pearl: {pearl}, Price: {pearl.Price:C2}, in Necklace {pearl.Necklace.NecklaceID}");
                }
            }
        }
        private static async Task QueryDatabasePearlCRUDEAsync()
        {
            Console.WriteLine("\n\nQuery Database Pearl CRUDE Async");
            Console.WriteLine("--------------------");
            using (var db = new NecklaceDbContext(_optionsBuilder.Options))
            {
                var _repo = new PearlRepository(db);

                Console.WriteLine("Testing ReadAllAsync()");
                var allPearls = await _repo.ReadAllAsync();
                Console.WriteLine($"Nr of Pearls {allPearls.Count()}");
                Console.WriteLine($"\nFirst 5 pearls");
                allPearls.Take(5).Print();


                Console.WriteLine("\nTesting ReadAsync()");
                var lastPearl = allPearls.Last();
                var lastPearl2 = await _repo.ReadAsync(lastPearl.PearlID);
                Console.WriteLine($"Last Pearl.\n{lastPearl}");
                Console.WriteLine($"Read Pearl with PearlID == Last Pearl\n{lastPearl2}");
                if (lastPearl == lastPearl2)
                    Console.WriteLine("Pearls Equal");
                else
                    Console.WriteLine("ERROR: Pearls not equal");


                Console.WriteLine("\nTesting UpdateAsync()");
                var (c, t) = (lastPearl2.Color, lastPearl2.Type);
                
                //Change properties
                (lastPearl2.Color, lastPearl2.Type) = (PearlColor.White, PearlType.FreshWater);
                var lastPearl3 = await _repo.UpdateAsync(lastPearl2);
                Console.WriteLine($"Last Pearl with updated properties.\n{lastPearl2}");

                if ((lastPearl2.Color == lastPearl3.Color) && (lastPearl2.Type == lastPearl3.Type))
                {
                    Console.WriteLine("Pearl Updated");
                    (lastPearl3.Color, lastPearl3.Type) = (c, t);

                    lastPearl3 = await _repo.UpdateAsync(lastPearl3);
                    Console.WriteLine($"Last Pearl with restored properties.\n{lastPearl3}");
                }
                else
                    Console.WriteLine("ERROR: Pearl not updated");

/* Cannot create individual pearls in EFC due to foreign Key
                Console.WriteLine("\nTesting CreateAsync()");
                var newPearl1 = Pearl.Factory.CreateRandomPearl();
                var newPearl2 = await _repo.CreateAsync(newPearl1);
                var newPearl3 = await _repo.ReadAsync(newPearl2.PearlID);

                Console.WriteLine($"Pearl created.\n{newPearl1}");
                Console.WriteLine($"Pearl Inserted in Db.\n{newPearl2}");
                Console.WriteLine($"Pearl ReadAsync from Db.\n{newPearl3}");

                if (newPearl1.Equals(newPearl2) && newPearl1.Equals(newPearl3))
                    Console.WriteLine("Pearls Equal");
                else
                    Console.WriteLine("ERROR: Pearls not equal");


                Console.WriteLine("\nTesting DeleteAsync()");
                var delPearl1 = await _repo.DeleteAsync(newPearl1.PearlID);
                Console.WriteLine($"Pearl to delete.\n{newPearl1}");
                Console.WriteLine($"Deleted Pearl.\n{delPearl1}");

                if (delPearl1 != null && delPearl1 == newPearl1)
                    Console.WriteLine("Pearls Equal");
                else
                    Console.WriteLine("ERROR: Pearls not equal");

                var DelCust2 = await _repo.ReadAsync(delPearl1.PearlID);
                if (DelCust2 != null)
                    Console.WriteLine("ERROR: Pearl not removed");
                else
                    Console.WriteLine("Pearl confirmed removed from Db");
*/
            }
        }

        private static async Task QueryDatabaseNecklaceCRUDEAsync()
        {
            Console.WriteLine("\n\nQuery Database Necklace CRUDE Async");
            Console.WriteLine("--------------------");
            using (var db = new NecklaceDbContext(_optionsBuilder.Options))
            {
                var _repo = new NecklaceRepository(db);

                Console.WriteLine("Testing ReadAllAsync()");
                var allNecklaces = await _repo.ReadAllAsync();
                Console.WriteLine($"Nr of Necklaces {allNecklaces.Count()}");

                Console.WriteLine("\nTesting ReadAsync()");
                var lastNecklace = allNecklaces.Last();
                var lastNecklace2 = await _repo.ReadAsync(lastNecklace.NecklaceID);
                Console.WriteLine($"Last Necklace. NecklaceId: {lastNecklace.NecklaceID}");
                Console.WriteLine($"Read Necklace with NecklaceID == Last Necklace. NecklaceId: {lastNecklace2.NecklaceID}");
                if (lastNecklace == lastNecklace2)
                    Console.WriteLine("Necklaces Equal");
                else
                    Console.WriteLine("ERROR: Necklaces not equal");

/*  Decision on how to update an Necklace
                Console.WriteLine("\nTesting UpdateAsync()");
                var (c, t) = (lastNecklace2.Color, lastNecklace2.Type);

                //Change properties
                (lastNecklace2.Color, lastNecklace2.Type) = (PearlColor.White, PearlType.FreshWater);
                var lastPearl3 = await _repo.UpdateAsync(lastNecklace2);
                Console.WriteLine($"Last Pearl with updated properties.\n{lastNecklace2}");

                if ((lastNecklace2.Color == lastPearl3.Color) && (lastNecklace2.Type == lastPearl3.Type))
                {
                    Console.WriteLine("Pearl Updated");
                    (lastPearl3.Color, lastPearl3.Type) = (c, t);

                    lastPearl3 = await _repo.UpdateAsync(lastPearl3);
                    Console.WriteLine($"Last Pearl with restored properties.\n{lastPearl3}");
                }
                else
                    Console.WriteLine("ERROR: Pearl not updated");
*/

                Console.WriteLine("\nTesting CreateAsync()");
                var newNecklace2 = await _repo.CreateAsync(Necklace.Factory.CreateRandomNecklace(25));
                var newNecklace3 = await _repo.ReadAsync(newNecklace2.NecklaceID);

                Console.WriteLine($"Necklace Inserted in Db. NecklaceId: {newNecklace2.NecklaceID}");
                Console.WriteLine($"Necklace ReadAsync from Db. NecklaceId: {newNecklace3.NecklaceID}");

                if (newNecklace2.NecklaceID == newNecklace3.NecklaceID)
                    Console.WriteLine("NecklaceId Equal");
                else
                    Console.WriteLine("ERROR: NecklaceId not equal");


                Console.WriteLine("\nTesting DeleteAsync()");
                var delNecklace1 = await _repo.DeleteAsync(newNecklace2.NecklaceID);
                Console.WriteLine($"Necklace ReadAsync from Db. NecklaceId:  {newNecklace2.NecklaceID}");
                Console.WriteLine($"Deleted Necklace.NecklaceId: {newNecklace2.NecklaceID}");

                if (delNecklace1 != null && delNecklace1.NecklaceID == newNecklace2.NecklaceID)
                    Console.WriteLine("NecklaceId Equal");
                else
                    Console.WriteLine("ERROR: NecklaceId not equal");

                var delNecklace2 = await _repo.ReadAsync(delNecklace1.NecklaceID);
                if (delNecklace2 != null)
                    Console.WriteLine("ERROR: Necklace not removed");
                else
                    Console.WriteLine("Necklace confirmed removed from Db");
            }
        }
        #endregion
    }
}
