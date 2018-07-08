using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponentTest
{
    public class ActionOfHead
    {
        private int JobUnitNo;

        private ActionOfHead() { }

        public ActionOfHead(int jobUnitNo)
        { JobUnitNo = jobUnitNo; }
        
        public void DoJob(InputManager im, DBConnector conn, SeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm)
        {
            try
            {
                im.LoadInput();
                im.SetCompleteLoading();

                conn.Init();

                bool isCompleted = false;
                while (!isCompleted)
                {
                    //디비에서 씨드로딩
                    if (conn.GetIsLackOfSeed())
                    {
                        SeedIndex si;
                        SeedContainer sc;
                        conn.LoadSeed(out si, out sc);
                        comm.Communicate(CommJobName.InsertSeed, ref sm, ref rm, Tuple.Create(si, sc));
                    }

                    //결과를 디비에 업로드
                    if (rm.CheckNeedSumUp())
                    {
                        Tuple<SeedIndex, ResultContainer> sumUpRes = (Tuple<SeedIndex, ResultContainer>)comm.Communicate(CommJobName.UploadResult, ref sm, ref rm, null);
                        conn.InsertResultToDB(sumUpRes.Item2);
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
                    isCompleted = conn.GetIsFinished() && sm.IsEmpty();
                }
            }
            catch
            {
                em.HasType0Error = true;
                throw new Exception();
            }
        }
    }

    public class ActionOfUpper
    {
        private int LayerNo; // 0, 1, 2, 3, ...
        private int ThisCoreNo;
        private int JobUnitNo;
        private System.Net.Sockets.Socket LowerSock;

        private ActionOfUpper() { }

        public ActionOfUpper(int layerNo, int thisCoreNo, int jobUnitNo, System.Net.Sockets.Socket lowerSock)
        {
            LayerNo = layerNo; ThisCoreNo = thisCoreNo; JobUnitNo = jobUnitNo; LowerSock = lowerSock;
        }

        public void DoJob(InputManager im, SeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm)
        {
            try
            {
                // 인풋 전달
                while (true)
                {
                    if (im.GetCompleteLoading())
                    {
                        Tools.SendReceive.SendGeneric(LowerSock, true);
                        InputContainer input = im.GetInput();
                        Tools.SendReceive.SendGeneric<InputContainer>(LowerSock, input);
                        break;
                    }
                    else
                    { Tools.SendReceive.SendGeneric(LowerSock, false);
                    }
                }

                while (true)
                {
                    // 시드 전송
                    bool isLackOfSeed = Tools.SendReceive.ReceiveGeneric<bool>(LowerSock);
                    if (isLackOfSeed)
                    {
                        if (sm.GetSeedCountNotAllocated() > 0)
                        {
                            SeedIndex si = (SeedIndex)comm.Communicate(CommJobName.AllocateSeed, ref sm, ref rm, Tuple.Create(ThisCoreNo, JobUnitNo));
                            Tools.SendReceive.SendGeneric(LowerSock, true);
                            Tools.SendReceive.SendGeneric(LowerSock, si);
                            SeedIndexCompart sic = Tools.SendReceive.ReceiveGeneric<SeedIndexCompart>(LowerSock);
                            SeedContainer sc = sm.GetSeedRequiredFromLowerLayer(sic);
                            Tools.SendReceive.SendGeneric(LowerSock, sc);
                        }
                        else
                        { Tools.SendReceive.SendGeneric(LowerSock, false); }
                    }

                    // 결과 받기
                    bool isResult = Tools.SendReceive.ReceiveGeneric<bool>(LowerSock);
                    if (isResult)
                    {
                        SeedIndex si = Tools.SendReceive.ReceiveGeneric<SeedIndex>(LowerSock);
                        ResultContainer res = Tools.SendReceive.ReceiveGeneric<ResultContainer>(LowerSock);
                        object resSet = Tuple.Create(ThisCoreNo, si, res);
                        comm.Communicate(CommJobName.StackResult, ref sm, ref rm, resSet);
                    }

                    // loop 탈출 : (더 이상 위에서 받을 씨드가 없음 and 씨드통에 내용 없음) or Type0에러 발생
                    bool isFinished = (!sm.GetIsMoreSeedFromUpperLayer() && sm.IsEmpty()) || em.HasType0Error;
                    Tools.SendReceive.SendGeneric(LowerSock, isFinished);
                    if (isFinished) break;
                }
            }
            catch
            {
                em.type1Errors.Add(ThisCoreNo);
            }
        }
    }

    public class ActionOfLower
    {
        private int LayerNo; // 0, 1, 2, 3, ...
        private int ThisCoreNo;
        private int JobUnitNo;
        private System.Net.Sockets.Socket UpperSock;

        private ActionOfLower() { }

        public ActionOfLower(int layerNo, int thisCoreNo, int jobUnitNo, System.Net.Sockets.Socket upperSock)
        {
            LayerNo = layerNo; ThisCoreNo = thisCoreNo; JobUnitNo = jobUnitNo; UpperSock = upperSock;
        }

        public void DoJob(InputManager im, SeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm)
        {
            try
            {
                // 인풋 전달 받음
                while (true)
                {
                    bool isPossibleInput = Tools.SendReceive.ReceiveGeneric<bool>(UpperSock);
                    if (isPossibleInput)
                    {
                        InputContainer ic = Tools.SendReceive.ReceiveGeneric<InputContainer>(UpperSock);
                        im.InsertInput(ic);
                        break;
                    }
                }

                while (true)
                {
                    // 시드 받기
                    bool isLackOfSeed = sm.GetIsLackOfSeed();
                    Tools.SendReceive.SendGeneric(UpperSock, isLackOfSeed);
                    if (isLackOfSeed)
                    {
                        bool isPossible = Tools.SendReceive.ReceiveGeneric<bool>(UpperSock);
                        if (isPossible)
                        {
                            SeedIndex si = Tools.SendReceive.ReceiveGeneric<SeedIndex>(UpperSock);
                            SeedIndexCompart sic = sm.GetSeedIndexNotInSeedContainer(si);
                            Tools.SendReceive.SendGeneric(UpperSock, sic);
                            SeedContainer sc = Tools.SendReceive.ReceiveGeneric<SeedContainer>(UpperSock);
                            comm.Communicate(CommJobName.InsertSeed, ref sm, ref rm, Tuple.Create(si, sc));
                        }
                    }

                    // 결과 주기
                    bool isResult = rm.CheckNeedSumUp();
                    Tools.SendReceive.SendGeneric(UpperSock, isResult);
                    if (isResult)
                    {
                        Tuple<SeedIndex, ResultContainer> sumUpRes = (Tuple<SeedIndex, ResultContainer>)comm.Communicate(CommJobName.UploadResult, ref sm, ref rm, null);
                        Tools.SendReceive.SendGeneric(UpperSock, sumUpRes.Item1);
                        Tools.SendReceive.SendGeneric(UpperSock, sumUpRes.Item2);
                    }

                    //Upper들에게 에러 났는지 체크
                    if (em.type1Errors.Count > 0)
                    {
                        foreach (int coreNo in em.type1Errors)
                        {
                            comm.Communicate(CommJobName.ReturnBackSeed, ref sm, ref rm, coreNo);
                        }

                        em.type1Errors.Clear();
                    }

                    //seed 모두 로딩되면, sm.Allo 비었으면, sm.NotAllo 비었으면, rm.Finish이면                    
                    bool isFinished = Tools.SendReceive.ReceiveGeneric<bool>(UpperSock);
                    if (isFinished) sm.SetIsMoreSeedFromUpperLayer(false);
                    if (isFinished == sm.IsEmpty()) break;
                    else throw new Exception("위와 아래가 동기화가 되지 않았습니다");
                }
            }
            catch
            {
                em.HasType0Error = true;
            }
        }
    }

    public class ActionOfWorker
    {
        private int LayerNo; // 0, 1, 2, 3, ...
        private int ThisCoreNo;
        private int JobUnitNo;

        private ActionOfWorker() { }

        public ActionOfWorker(int layerNo, int thisCoreNo, int jobUnitNo)
        {
            LayerNo = layerNo; ThisCoreNo = thisCoreNo; JobUnitNo = jobUnitNo;
        }

        public void DoJob(InputManager im, SeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, IProjector projector, Func<InputContainer, SeedContainer, ProjectionData> funcRun)
        {
            try
            {
                //인풋 다 받을 때까지 대기
                while (true) if (im.GetCompleteLoading()) break;

                // 런 실행                
                projector.Init(im.GetInput(), funcRun);
                while (true)
                {
                    SeedIndex siList = (SeedIndex)comm.Communicate(CommJobName.AllocateSeed, ref sm, ref rm, Tuple.Create(ThisCoreNo, JobUnitNo));
                    ResultContainer res = projector.Execute();
                    comm.Communicate(CommJobName.StackResult, ref sm, ref rm, Tuple.Create(ThisCoreNo, siList, res));
                    if (!sm.GetIsMoreSeedFromUpperLayer() && sm.IsEmpty()) break;
                }
            }
            catch
            {
                em.type1Errors.Add(ThisCoreNo);
                throw new Exception();
            }
        }
    }
}
