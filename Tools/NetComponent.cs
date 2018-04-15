using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponents
{
    abstract public class InputContainer
    { }

    public class InputManager
    {
        private static InputManager IM;
        protected InputManager() { }
        private static object SyncLock = new object();

        public static InputManager GetInputManager()
        {
            // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
            if (IM == null)
            {
                lock (SyncLock)
                {
                    if (IM == null) { IM = new InputManager(); }
                }
            }

            return IM;
        }

        public InputContainer IC;
        public void Insert(InputContainer IC) { }
        public bool CompleteLoadingYN = false;
    }

    abstract public class SeedContainer
    { }

    abstract public class SeedIndex
    {
        abstract public bool IsSame(SeedIndex a);
    }

    public class SeedManager
    {
        private static SeedManager SM;
        protected SeedManager() { }
        private static object SyncLock = new object();

        public static SeedManager GetSeedManager()
        {
            // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
            if (SM == null)
            {
                lock (SyncLock)
                {
                    if (SM == null) { SM = new SeedManager(); }
                }
            }

            return SM;
        }

        public SeedContainer SC;
        public List<SeedIndex> SeedBef;
        public Dictionary<int, List<SeedIndex>> SeedAllocated;
        public void Insert(SeedContainer sc) { }
        public void Remove(SeedIndex si) { }        
        public bool CompleteLoadingYN = false;
    }

    abstract public class ResultContainer
    { }

    public class ResultManager
    {
        private static ResultManager RM;
        protected ResultManager() { }
        private static object SyncLock = new object();

        public static ResultManager GetResultManager()
        {
            // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
            if (RM == null)
            {
                lock (SyncLock)
                {
                    if (RM == null) { RM = new ResultManager(); }
                }
            }

            return RM;
        }
        public ResultContainer RC;
        public List<SeedIndex> ResStacked;
        public void SumUp(ref ResultContainer rbase) { }
    }

    public enum CommJobName
    { RegisterSeed, ProcessResult, AllocateSeed, StackResult, ReturnBackSeed }

    public class Comm
    {
        public static object ShiftData(CommJobName jn, object source)
        {
            object syncLock = new object();
            object returnData = new object();

            lock(syncLock)
            {
                if (jn == CommJobName.RegisterSeed)
                {
                    List<SeedIndex> si = (List<SeedIndex>)source;
                    SeedManager sm = SeedManager.GetSeedManager();
                    sm.SeedBef.AddRange(si);
                }
                else if (jn == CommJobName.ProcessResult)
                {
                    //집계될 인덱스를 RM.ResBef에서 제거  
                    ResultManager rm = ResultManager.GetResultManager();
                    SeedIndex[] res= new SeedIndex[rm.ResStacked.Count];
                    rm.ResStacked.CopyTo(res);
                    List<SeedIndex> resList = res.ToList();
                    rm.ResStacked.Clear();                    

                    //집계될 인덱스를 SM.SeedAllocated에서 제거
                    SeedManager sm = SeedManager.GetSeedManager();
                    foreach (SeedIndex si in resList)
                    {
                        foreach (KeyValuePair<int, List<SeedIndex>> item in sm.SeedAllocated)
                        {
                            foreach (SeedIndex item2 in item.Value)
                            {
                                if (item2.IsSame(si))
                                {
                                    item.Value.Remove(item2);
                                    break;
                                }
                            }
                        }
                    }

                    returnData = resList;
                }
                else if (jn == CommJobName.AllocateSeed)
                { }
                else if (jn == CommJobName.StackResult)
                { }
                else if (jn == CommJobName.ReturnBackSeed)
                { }
            }

            return returnData;

        }
           
    }



    //public class SeedManager
    //{
    //    private static SeedManager Sm;
    //    private SeedContainer Sc = new SeedContainer();
    //    private bool CompleteLoadingYN;

    //    private static object SyncLock = new object();
    //    private static object SyncLock2 = new object();
    //    private static object SyncLock3 = new object();
    //    private static object SyncLock4 = new object();
    //    private static object SyncLock5 = new object();

    //    protected SeedManager() { }

    //    public static SeedManager GetSeedManager()
    //    {
    //        // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
    //        if (Sm == null)
    //        {
    //            lock (SyncLock)
    //            {
    //                if (Sm == null)
    //                {
    //                    Sm = new SeedManager();
    //                }
    //            }
    //        }

    //        return Sm;
    //    }
                
    //    public void InsertSeed(Dictionary<int, List<ICloneable>> seedFrom, bool completeLoadingYN)
    //    {
    //        lock (SyncLock2)
    //        {
    //            foreach (var item in seedFrom)
    //            {
    //                Sc.SeedNotAllocated.Add(item.Key, item.Value);
    //            }
                                
    //            CompleteLoadingYN = completeLoadingYN;
    //        }
    //    }

    //    public Dictionary<int, List<ICloneable>> AllocateSeed(int toNodeNo, int seedGroupIndex)
    //    {
    //        lock (SyncLock3)
    //        {
    //            List<ICloneable> seeds;
    //            Sc.SeedNotAllocated.TryGetValue(seedGroupIndex, out seeds);

    //            //dic<씨드그룹번호, 씨드그룹> 로딩
    //            Dictionary<int, List<ICloneable>> temp;                
    //            Sc.SeedAllocated.TryGetValue(toNodeNo, out temp);
    //            //씨드그룹 옮김
    //            Sc.SeedAllocated.Add(toNodeNo, temp);

    //            return temp;
    //        }
    //    }

    //    public void DeleteFinishedWork(int toNodeNo)
    //    {
    //        lock(SyncLock4)
    //        {
    //            //SeedManaged.SeedAllocated.Remove(toNodeNo);
    //        }          
    //    }

    //    public void ReturnBackSeed(int nodeNo)
    //    {
    //        lock (SyncLock5)
    //        {
    //            //foreach (ICloneable item in SeedManaged.SeedAllocated[nodeNo])
    //            //{
    //            //    ICloneable itemCopied = (ICloneable)item.Clone();
    //            //    SeedManaged.SeedNotAllocated.Add(itemCopied);
    //            //    SeedManaged.SeedAllocated[nodeNo].Remove(item);
    //            //}
    //        }
    //    }
    

    //public class SeedContainer
    //{
    //    public Dictionary<int, List<ICloneable>> SeedNotAllocated = new Dictionary<int, List<ICloneable>>();
    //    public Dictionary<int, Dictionary<int, List<ICloneable>>> SeedAllocated = new Dictionary<int, Dictionary<int, List<ICloneable>>>();
    //    public List<int> seedFinished = new List<int>();
    //}
}
