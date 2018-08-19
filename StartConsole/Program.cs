using System;
using System.Collections.Generic;
using System.Threading;
using System.Data;
using Modules;
using System.Linq;
using System.Text;

namespace StartConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //SendDataTest(0);
            //Tools.PartitionTools2.Party();
            //Tools.PartitionTools2.SetCorePartition();
            PartyTest3();
        }

        // connCnt 정하기
        public static int GetConnCnt(int layerCnt, int nodeCnt)
        {
            int connCnt = 0;

            if (layerCnt == 2)
            { connCnt = nodeCnt; }
            else
            {
                double a1 = Math.Log(Convert.ToDouble(nodeCnt));
                double a2 = layerCnt - 2;
                double a3 = Math.Exp(a1 / a2);
                connCnt = Convert.ToInt32(a3);
            }

            return connCnt;
        }

        static void PartyTest3()
        {
            int nodeCnt = 100;
            int coreCntByNode = 20;
            int layerCnt = 4;   // 정하는것
            int connCnt = GetConnCnt(layerCnt, nodeCnt); // 계산되는것

            // 노드 세팅
            List<Tools.NodeInfo> nodesBef = new List<Tools.NodeInfo>();
            for (int i = 0; i < nodeCnt; i++)
            {
                var temp = new Tools.NodeInfo
                {
                    HpcName = "HPC0",
                    Addr = new int[layerCnt],
                    IP = @"IP" + i.ToString(),
                    IsServer = false
                };

                nodesBef.Add(temp);
            }

            // 노드 파티션
            List<Tools.NodeInfo> nodes = Tools.PartitionTools3.DoNodePartition(layerCnt, 0, connCnt, 0, nodesBef);

            // 코어 세팅
            List<Tools.CoreInfo> coresBef = new List<Tools.CoreInfo>();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < coreCntByNode; j++)
                {
                    Tools.CoreInfo c = new Tools.CoreInfo
                    {
                        HpcName = nodes[i].HpcName,
                        rankNo = i * coreCntByNode + j,
                        IP = nodes[i].IP,
                        Addr = new int[layerCnt]
                    };
                    coresBef.Add(c);
                }
            }

            // 코어 파티션
            List<Tools.CoreInfo> cores = Tools.PartitionTools3.DoCorePartition(layerCnt, 0, ref nodes, coresBef);
            
            // 노드 출력
            int cnter = 1;
            foreach (var item in nodes)
            {
                Console.Write("{0},{1}   \t:", cnter, item.IP);
                for (int i = 0; i < layerCnt; i++) Console.Write("\t{0}", item.Addr[i]);
                Console.Write("\t{0}", item.IsServer);
                Console.WriteLine();
                cnter++;
            }

            Console.WriteLine("end");
            Console.WriteLine();
            
            // 출력하고픈 것만 추림
            var selected = cores.Where(x => x.layerNo <= 2);

            // 코어 출력
            cnter = 0;
            foreach (var item in selected)
            {
                Console.Write("{0},{1}   \t:", cnter, item.IP);
                for (int i = 0; i < layerCnt; i++) Console.Write("\t{0}", item.Addr[i]);
                Console.Write("\t{0}", item.layerNo);
                Console.WriteLine();
                cnter++;
            }

            Console.WriteLine();
            Console.Write("end");
        }
        

        private static void SendDataTest(int coreNo)
        {
            if (coreNo == 0)
            {
                //객체 생성
                Dictionary<int, double[]> testData = new Dictionary<int, double[]>();
                for (int i = 0; i < 1000; i++)
                {
                    double[] scn = new double[1200];
                    Random rn = new Random();
                    for (int j = 0; j < 1200; j++)
                    {                        
                        scn[j] = rn.NextDouble();
                    }
                    testData.Add(i, scn);
                }                
                
                //Client Socket 생성
                Tools.ClientSocket cs = new Tools.ClientSocket(@"192.168.10.101", 7878);

                //send
                Tools.SendReceive.SendPrimitive(cs.sock, testData);

                Console.ReadLine();
            }
            else if (coreNo == 1)
            {
                //ServerSocket Socket 생성
                Tools.ServerSocket sc = new Tools.ServerSocket(7878);
                
                //Deserialize
                Dictionary<int, double[]> scnSet = Tools.SendReceive.ReceivePrimitive<Dictionary<int, double[]>>(sc.clientSock);

                Console.ReadLine();
            }
        }

        static void SingletonTest()
        {
            //Tools.SeedManager sm1 = Tools.SeedManager.GetSeedManager();

            //Engine.InforceComposer ic = new Engine.InforceComposer(100000);
            //List<Engine.Inforce> inf = ic.GetInforceSet();
            ////sm1.InsertSeedList(inf);
            
            //Tools.SeedManager sm2 = Tools.SeedManager.GetSeedManager();

            //object allocSeeds = sm2.AllocateSeed(1, 10000);

            //Console.ReadKey();
        }

        static void CalcTest()
        {
            Thread t1 = new Thread(new ParameterizedThreadStart(HeadJob));
            Thread t2 = new Thread(new ParameterizedThreadStart(WorkerJob));

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();                       
        }

        static void HeadJob(object args)
        {
            //Tools.ServerSocket ss = new Tools.ServerSocket(7878);

            //Engine.ScenarioComposer sc = new Engine.ScenarioComposer(1000, 1200);
                        
            //foreach(KeyValuePair<string, List<Engine.ScenarioSet>> item in sc.scnFullSet)
            //{
            //    Tools.SendReceive.SendGeneric(ss.clientSock, item.Key);
            //    Tools.SendReceive.SendGeneric(ss.clientSock, item.Value);
            //}

            //Engine.InforceComposer ic = new Engine.InforceComposer(100000);
            //Tools.SendReceive.SendGeneric(ss.clientSock, ic.GetInforceSet());

            //string endMessage = Tools.SendReceive.ReceiveGeneric<string>(ss.clientSock);

            //Console.Write(@"end");
        }



        static void WorkerJob(object args)
        {
            //Thread.Sleep(200);

            //Tools.ClientSocket cs = new Tools.ClientSocket(@"192.168.10.100", 7878);

            //Dictionary<string, List<Engine.ScenarioSet>> sc = new Dictionary<string, List<Engine.ScenarioSet>>();
                        
            //for (int i = 0; i < 6; i++)
            //{
            //    string assetName = Tools.SendReceive.ReceiveGeneric<string>(cs.sock);
            //    List<Engine.ScenarioSet> scn = Tools.SendReceive.ReceiveGeneric<List<Engine.ScenarioSet>>(cs.sock);

            //    sc.Add(assetName, scn);
            //}

            //List<Engine.Inforce> ic = Tools.SendReceive.ReceiveGeneric<List<Engine.Inforce>>(cs.sock);

            //Tools.SendReceive.SendGeneric(cs.sock, @"ThankYou");

            //Console.Write(@"end");
        }

        static void SocketTest()
        {
            //쓰레드2개 
            //전송해보기
        }

        // serialization버전에 따라 기능이 상이하므로 원시타입만 전송하는 걸로!!
        static void StreamTestList()
        {     //객체 생성
            //Engine.ValuationResult res1 = new Engine.ValuationResult(0.0, 0.1, 0.2, 0.3, 0.4);
            //Engine.ValuationResult res2 = new Engine.ValuationResult(1.0, 1.1, 1.2, 1.3, 1.4);
            //Engine.ValuationResult res3 = new Engine.ValuationResult(2.0, 2.1, 2.2, 2.3, 2.4);

            ////테스트1, 객체
            //byte[] resb1 = Tools.SerializationUtil.SerializeToByte(res1);
            //Engine.ValuationResult res1Returned = (Engine.ValuationResult)Tools.SerializationUtil.DeserializeToObject(resb1);

            ////테스트2, List<객체> 
            //List<Engine.ValuationResult> listRes = new List<Engine.ValuationResult>();
            //listRes.Add(res1);
            //listRes.Add(res2);
            //listRes.Add(res3);

            //byte[] listResB = Tools.SerializationUtil.SerializeToByte(listRes);
            //List<Engine.ValuationResult> listResReturned = (List<Engine.ValuationResult>)Tools.SerializationUtil.DeserializeToObject(listResB);

            ////테스트3, List<List<객체>> 
            //List<Engine.ValuationResult> listRes2 = new List<Engine.ValuationResult>();
            //listRes2.Add(res1);
            //listRes2.Add(res2);
            //listRes2.Add(res3);

            //List<List<Engine.ValuationResult>> listOfListRes = new List<List<Engine.ValuationResult>>();
            //listOfListRes.Add(listRes);
            //listOfListRes.Add(listRes2);

            //byte[] listOfListResB = Tools.SerializationUtil.SerializeToByte(listOfListRes);
            //List<List<Engine.ValuationResult>> listOfListResReturned = (List<List<Engine.ValuationResult>>)Tools.SerializationUtil.DeserializeToObject(listOfListResB);

            //Console.WriteLine();
        }

        // serialization버전에 따라 기능이 상이하므로 원시타입만 전송하는 걸로!!
        static void StreamTestDic()
        { //객체 생성
            //Engine.ValuationResult res1 = new Engine.ValuationResult(0.0, 0.1, 0.2, 0.3, 0.4);
            //Engine.ValuationResult res2 = new Engine.ValuationResult(1.0, 1.1, 1.2, 1.3, 1.4);
            //Engine.ValuationResult res3 = new Engine.ValuationResult(2.0, 2.1, 2.2, 2.3, 2.4);

            ////테스트1, 객체
            //byte[] resb1 = Tools.SerializationUtil.SerializeToByte(res1);
            //Engine.ValuationResult res1Returned = (Engine.ValuationResult)Tools.SerializationUtil.DeserializeToObject(resb1);

            ////테스트2, Dic<객체> 
            //Dictionary<int, Engine.ValuationResult> dicRes = new Dictionary<int, Engine.ValuationResult>();
            //dicRes.Add(1, res1);
            //dicRes.Add(2, res2);
            //dicRes.Add(3, res3);

            //byte[] dicResByte = Tools.SerializationUtil.SerializeToByte(dicRes);
            //Dictionary<int, Engine.ValuationResult> dicResReturned = (Dictionary<int, Engine.ValuationResult>)Tools.SerializationUtil.DeserializeToObject(dicResByte);

            ////테스트3, List<List<객체>> 
            //Dictionary<int, Engine.ValuationResult> dicRes2 = new Dictionary<int, Engine.ValuationResult>();
            //dicRes2.Add(1, res1);
            //dicRes2.Add(2, res2);
            //dicRes2.Add(3, res3);

            //Dictionary<int, Dictionary<int, Engine.ValuationResult>> dicOfDicRes = new Dictionary<int, Dictionary<int, Engine.ValuationResult>>();
            //dicOfDicRes.Add(1, dicRes);
            //dicOfDicRes.Add(2, dicRes2);

            //byte[] dicOfDicResByte = Tools.SerializationUtil.SerializeToByte(dicOfDicRes);
            //Dictionary<int, Dictionary<int, Engine.ValuationResult>> dicOfDicResReturned = (Dictionary<int, Dictionary<int, Engine.ValuationResult>>)Tools.SerializationUtil.DeserializeToObject(dicOfDicResByte);

            //Console.WriteLine();
        }
    }

    
}
