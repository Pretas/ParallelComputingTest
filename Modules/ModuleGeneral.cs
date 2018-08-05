using System;
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
        public Projector Pj;
        int UnitSeed;
    }

    public interface ICommActions
    {
        int GetCount(SeedIndex siAft);        
    }

    public class NetModule
    {
        public ICommActions Comm;
        public NetComponents NetC = new NetComponents();
        
        public bool IsDoneLoadingInput = false;
        public bool HasSeedFrom = true;

        public int UnitSeedBase;
        public int LayerCount;
        public int LayerNo;
        public int ConnCount;

        private readonly object LockObj = new object();

        public NetModule(NetComponents nc, ICommActions comm, int unitSeedBase, int layerCnt, int layerNo, int connCnt)
        {
            NetC = nc;
            Comm = comm;
            UnitSeedBase = unitSeedBase;
            LayerCount = layerCnt;
            LayerNo = layerNo;
            ConnCount = connCnt;
        }

        public void DoHeadJob(Socket sock)
        {
            int unitSeed = UnitSeedBase * Convert.ToInt32(Math.Pow(Convert.ToDouble(ConnCount), Convert.ToDouble(LayerCount) - Convert.ToDouble(LayerNo) - 1));

            Comm.LoadInputFromDB(ref NetC);
            Comm.ListUpSeed(ref NetC);
            lock (LockObj) IsDoneLoadingInput = true;

            while (Comm.GetCount(NetC.SiTotal) > 0 ||
                Comm.GetCount(NetC.SiBef) > 0 ||
                Comm.GetCount(NetC.SiAllo) > 0 ||
                Comm.GetCount(NetC.SiAft) > 0)
            {
                if (Comm.GetCount(NetC.SiBef) < unitSeed || Comm.GetCount(NetC.SiTotal) > 0)
                {
                    SeedIndex siTemp = default(SeedIndex);
                    SeedContainer sConTemp = default(SeedContainer);
                    Comm.LoadSeedFromDB(NetC, ref siTemp, ref sConTemp);
                    lock (LockObj) Comm.PutSeed(siTemp, sConTemp, ref NetC);
                }

                if (Comm.GetCount(NetC.SiAft) >= UnitSeed ||
                    (Comm.GetCount(NetC.SiTotal) == 0 && Comm.GetCount(NetC.SiAft) > 0))
                {
                    SeedIndex siTemp = default(SeedIndex);
                    ResultContainer rConTemp = default(ResultContainer);
                    lock (LockObj) Comm.PullResult(ref NetC, ref siTemp, ref rConTemp);
                    ResultContainer rConMerged = default(ResultContainer);
                    Comm.MergeResult(rConTemp, ref rConMerged);
                    Comm.InsertResultToDB(siTemp, rConMerged);
                }

                NetC.HasSeedFrom = Comm.GetCount(NetC.SiTotal) > 0;
            }
        }

        public void DoUpperJob(Socket sock)
        {
            while (!NetC.IsDoneLoadingInput) { }
            Comm.SendInput(NetC);

            bool hasSeedTo = true;
            bool hasResultFrom = true;

            while (hasSeedTo || hasResultFrom)
            {
                bool ynSeed = SendReceive.ReceivePrimitive<bool>(sock);
                if (ynSeed)
                {
                    SeedIndex siTemp = default(SeedIndex);
                    SeedContainer sConTemp = default(SeedContainer);
                    lock (LockObj) Comm.PullSeed(ref NetC, ref siTemp, ref sConTemp);

                    bool ynSeed2 = Comm.GetCount(siTemp) > 0;
                    SendReceive.SendPrimitive(sock, ynSeed2);
                    if (ynSeed2)
                    {
                        Comm.SendSeed(ref siTemp, ref sConTemp);
                    }
                }

                bool ynResult = SendReceive.ReceivePrimitive<bool>(sock);
                if (ynResult)
                {
                    SeedIndex siTemp = default(SeedIndex);
                    ResultContainer rConTemp = default(ResultContainer);
                    Comm.ReceiveResult(ref siTemp, ref rConTemp);
                    lock (LockObj) Comm.PutResult(siTemp, rConTemp, ref NetC);
                }

                hasSeedTo = (NetC.HasSeedFrom || Comm.GetCount(NetC.SiBef) > 0);
                SendReceive.SendPrimitive(sock, hasSeedTo);
                hasResultFrom = SendReceive.ReceivePrimitive<bool>(sock);
            }
        }

        public void DoLowerJob(Socket sock)
        {
            Comm.ReceiveInput(ref NetC);
            lock (LockObj) NetC.IsDoneLoadingInput = true;

            bool hasResultTo = true;

            while (NetC.HasSeedFrom || hasResultTo)
            {
                bool ynSeed = UnitSeed - Comm.GetCount(NetC.SiBef) > 0;
                SendReceive.SendPrimitive(sock, ynSeed);

                if (ynSeed)
                {
                    bool ynSeed2 = SendReceive.ReceivePrimitive<bool>(sock);

                    if (ynSeed2)
                    {
                        SeedIndex siTemp = default(SeedIndex);
                        SeedContainer sConTemp = default(SeedContainer);
                        Comm.ReceiveSeed(ref siTemp, ref sConTemp);
                        lock (LockObj) Comm.PutSeed(siTemp, sConTemp, ref NetC);
                    }
                }

                bool ynResult = Comm.GetCount(NetC.SiAft) > UnitSeed ||
                    (!NetC.HasSeedFrom && Comm.GetCount(NetC.SiAft) > 0);
                SendReceive.SendPrimitive(sock, ynResult);

                if (ynResult)
                {
                    SeedIndex siTemp = default(SeedIndex);
                    ResultContainer rConTemp = default(ResultContainer);
                    lock (LockObj) Comm.PullResult(ref NetC, ref siTemp, ref rConTemp);
                    ResultContainer rConMerged = default(ResultContainer);
                    Comm.MergeResult(rConTemp, ref rConMerged);
                    Comm.SendResult(siTemp, rConMerged);
                }

                lock (LockObj) NetC.HasSeedFrom = SendReceive.ReceivePrimitive<bool>(sock);
                hasResultTo = Comm.GetCount(NetC.SiBef) > 0 ||
                    Comm.GetCount(NetC.SiAllo) > 0 ||
                    Comm.GetCount(NetC.SiAft) > 0;
                SendReceive.SendPrimitive(sock, hasResultTo);
            }
        }

        public void DoWorkerJob(Socket sock)
        {
            while (!NetC.IsDoneLoadingInput) { }

            Comm.InitProjector(ref NetC);
            
            while (NetC.HasSeedFrom)
            {
                if(Comm.GetCount(NetC.SiBef)>0)
                {
                    SeedIndex siTemp = default(SeedIndex);
                    SeedContainer sConTemp = default(SeedContainer);
                    lock (LockObj) Comm.PullSeed(ref NetC, ref siTemp, ref sConTemp);

                    ResultContainer rConTemp = default(ResultContainer);
                    Comm.Run(NetC, sConTemp, ref rConTemp);
                    lock (LockObj) Comm.PutResult(siTemp, rConTemp, ref NetC);
                } 
            }
        }
    }
}