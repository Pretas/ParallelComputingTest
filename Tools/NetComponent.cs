using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class SeedManager
    {
        private static SeedManager Sm;
        private SeedContainer SeedManaged = new SeedContainer();

        private bool CompleteLoadingYN;

        private static object SyncLock = new object();
        private static object SyncLock2 = new object();
        private static object SyncLock3 = new object();
        private static object SyncLock4 = new object();
        private static object SyncLock5 = new object();

        protected SeedManager() { }

        public static SeedManager GetSeedManager()
        {
            // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
            if (Sm == null)
            {
                lock (SyncLock)
                {
                    if (Sm == null)
                    {
                        Sm = new SeedManager();
                    }
                }
            }

            return Sm;
        }
                
        public void InsertSeedFromDB(List<ICloneable> seedFromDB, bool completeLoadingYN)
        {
            lock (SyncLock2)
            {
                foreach (ICloneable item in seedFromDB)
                {
                    ICloneable itemCopied = (ICloneable)item.Clone();
                    SeedManaged.SeedNotAllocated.Add(itemCopied);
                }

                CompleteLoadingYN = completeLoadingYN;
            }
        }

        public List<ICloneable> AllocateSeed(int toNodeNo, int unit)
        {
            List<ICloneable> seeds = new List<ICloneable>();

            int alloNo = Math.Min(unit, SeedManaged.SeedNotAllocated.Count);

            lock(SyncLock3)
            {
                for (int i = 0; i < alloNo; i++)
                {
                    ICloneable seed = (ICloneable)SeedManaged.SeedNotAllocated[i].Clone();
                    seeds.Add(seed);
                    SeedManaged.SeedNotAllocated.RemoveAt(i);
                }

                SeedManaged.SeedAllocated.Add(toNodeNo, seeds);
            }

            return seeds;
        }

        public void DeleteFinishedWork(int toNodeNo)
        {
            lock(SyncLock4)
            {
                SeedManaged.SeedAllocated.Remove(toNodeNo);
            }          
        }

        public void ReturnBackSeed(int nodeNo)
        {
            lock (SyncLock5)
            {
                foreach (ICloneable item in SeedManaged.SeedAllocated[nodeNo])
                {
                    ICloneable itemCopied = (ICloneable)item.Clone();
                    SeedManaged.SeedNotAllocated.Add(itemCopied);
                    SeedManaged.SeedAllocated[nodeNo].Remove(item);
                }
            }
        }
    }

    public class SeedContainer
    {
        public List<ICloneable> SeedNotAllocated = new List<ICloneable>();

        public Dictionary<int, List<ICloneable>> SeedAllocated = new Dictionary<int, List<ICloneable>>();
    }
}
