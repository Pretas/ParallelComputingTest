using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponentTest
{
    public abstract class NetComponentFamily
    {
        public InputManager im;
        public DBConnector sl;
        public SeedManager sm;
        public IResultManager rm;
        public IProjector projector;
        //public InputContainer inputContainer;
        //public SeedIndex seedIndex;
        //public SeedIndexCompart sic;
        //public SeedContainer seedContainer;
        //public Result result;
        //public ProjectionData pjd;
    }
        
    public abstract class DBConnector
    {
        public SeedIndex totalSeedIndex { get; private set; }

        // 초기화, 불러와야 할 총 인풋리스트 저장
        public abstract void LoadInput(ref InputManager im);
        // 전체 시드인덱스 리스트 생성, totalSeedIndex에 영향
        public abstract void ListUpTotalSeedIndex();
        // 시드 개수가 부족한지 체크
        public abstract bool GetIsShortOfSeed(SeedManager sm, int unit);
        // 필요한 만큼 시드 가져오기(SeedContainer에 없는 부분만 추려서 가져오기), current에 반영
        public abstract void LoadSeed(ref SeedManager sm, int unit);
        // 산출결과를 디비에 입력
        public abstract void InsertResultToDB(ResultContainer result);
        // 모든 시드가 로딩 되었으면 true
        public abstract void SetIsFinished(bool yn);
        // 모든 시드가 로딩 되었는지 가져옴
        public abstract bool GetIsFinished();
    }

    public abstract class InputManager
    {
        public bool IsDoneLoading { get; private set; }
        public InputContainer InputData { get; private set; }

        // Lower가 Upper에게서 받은 인풋 저장할 때
        public abstract void InsertInput(InputContainer ic);
        // 인풋 로딩이 끝났을 때 True 입력
        public abstract void SetCompleteLoading();
    }



    public abstract class SeedManager
    {
        // Upper에서 받은 씨드 입력
        public abstract void InsertSeed(SeedIndex si, SeedContainer sc);
        // 특정 Lower에게 씨드인덱스 할당
        public abstract void PickUpAndAllocateSeed(int coreNo, int unit, out SeedIndex si);
        // 작업이 끝난 뒤 씨드인덱스 삭제
        public abstract void RemoveAllocatedSeed(int coreNo, SeedIndex si);
        // SeedContainer에 필요없는 씨드 삭제
        public abstract void RearrangeSeedContainer();
        // Lower에 에러났을 때 할당된 씨드인덱스를 회수
        public abstract void ReturnBackSeed(int coreNo);
        // 아래층에서 요청받은 씨드
        public abstract SeedContainer GetSeedRequiredFromLowerLayer(SeedIndexCompart sic);
        // 위층에서 씨드가 더이상 있는지 없는지 입력
        public abstract void SetIsMoreSeedFromUpperLayer(bool isFinished);
        // 위층에서 씨드가 더이상 있는지 없는지 가져옴
        public abstract bool GetIsMoreSeedFromUpperLayer();
        // 배분되기 전의 시드, 배분된 시드 모두가 비어있으면
        public abstract bool IsEmpty();
        // 시드인덱스가 모자른지
        public abstract bool GetIsLackOfSeed();
        // 새로 받은 씨드인덱스 중에서 씨드컨테이너에 없는 씨드인덱스 추림(씨드컨테이너 받기 위해)
        public abstract SeedIndexCompart GetSeedIndexNotInSeedContainer(SeedIndex si);
        // 할당되지 않은 씨드인덱스 개수
        public abstract int GetSeedCountNotAllocated();
    }

    abstract public class SeedIndexCompart { }
    
    public interface IResultManager
    {
        // 런이 끝난 결과를 쌓기
        void StackResult(SeedIndex resIndex, ResultContainer resReal);
        // 런결과가 많이 쌓여서 집계를 할지 말지 체크 
        bool CheckNeedSumUp();
        // 집계되어 위층으로 보낸 후 비우기
        void ClearResult();
        // 집계함수
        void SumUp(out SeedIndex si, out ResultContainer res);
        // 집계한 후 위로 보내기
        void UploadResult();
        // 쌓여있는 결과가 없으면
        bool GetIsEmpty();
    }


    public class ExceptionManager
    {
        // Head or Lower의 에러가 발생시 true
        public bool HasType0Error = false;
        // Upper에러 발생시 코어이름 등재함
        public List<int> type1Errors = new List<int>(); 
    }

    public interface IProjector
    {
        // 인풋, 런함수 세팅
        void Init(InputContainer ic, Func<InputContainer, SeedContainer, ProjectionData> funcRun);
        // 런 실행
        ResultContainer Execute();
        // 단건씩 런 실행 후 결과 집계
        void SumUp(ProjectionData pjd, ref ResultContainer baseResult);
    }

    public abstract class ProjectionData { }
    
    public enum CommJobName
    { InsertSeed, AllocateSeed, ReturnBackSeed, StackResult, UploadResult }

    public class Communicator
    {
        // ***** 싱글턴 구현부
        protected static Communicator ST;
        protected Communicator() { }
        protected static object SyncLock = new object();

        public static Communicator GetSingleton()
        {
            if (ST == null) lock (SyncLock) if (ST == null) ST = new Communicator();
            return ST;
        }
        // *****

        private static object SyncLockComm = new object();

        public object Communicate(CommJobName jn, ref SeedManager sm, ref IResultManager rm, object input)
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
                    Tuple<int, int> input2 = (Tuple<int, int>)input;
                    int coreNo = input2.Item1;
                    int unit = input2.Item2;
                    SeedIndex si;
                    sm.PickUpAndAllocateSeed(coreNo, unit, out si);
                    res = si;
                }
                else if (jn == CommJobName.ReturnBackSeed)
                {
                    int coreNo = (int)input;
                    sm.ReturnBackSeed(coreNo);
                }
                else if (jn == CommJobName.StackResult)
                {
                    Tuple<int, SeedIndex, ResultContainer> input2 = (Tuple<int, SeedIndex, ResultContainer>)input;
                    int coreNo = input2.Item1;
                    SeedIndex resIndex = input2.Item2;
                    ResultContainer resReal = input2.Item3;

                    rm.StackResult(resIndex, resReal);
                    sm.RemoveAllocatedSeed(coreNo, resIndex);
                    sm.RearrangeSeedContainer();
                }
                else if (jn == CommJobName.UploadResult)
                {
                    SeedIndex si;
                    ResultContainer sumUpRes;
                    rm.SumUp(out si, out sumUpRes);
                    rm.ClearResult();
                    res = Tuple.Create(si, sumUpRes);
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