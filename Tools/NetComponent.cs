using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class SeedManager
    {
        private static SeedContainer SeedManaged;
        private static object SyncLock = new object();

        private static SeedContainer GetSeedContainer()
        {
            // Support multithreaded applications through
            // 'Double checked locking' pattern which (once
            // the instance exists) avoids locking each
            // time the method is invoked
            if (SeedManaged == null)
            {
                lock (SyncLock)
                {
                    if (SeedManaged == null)
                    {
                        SeedManaged = new SeedContainer();
                    }
                }
            }

            return SeedManaged;
        }
        
        public void InsertSeedList(List<ICloneable> seedFromDB)
        {
            SeedManaged.SeedNotAllocated.AddRange(seedFromDB);
        }

        public List<ICloneable> AllocateSeed(int toNodeNo, int unit)
        {
            List<ICloneable> seeds = new List<ICloneable>();

            for (int i = 0; i < unit; i++)
            {
                ICloneable seed = (ICloneable)SeedManaged.SeedNotAllocated[i].Clone();
                seeds.Add(seed);
            }

            SeedManaged.SeedAllocated.Add(toNodeNo, seeds);

            return seeds;
        }

        public void DeleteFinishedWork(int toNodeNo)
        {
            SeedManaged.SeedAllocated.Remove(toNodeNo);
        }       
    }

    public class SeedContainer
    {
        public List<ICloneable> SeedNotAllocated = new List<ICloneable>();
        public Dictionary<int, List<ICloneable>> SeedAllocated = new Dictionary<int, List<ICloneable>>();
    }
}
