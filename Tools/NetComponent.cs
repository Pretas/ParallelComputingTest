using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponents
{
    public class Singleton
    {
        protected static Singleton ST;
        protected Singleton() { }
        protected static object SyncLock = new object();

        public static Singleton GetSingleton()
        {
            // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
            if (ST == null)
            {
                lock (SyncLock)
                {
                    if (ST == null)
                    {
                        ST = new Singleton();
                    }
                }
            }

            return ST;
        }
    }

    public interface IInputManager
    {
        void LoadInput();
        InputContainer GetInput();
        void SetCompleteLoading();
        bool GetCompleteLoading();
    }

    abstract public class InputContainer { }

    public interface ISeedLoader
    {
        // 초기화, 불러와야 할 총 인풋리스트 저장
        void Init();
        // 시드 개수가 부족한지 체크
        bool CheckLackOfSeed();
        // 필요한 만큼 시드 가져오기(SeedContainer에 없는 부분만 추려서 가져오기), current에 반영
        Tuple<List<SeedIndex>, SeedContainer> LoadSeed();
        // 모든 시드가 로딩 되었으면 true
        bool IsFinished();
    }

    public interface ISeedManager
    {
        void InsertSeed(List<SeedIndex> si, SeedContainer sc);
        void AllocateSeed(int coreNo);
        void ReturnBackSeed(int coreNo);
        void RemoveSeedAllocated(int coreNo, List<SeedIndex> si);
        // 배분되기 전의 시드, 배분된 시드 모두가 비어있으면
        bool IsEmpty();
    }

    abstract public class SeedContainer
    { }

    abstract public class SeedIndex
    {
        //abstract public bool IsSame(SeedIndex a);
    }

    abstract public class SeedIndexCompart
    { }

    public interface IResultManager
    {
        void StackResult(List<SeedIndex> resIndex, List<Result> resReal);
        bool CheckNeedSumUp();
        void ClearResult();
        void SumUp();
        void UploadResult();
        // 쌓여있는 결과가 없으면
        bool IsEmpty();
    }

    public abstract class Result
    { }

    public enum CommJobName
    { InsertSeed, AllocateSeed, ReturnBackSeed, StackResult, UploadResult }

    public class Communicator : Singleton
    {
        private static object SyncLockComm = new object();

        //public static Communicator GetSingleton(ISeedManager sm, IResultManager rm)
        //{
        //    // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
        //    if (ST == null)
        //    {
        //        lock (SyncLock)
        //        {
        //            if (ST == null)
        //            {

        //                ST = new Singleton();
        //                this. ST.sm
        //                Comm.Sm = sm;
        //                Comm.Rm = rm;
        //            }
        //        }
        //    }

        //    return Comm;
        //}

        public void Communicate(CommJobName jn, ref ISeedManager sm, ref IResultManager rm, object input)
        {
            object res = new object();

            lock (SyncLockComm)
            {
                if (jn == CommJobName.InsertSeed)
                {
                    object[] input2 = (object[])input;
                    List<SeedIndex> si = (List<SeedIndex>)input2[0];
                    SeedContainer sc = (SeedContainer)input2[1];

                    sm.InsertSeed(si, sc);
                }
                else if (jn == CommJobName.AllocateSeed)
                {
                    int coreNo = (int)input;
                    sm.AllocateSeed(coreNo);
                }
                else if (jn == CommJobName.ReturnBackSeed)
                {
                    int coreNo = (int)input;
                    sm.ReturnBackSeed(coreNo);
                }
                else if (jn == CommJobName.StackResult)
                {
                    object[] input2 = (object[])input;
                    int coreNo = (int)input2[0];
                    List<SeedIndex> resIndex = (List<SeedIndex>)input2[1];
                    List<Result> resReal = (List<Result>)input2[2];

                    rm.StackResult(resIndex, resReal);
                    sm.RemoveSeedAllocated(coreNo, resIndex);
                }
                else if (jn == CommJobName.UploadResult)
                {
                    bool yn = rm.CheckNeedSumUp();
                    if (yn)
                    {
                        rm.SumUp();
                        rm.ClearResult();
                        rm.UploadResult();
                    }
                }
            }
        }
    }

    public class ExceptionManager
    {
        public bool HasType0Error = false; // Head or Lower의 에러가 발생시 true
        public List<int> type1Errors = new List<int>(); // Upper에러 발생시 코어이름 등재함
    }

    public class ActionsByRole
    {
        public void DoHeadJob(IInputManager im, ISeedLoader sl, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm)
        {
            try
            {
                im.LoadInput();
                im.SetCompleteLoading();

                sl.Init();

                bool isCompleted = false;
                while (!isCompleted)
                {
                    //디비에서 씨드로딩
                    if (sl.CheckLackOfSeed())
                    {
                        Tuple<List<SeedIndex>, SeedContainer> seedLoaded = sl.LoadSeed();
                        comm.Communicate(CommJobName.InsertSeed, ref sm, ref rm, seedLoaded);
                    }

                    //결과를 디비에 업로드
                    comm.Communicate(CommJobName.UploadResult, ref sm, ref rm, null);

                    //Lower들에게 에러 났는지 체크
                    if (em.type1Errors.Count > 0)
                    {
                        foreach (int coreNo in em.type1Errors)
                        {
                            comm.Communicate(CommJobName.ReturnBackSeed, ref sm, ref rm, coreNo);
                        }

                        em.type1Errors.Clear();
                    }

                    //seed 모두 로딩되면, sm.Allo 비었으면, sm.NotAllo 비었으면, rm.Finish이면
                    isCompleted = sl.IsFinished() && sm.IsEmpty() && rm.IsEmpty();
                }
            }
            catch
            {
                em.HasType0Error = true;
                throw new Exception();
            }
        }

        public void DoUpperJob(IInputManager im, ISeedLoader sl, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, System.Net.Sockets.Socket lowerSock)
        {
            try
            {
                // 인풋 전달
                while (true)
                {
                    if (im.GetCompleteLoading())
                    {
                        Tools.SendReceive.SendGeneric<bool>(lowerSock, true);
                        InputContainer input = im.GetInput();
                        Tools.SendReceive.SendGeneric<InputContainer>(lowerSock, input);
                        break;
                    }
                }

                // 시드 전송
                while (true)
                {
                    bool isNeedSeed = (bool)Tools.SendReceive.ReceiveGeneric<bool>(lowerSock);
                    if (!sm.IsEmpty()) { }
                }


                //while               
                //  시드 요청 받음
                //  시드 있는지 전달
                //  시드 있으면
                //    시드 전달
                //    시드 state 변경
                //  결과 전달받을 것 있는지 받음
                //  전달받을 것이 있다면
                //    결과 받음
                //    결과 저장
                //  sl의 로딩 끝 & sm에서 끝 전송            


            }
            catch
            { }

        }
    }
}



        //public class Comm
        //{
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