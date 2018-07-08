using NetComponentTest;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tools;

namespace Modules
{
    public abstract class InputContainer { }
    public abstract class SeedList { }
    public abstract class SeedContainer { }
    public abstract class SeedIndex { }
    public abstract class ResultContainer { }
    public abstract class Projector { }

    public class NetComponents
    {
        public InputContainer ICon;
        public SeedList Sl;
        public SeedContainer SCon;
        public SeedIndex SiTotal;
        public SeedIndex SiBef;
        public SeedIndex SiAllo;
        public SeedIndex SiAft;
        public ResultContainer RCon;
        public bool IsDoneLoadingInput = false;
        public bool IsInProcessFrom = true;
        public Projector Pj;
    }

    public interface ICommActions
    {
        // ********** Thread Safe Actions  
        void SetIsDoneLoadingInput(ref NetComponents nC, Role role);
        void LoadSeedFromDB(ref NetComponents nc);
        void InsertResultToDB(ref NetComponents nc);
        void SendSeed(ref NetComponents nc);
        void ReceiveResult(ref NetComponents nc);        
        void ReceiveSeed(ref NetComponents nc);
        void SendResult(ref NetComponents nc);
        void AcheiveSeed(ref NetComponents nc);
        void PutResult(ref NetComponents nc);
        void SetIsInProcessFrom(ref NetComponents nC, Role role);

        // ********** Other Actions
        void ListUpSeed(ref NetComponents nc);
        void sendInput(NetComponents nc);
        void ReceiveInput(ref NetComponents nc);
        int GetCount(SeedIndex si);        
        bool GetIsInProcess(NetComponents nc, Role role);
        void ProjectionInit(NetComponents nc);
        void ProjectionExecute(ref NetComponents nc);
        void LoadInputFromDB(ref NetComponents nc);
    }

    public class NetModule
    {
        public NetComponents NC = new NetComponents();
        public ICommActions Comm;
        public int UnitSeed;
        public int LayerNo;

        public NetModule(NetComponents nc, ICommActions comm, int us, int ln)
        {
            Comm = comm;
            NC = nc;
            UnitSeed = us;
            LayerNo = ln;
        }

        public void DoHeadJob(Socket sock)
        {
            Comm.LoadInputFromDB(ref NC);            
            Comm.ListUpSeed(ref NC);
            ThreadSafeActions(CommJobName.SetIsDoneLoadingInput, Role.Head);

            bool isInProcess = true;
            while (isInProcess)
            {
                ThreadSafeActions(CommJobName.LoadSeedFromDB, Role.Head);
                ThreadSafeActions(CommJobName.InsertResultToDB, Role.Head);
                ThreadSafeActions(CommJobName.SetIsInProcessFrom, Role.Head);
                isInProcess = Comm.GetIsInProcess(NC, Role.Head);
            }
        }

        public void DoUpperJob(Socket sock)
        {
            while (!NC.IsDoneLoadingInput) { }
            Comm.sendInput(NC);

            bool isInProcess = true;
            while (isInProcess)
            {
                // 씨드 요청오고 보냄
                ThreadSafeActions(CommJobName.SendSeed, Role.Upper);
                ThreadSafeActions(CommJobName.ReceiveResult, Role.Upper);
                
                isInProcess = Comm.GetIsInProcess(NC, Role.Upper);
                SendReceive.SendGeneric(sock, isInProcess);
            }
        }

        public void DoLowerJob(Socket sock)
        {
            Comm.ReceiveInput(ref NC);
            ThreadSafeActions(CommJobName.SetIsDoneLoadingInput, Role.Lower);

            bool isInProcess = true;
            while (isInProcess)
            {
                ThreadSafeActions(CommJobName.ReceiveSeed, Role.Lower);
                ThreadSafeActions(CommJobName.SendResult, Role.Lower);
                ThreadSafeActions(CommJobName.SetIsInProcessFrom, Role.Lower);
                isInProcess = SendReceive.ReceiveGeneric<bool>(sock);                
            }            
        }

        public void DoWorkerJob(Socket sock)
        {
            while (!NC.IsDoneLoadingInput) { }

            Comm.ProjectionInit(NC);
            bool isInProcess = true;
            while (isInProcess)
            {
                // 씨드 요청오고 보냄
                ThreadSafeActions(CommJobName.AcheiveSeed, Role.Worker);
                Comm.ProjectionExecute(ref NC);
                ThreadSafeActions(CommJobName.PutResult, Role.Worker);

                isInProcess = Comm.GetIsInProcess(NC, Role.Worker);
            }
        }

        object Lock = new object();

        public void ThreadSafeActions(CommJobName jn, Role role)
        {
            lock (Lock)
            {
                if (jn == CommJobName.SetIsDoneLoadingInput) Comm.SetIsDoneLoadingInput(ref NC, role);
                else if (jn == CommJobName.LoadSeedFromDB) Comm.LoadSeedFromDB(ref NC);
                else if (jn == CommJobName.InsertResultToDB) Comm.InsertResultToDB(ref NC);
                else if (jn == CommJobName.SendSeed) Comm.SendSeed(ref NC);
                else if (jn == CommJobName.ReceiveResult) Comm.ReceiveResult(ref NC);
                else if (jn == CommJobName.ReceiveSeed) Comm.ReceiveSeed(ref NC);
                else if (jn == CommJobName.SendResult) Comm.SendResult(ref NC);
                else if (jn == CommJobName.AcheiveSeed) Comm.AcheiveSeed(ref NC);
                else if (jn == CommJobName.PutResult) Comm.PutResult(ref NC);
                else if (jn == CommJobName.SetIsInProcessFrom) Comm.SetIsInProcessFrom(ref NC, role);
            }
        }
    }

    public enum CommJobName
    {
        SetIsDoneLoadingInput, LoadSeedFromDB, InsertResultToDB, SendSeed, ReceiveResult, ReceiveSeed, SendResult, AcheiveSeed, PutResult, SetIsInProcessFrom        
    }

    public enum Role
    {
        Head, Upper, Lower, Worker
    }

}