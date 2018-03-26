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
        private SeedContainer Sc = new SeedContainer();
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
                
        public void InsertSeed(Dictionary<int, List<ICloneable>> seedFrom, bool completeLoadingYN)
        {
            lock (SyncLock2)
            {
                foreach (var item in seedFrom)
                {
                    Sc.SeedNotAllocated.Add(item.Key, item.Value);
                }
                                
                CompleteLoadingYN = completeLoadingYN;
            }
        }

        public Dictionary<int, List<ICloneable>> AllocateSeed(int toNodeNo, int seedGroupIndex)
        {
            lock (SyncLock3)
            {
                List<ICloneable> seeds;
                Sc.SeedNotAllocated.TryGetValue(seedGroupIndex, out seeds);

                //dic<씨드그룹번호, 씨드그룹> 로딩
                Dictionary<int, List<ICloneable>> temp;                
                Sc.SeedAllocated.TryGetValue(toNodeNo, out temp);
                //씨드그룹 옮김
                Sc.SeedAllocated.Add(toNodeNo, temp);

                return temp;
            }
        }

        public void DeleteFinishedWork(int toNodeNo)
        {
            lock(SyncLock4)
            {
                //SeedManaged.SeedAllocated.Remove(toNodeNo);
            }          
        }

        public void ReturnBackSeed(int nodeNo)
        {
            lock (SyncLock5)
            {
                //foreach (ICloneable item in SeedManaged.SeedAllocated[nodeNo])
                //{
                //    ICloneable itemCopied = (ICloneable)item.Clone();
                //    SeedManaged.SeedNotAllocated.Add(itemCopied);
                //    SeedManaged.SeedAllocated[nodeNo].Remove(item);
                //}
            }
        }
    }

    public class SeedContainer
    {
        public Dictionary<int, List<ICloneable>> SeedNotAllocated = new Dictionary<int, List<ICloneable>>();
        public Dictionary<int, Dictionary<int, List<ICloneable>>> SeedAllocated = new Dictionary<int, Dictionary<int, List<ICloneable>>>();
        public List<int> seedFinished = new List<int>();
    }
}
