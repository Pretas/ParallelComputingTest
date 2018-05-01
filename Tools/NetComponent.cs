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
        void InsertInput(InputContainer ic);
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
        List<SeedIndex> PickUpAndAllocateSeed(int coreNo);
        SeedContainer GetSeedRequired(SeedIndexCompart sic);
        void ReturnBackSeed(int coreNo);
        void RemoveSeedAllocated(int coreNo, List<SeedIndex> si);
        void SetIsFinished(bool isFinished);
        bool GetIsFinished();
        // 배분되기 전의 시드, 배분된 시드 모두가 비어있으면
        bool IsEmpty();
        bool GetIsLackOfSeed();
        SeedIndexCompart GetSeedNotInSeedContainer(List<SeedIndex> si);
    }

    abstract public class SeedContainer
    { }

    abstract public class SeedIndex
    { }

    abstract public class SeedIndexCompart
    { }

    public interface IResultManager
    {
        void StackResult(List<SeedIndex> resIndex, Result resReal);
        bool CheckNeedSumUp();
        void ClearResult();
        Tuple<List<SeedIndex>, Result> SumUp();
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

        public object Communicate(CommJobName jn, ref ISeedManager sm, ref IResultManager rm, object input)
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
                    List<SeedIndex> si = sm.PickUpAndAllocateSeed(coreNo);
                    res = si;
                }
                else if (jn == CommJobName.ReturnBackSeed)
                {
                    int coreNo = (int)input;
                    sm.ReturnBackSeed(coreNo);
                }
                else if (jn == CommJobName.StackResult)
                {
                    Tuple<int, List<SeedIndex>, Result> input2 = (Tuple<int, List<SeedIndex>, Result>)input;
                    int coreNo = input2.Item1;
                    List<SeedIndex> resIndex = input2.Item2;
                    Result resReal = input2.Item3;

                    rm.StackResult(resIndex, resReal);
                    sm.RemoveSeedAllocated(coreNo, resIndex);
                }
                else if (jn == CommJobName.UploadResult)
                {
                    Tuple<List<SeedIndex>, Result> sumUpRes = rm.SumUp();
                    rm.ClearResult();
                    res = sumUpRes;
                }

                return res;
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
        private int coreNo;

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
                    if (rm.CheckNeedSumUp())
                    {
                        comm.Communicate(CommJobName.UploadResult, ref sm, ref rm, null);
                    }                    

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
                    isCompleted = sl.IsFinished() && sm.IsEmpty();
                }
            }
            catch
            {
                em.HasType0Error = true;
                throw new Exception();
            }
        }

        public void DoUpperJob(IInputManager im, ISeedLoader sl, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, System.Net.Sockets.Socket lowerSock, int lowerCoreNo)
        {
            try
            {
                // 인풋 전달
                while (true)
                {
                    if (im.GetCompleteLoading())
                    {
                        Tools.SendReceive.SendGeneric(lowerSock, true);
                        InputContainer input = im.GetInput();
                        Tools.SendReceive.SendGeneric<InputContainer>(lowerSock, input);
                        break;
                    }
                    else
                    { Tools.SendReceive.SendGeneric(lowerSock, false); }
                }

                bool isFinished = false;
                while (!isFinished)
                {
                    // 시드 전송
                    bool isNeedSeed = Tools.SendReceive.ReceiveGeneric<bool>(lowerSock);
                    if (isNeedSeed)
                    {
                        List<SeedIndex> si = (List<SeedIndex>)comm.Communicate(CommJobName.AllocateSeed, ref sm, ref rm, this.coreNo);

                        if (si.Count > 0)
                        {
                            Tools.SendReceive.SendGeneric(lowerSock, true);
                            Tools.SendReceive.SendGeneric(lowerSock, si);
                            SeedIndexCompart sic = Tools.SendReceive.ReceiveGeneric<SeedIndexCompart>(lowerSock);
                            SeedContainer sc = sm.GetSeedRequired(sic);
                            Tools.SendReceive.SendGeneric(lowerSock, sc);
                        }
                        else
                        { Tools.SendReceive.SendGeneric(lowerSock, false); }
                    }

                    // 결과 받기
                    bool isResult = Tools.SendReceive.ReceiveGeneric<bool>(lowerSock);
                    if (isResult)
                    {
                        List<SeedIndex> si = Tools.SendReceive.ReceiveGeneric<List<SeedIndex>>(lowerSock);
                        Result res = Tools.SendReceive.ReceiveGeneric<Result>(lowerSock);
                        object resSet = Tuple.Create(lowerCoreNo, si, res);
                        comm.Communicate(CommJobName.StackResult, ref sm, ref rm, resSet);
                    }

                    // 더이상받을 씨드없고, 씨드통에 내용 없고, emType0 false
                    isFinished = (sm.GetIsFinished() && sm.IsEmpty()) || em.HasType0Error;
                    Tools.SendReceive.SendGeneric(lowerSock, isFinished);
                }
            }
            catch
            {
                em.type1Errors.Add(coreNo);
            }
        }

        public void DoLowerJob(IInputManager im, ISeedLoader sl, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, System.Net.Sockets.Socket upperSock)
        {
            try
            {
                // 인풋 전달 받음
                while (true)
                {
                    bool isPossibleInput = Tools.SendReceive.ReceiveGeneric<bool>(upperSock);
                    if (isPossibleInput)
                    {
                        InputContainer ic = Tools.SendReceive.ReceiveGeneric<InputContainer>(upperSock);
                        im.InsertInput(ic);
                        break;
                    }
                }

                bool isFinished = false;
                while (!isFinished)
                {
                    // 시드 받기
                    bool isLackOfSeed = sm.GetIsLackOfSeed();
                    Tools.SendReceive.SendGeneric(upperSock, isLackOfSeed);
                    if (isLackOfSeed)
                    {
                        bool isPossible = Tools.SendReceive.ReceiveGeneric<bool>(upperSock);
                        if (isPossible)
                        {
                            List<SeedIndex> si = Tools.SendReceive.ReceiveGeneric<List<SeedIndex>>(upperSock);
                            SeedIndexCompart sic = sm.GetSeedNotInSeedContainer(si);
                            Tools.SendReceive.SendGeneric(upperSock, sic);
                            SeedContainer sc = Tools.SendReceive.ReceiveGeneric<SeedContainer>(upperSock);
                            object[] input = new object[2] { si, sc };
                            comm.Communicate(CommJobName.InsertSeed, ref sm, ref rm, input);
                        }
                    }

                    // 결과 주기
                    bool isResult = rm.CheckNeedSumUp();
                    Tools.SendReceive.SendGeneric(upperSock, isResult);
                    if (isResult)
                    {
                        Tuple<List<SeedIndex>, Result> sumUpRes = (Tuple<List<SeedIndex>, Result>)comm.Communicate(CommJobName.UploadResult, ref sm, ref rm, null);
                        Tools.SendReceive.SendGeneric(upperSock, sumUpRes.Item1);
                        Tools.SendReceive.SendGeneric(upperSock, sumUpRes.Item2);
                    }
                                        
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
                    isFinished = Tools.SendReceive.ReceiveGeneric<bool>(upperSock);
                    if (isFinished) sm.SetIsFinished(true); ;
                    bool isFinishedLower = sm.IsEmpty();
                    if (isFinished != isFinishedLower)
                    { throw new Exception("위와 아래가 동기화가 되지 않았습니다"); }                      
                }
            }
            catch
            {
                em.type1Errors.Add(coreNo);
            }
        }

    }
}