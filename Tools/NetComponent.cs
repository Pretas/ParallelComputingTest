using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponents
{
    public abstract class ProjectionFamily
    {
        public IInputManager im;
        public ISeedLoader sl;
        public ISeedManager sm;
        public IResultManager rm;
        public IProjector projector;
        //public InputContainer inputContainer;
        //public SeedIndex seedIndex;
        //public SeedIndexCompart sic;
        //public SeedContainer seedContainer;
        //public Result result;
        //public ProjectionData pjd;
    }

    public interface IInputManager
    {
        // DB에서 인풋 로딩
        void LoadInput();
        // 사용처에서 인풋 가져갈 때
        InputContainer GetInput();
        // 인풋 로딩이 끝났을 때 True 입력
        void SetCompleteLoading();
        // 인풋 로딩이 끝났는지 알고 싶을 때
        bool GetCompleteLoading();
    }

    abstract public class InputContainer { }

    public interface ISeedLoader
    {
        // 초기화, 불러와야 할 총 인풋리스트 저장
        void Init();
        // 시드 개수가 부족한지 체크
        bool GetIsLackOfSeed();
        // 필요한 만큼 시드 가져오기(SeedContainer에 없는 부분만 추려서 가져오기), current에 반영
        void LoadSeed(out SeedIndex si, out SeedContainer sc);
        // 모든 시드가 로딩 되었으면 true
        bool IsFinished();
    }

    public interface ISeedManager
    {
        void InsertSeed(SeedIndex si, SeedContainer sc);
        void PickUpAndAllocateSeed(int coreNo, out SeedIndex si);
        void RemoveAllocatedSeed(int coreNo, SeedIndex si);
        void RearrangeSeedContainer();
        void ReturnBackSeed(int coreNo);

        SeedContainer GetSeed();
        SeedContainer GetSeedRequiredFromLowerLayer(SeedIndexCompart sic);
               
        void SetIsMoreSeedFromUpperLayer(bool isFinished);
        bool GetIsMoreSeedFromUpperLayer();
        // 배분되기 전의 시드, 배분된 시드 모두가 비어있으면
        bool IsEmpty();
        bool IsLackOfSeed();
        SeedIndexCompart GetSeedIndexNotInSeedContainer(SeedIndex si);
        int GetSeedCountNotAllocated();
    }

    abstract public class SeedContainer { }

    abstract public class SeedIndex { }

    abstract public class SeedIndexCompart { }

    public interface IResultManager
    {
        void StackResult(SeedIndex resIndex, Result resReal);
        bool CheckNeedSumUp();
        void ClearResult();
        void SumUp(out SeedIndex si, out Result res);
        void UploadResult();
        // 쌓여있는 결과가 없으면
        bool IsEmpty();
    }

    public abstract class Result { }

    public class ExceptionManager
    {
        public bool HasType0Error = false; // Head or Lower의 에러가 발생시 true
        public List<int> type1Errors = new List<int>(); // Upper에러 발생시 코어이름 등재함
    }

    public interface IProjector
    {
        void Init(InputContainer ic, Func<InputContainer, SeedContainer, ProjectionData> funcRun);
        Result Execute();
        void SumUp(ProjectionData pjd, ref Result baseResult);
    }

    public abstract class ProjectionData { }

    public enum CommJobName
    { InsertSeed, AllocateSeed, ReturnBackSeed, StackResult, UploadResult }

    public class Communicator
    {
        private static object SyncLockComm = new object();

        public object Communicate(CommJobName jn, ref ISeedManager sm, ref IResultManager rm, object input)
        {
            object res = new object();

            lock (SyncLockComm)
            {
                if (jn == CommJobName.InsertSeed)
                {
                    Tuple<SeedIndex, SeedContainer> input2 = (Tuple<SeedIndex, SeedContainer>)input;
                    SeedIndex si = input2.Item1;
                    SeedContainer sc = input2.Item2;

                    sm.InsertSeed(si, sc);
                }
                else if (jn == CommJobName.AllocateSeed)
                {
                    int coreNo = (int)input;
                    SeedIndex si;
                    sm.PickUpAndAllocateSeed(coreNo, out si);
                    res = si;
                }
                else if (jn == CommJobName.ReturnBackSeed)
                {
                    int coreNo = (int)input;
                    sm.ReturnBackSeed(coreNo);
                }
                else if (jn == CommJobName.StackResult)
                {
                    Tuple<int, SeedIndex, Result> input2 = (Tuple<int, SeedIndex, Result>)input;
                    int coreNo = input2.Item1;
                    SeedIndex resIndex = input2.Item2;
                    Result resReal = input2.Item3;

                    rm.StackResult(resIndex, resReal);
                    sm.RemoveAllocatedSeed(coreNo, resIndex);
                    sm.RearrangeSeedContainer();
                }
                else if (jn == CommJobName.UploadResult)
                {
                    SeedIndex si;
                    Result sumUpRes;
                    rm.SumUp(out si, out sumUpRes);
                    rm.ClearResult();
                    res = sumUpRes;
                }

                return res;
            }
        }
    }
    
    //public class Singleton
    //{
    //    protected static Singleton ST;
    //    protected Singleton() { }
    //    protected static object SyncLock = new object();

    //    public static Singleton GetSingleton()
    //    {
    //        // Support multithreaded applications through 'Double checked locking' pattern which (once the instance exists) avoids locking each time the method is invoked
    //        if (ST == null)
    //        {
    //            lock (SyncLock)
    //            {
    //                if (ST == null)
    //                {
    //                    ST = new Singleton();
    //                }
    //            }
    //        }

    //        return ST;
    //    }
    //}
}