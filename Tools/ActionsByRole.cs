using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetComponents
{
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
                    isCompleted = sl.GetIsFinished() && sm.IsEmpty();
                }
            }
            catch
            {
                em.HasType0Error = true;
                throw new Exception();
            }
        }

        public void DoUpperJob(IInputManager im, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, System.Net.Sockets.Socket lowerSock, int lowerCoreNo)
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

                while (true)
                {
                    // 시드 전송
                    bool isLackOfSeed = Tools.SendReceive.ReceiveGeneric<bool>(lowerSock);
                    if (isLackOfSeed)
                    {
                        List<SeedIndex> si = (List<SeedIndex>)comm.Communicate(CommJobName.AllocateSeed, ref sm, ref rm, this.coreNo);

                        if (si.Count > 0)
                        {
                            Tools.SendReceive.SendGeneric(lowerSock, true);
                            Tools.SendReceive.SendGeneric(lowerSock, si);
                            SeedIndexCompart sic = Tools.SendReceive.ReceiveGeneric<SeedIndexCompart>(lowerSock);
                            SeedContainer sc = sm.GetSeedRequiredFromLowerLayer(sic);
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

                    // loop 탈출 : (더 이상 위에서 받을 씨드가 없음 and 씨드통에 내용 없음) or Type0에러 발생
                    bool isFinished = (!sm.GetIsMoreSeedFromUpperLayer() && sm.IsEmpty()) || em.HasType0Error;
                    Tools.SendReceive.SendGeneric(lowerSock, isFinished);
                    if (isFinished) break;
                }
            }
            catch
            {
                em.type1Errors.Add(coreNo);
            }
        }

        public void DoLowerJob(IInputManager im, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, System.Net.Sockets.Socket upperSock)
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

                while (true)
                {
                    // 시드 받기
                    bool isLackOfSeed = sm.IsLackOfSeed();
                    Tools.SendReceive.SendGeneric(upperSock, isLackOfSeed);
                    if (isLackOfSeed)
                    {
                        bool isPossible = Tools.SendReceive.ReceiveGeneric<bool>(upperSock);
                        if (isPossible)
                        {
                            List<SeedIndex> si = Tools.SendReceive.ReceiveGeneric<List<SeedIndex>>(upperSock);
                            SeedIndexCompart sic = sm.GetSeedIndexNotInSeedContainer(si);
                            Tools.SendReceive.SendGeneric(upperSock, sic);
                            SeedContainer sc = Tools.SendReceive.ReceiveGeneric<SeedContainer>(upperSock);
                            object input = Tuple.Create(si, sc);
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
                    bool isFinished = Tools.SendReceive.ReceiveGeneric<bool>(upperSock);
                    if (isFinished) sm.SetIsMoreSeedFromUpperLayer(false);
                    if (isFinished == sm.IsEmpty()) break;
                    else throw new Exception("위와 아래가 동기화가 되지 않았습니다");
                }
            }
            catch
            {
                em.type1Errors.Add(coreNo);
            }
        }

        public void DoWorkerJob(IInputManager im, ISeedManager sm, IResultManager rm, ExceptionManager em, Communicator comm, IProjector projector, Func<InputContainer, SeedContainer, ProjectionData> funcRun)
        {
            try
            {
                //인풋 다 받을 때까지 대기
                while (true) if (im.GetCompleteLoading()) break;

                // 런 실행                
                projector.Init(im.GetInput(), funcRun);
                while (true)
                {
                    List<SeedIndex> siList = (List<SeedIndex>)comm.Communicate(CommJobName.AllocateSeed, ref sm, ref rm, this.coreNo);
                    Result res = projector.Execute();
                    comm.Communicate(CommJobName.StackResult, ref sm, ref rm, Tuple.Create(siList, res));
                    if (!sm.GetIsMoreSeedFromUpperLayer() && sm.IsEmpty()) break;
                }
            }
            catch
            {
                em.type1Errors.Add(coreNo);
                throw new Exception();
            }
        }
    }
}
