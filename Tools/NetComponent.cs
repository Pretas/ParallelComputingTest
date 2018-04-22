using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponents
{
    abstract public class InputManager
    {
        private static InputManager IM;
        protected InputManager() { }
        private static object SyncLock = new object();

        public static InputManager GetInputManager()
        { return null; }

        public InputContainer IC;
        //abstract public void Insert(InputContainer IC);
        abstract public void LoadInput();

        public bool CompleteLoadingYN = false;
    }

    abstract public class InputContainer { }

    abstract public class SeedLoader
    {
        // 예시
        // dic<int(polIndex), tuple<int(sNo), int(eNo)>> PolList : 초기화 때 세팅
        // dic<int(scnIndex), string(coeffScn)> ScnList : 초기화 때 세팅
        // int sPol, ePol, cPol
        // int sScn, eScn, cScn

        abstract public bool CheckLackOfSeed(); //시드 개수가 부족한지 체크
        abstract public SeedIndexCompart GetSeedRequired(SeedManager sm); //로딩할 시드에서 기존에 저장되어 있는 시드 이외의 것을 추림        
        abstract public SeedContainer LoadSeed(SeedIndexCompart sic, ref SeedManager sm); // 시드가 필요한지 체크, 그 중에 로드해야 할 시드만 고르고, 로딩하여 SeedManager에 저장, current 반영
    }

    abstract public class SeedManager
    {
        private static SeedManager SM;
        protected SeedManager() { }
        private static object SyncLock = new object();

        public static SeedManager GetSeedManager()
        { return null;        }

        public SeedContainer SC;
        public List<SeedIndex> SeedBef;
        public Dictionary<int, List<SeedIndex>> SeedAllocated;
        abstract public void Insert(SeedContainer sc);
        abstract public void RemoveSeedBef(List<SeedIndex> si);
        public bool CompleteLoadingYN = false;
    }

    abstract public class SeedContainer
    { }

    abstract public class SeedIndex
    {
        //abstract public bool IsSame(SeedIndex a);
    }

    abstract public class SeedIndexCompart
    { }

    abstract public class ResultContainer
    {
        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }

    abstract public class ResultManager
    {
        private static ResultManager RM;
        protected ResultManager() { }
        private static object SyncLock = new object();

        public static ResultManager GetResultManager()
        {
            return null;
        }
        public ResultContainer RC;
        public List<SeedIndex> ResStacked;

        public int SumUpPoint { get; set; }

        abstract public ResultContainer SumUp();
    }
    
    public class Communicator
    {
        private static Communicator Comm;
        protected Communicator() { }
        private static object SyncLock = new object();
        private static object SyncLock2 = new object();

        private SeedManager Sm;
        private ResultManager Rm;

        public static Communicator GetSingleton(SeedManager sm, ResultManager rm)
        {
            // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
            if (Comm == null)
            {
                lock (SyncLock)
                {
                    if (Comm == null)
                    {
                        Comm = new Communicator();
                        Comm.Sm = sm;
                        Comm.Rm = rm;
                    }
                }
            }

            return Comm;
        }

        public void Communicate(CommJobName jn, object input)
        {
            lock(SyncLock2)
            {
                if (jn == CommJobName.RegisterSeed)
                {
                    SeedIndex si = (SeedIndex)input;
                    Sm.SeedBef.Add(si);
                }
                else if (jn == CommJobName.ProcessResult)
                {
                    Rm.ResStacked.Clear();
                }
                else if (jn == CommJobName.AllocateSeed)
                {
                    KeyValuePair<int, List<SeedIndex>> seed = (KeyValuePair<int, List<SeedIndex>>)input;
                    Sm.SeedAllocated.Add(seed.Key, seed.Value);
                    Sm.RemoveSeedBef(seed.Value);
                }
                else if (jn == CommJobName.StackResult)
                {
                    KeyValuePair<int, List<SeedIndex>> seed = (KeyValuePair<int, List<SeedIndex>>)input;
                    Sm.SeedAllocated.Remove(seed.Key);
                    Rm.ResStacked.AddRange(seed.Value);
                }
                else if (jn == CommJobName.ReturnBackSeed)
                {
                    KeyValuePair<int, List<SeedIndex>> seed = (KeyValuePair<int, List<SeedIndex>>)input; Sm.SeedAllocated.Remove(seed.Key);
                    Sm.SeedAllocated.Remove(seed.Key);
                    Sm.SeedBef.AddRange(seed.Value);                    
                }
            }
        }
    }


    public enum CommJobName
    { RegisterSeed, ProcessResult, AllocateSeed, StackResult, ReturnBackSeed }

    public class TasksOnFrame
    {
        public void DoHeadJob(InputManager im, SeedLoader sl, SeedManager sm, ResultManager rm, ExceptionManager em, Communicator comm)
        {
            try
            {
                im.LoadInput();
                im.CompleteLoadingYN = true;

                bool completeYN = false;
                while (!completeYN)
                {
                    //Seed 로딩
                    if (sl.CheckLackOfSeed())
                    {
                        SeedIndexCompart sic = sl.GetSeedRequired(sm);
                        sl.LoadSeed(sic, ref sm);
                    }
                    // 결과물 DB에 전송
                    if(rm.ResStacked.Count >= rm.SumUpPoint)
                    {
                        // 집계
                        ResultContainer res = rm.SumUp();
                        {
                            rm.ResStacked.Clear();
                            rm.RC.Clear();
                        }

                        // 업로드
                        // sm, rm 삭제
                    }
                    
                }
                
        //  * InputManager im = InputManager.GetIM()
        //  * InputContainer ic = LoadInputFromDB()
        //  * im.Insert(ic)
        //  * im.CompleteLoadingYN = true
        //  * SeedLoadingState sls 생성
        //  *Result resBase 생성
        //  *While(!completeYN & em.Error1.Count < em.UpperCount)
        //    * LoadFromDB()
        //    * UpLoadToDB(ref resBase)
        //    * CheckError()
        //* Catch
        //  * em.Error0.Add
        //  * throw new Exception​

            }
            catch
            { }
            finally
            { }

        //    
        }
    }

    public class ExceptionManager
    {
        public bool type0YN = false; // Head or Lower의 에러가 발생시 true
        public List<int> type1Errors = new List<int>(); // Upper에러 발생시 코어이름 등재함
    }

    public class Comm
    {
        //public static object ShiftData(CommJobName jn, object source)
        //{
        //    object syncLock = new object();
        //    object returnData = new object();

        //    lock(syncLock)
        //    {
        //        if (jn == CommJobName.RegisterSeed)
        //        {
        //            List<SeedIndex> si = (List<SeedIndex>)source;
        //            SeedManager sm = SeedManager.GetSeedManager();
        //            sm.SeedBef.AddRange(si);
        //        }
        //        else if (jn == CommJobName.ProcessResult)
        //        {
        //            //집계될 인덱스를 RM.ResBef에서 제거  
        //            ResultManager rm = ResultManager.GetResultManager();
        //            SeedIndex[] res= new SeedIndex[rm.ResStacked.Count];
        //            rm.ResStacked.CopyTo(res);
        //            List<SeedIndex> resList = res.ToList();
        //            rm.ResStacked.Clear();                    

        //            //집계될 인덱스를 SM.SeedAllocated에서 제거
        //            SeedManager sm = SeedManager.GetSeedManager();
        //            foreach (SeedIndex si in resList)
        //            {
        //                foreach (KeyValuePair<int, List<SeedIndex>> item in sm.SeedAllocated)
        //                {
        //                    foreach (SeedIndex item2 in item.Value)
        //                    {
        //                        if (item2.IsSame(si))
        //                        {
        //                            item.Value.Remove(item2);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }

        //            returnData = resList;
        //        }
        //        else if (jn == CommJobName.AllocateSeed)
        //        { }
        //        else if (jn == CommJobName.StackResult)
        //        { }
        //        else if (jn == CommJobName.ReturnBackSeed)
        //        { }
        //    }

        //    return returnData;

        //}
           
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
