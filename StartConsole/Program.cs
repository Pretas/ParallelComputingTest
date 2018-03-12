using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace StartConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            CalcTest();
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
            Tools.ServerSocket ss = new Tools.ServerSocket(7878);

            Engine.ScenarioComposer sc = new Engine.ScenarioComposer(1000, 1200);
                        
            foreach(KeyValuePair<string, List<Engine.ScenarioSet>> item in sc.scnFullSet)
            {
                Tools.SendReceive.SendGeneric(ss.clientSock, item.Key);
                Tools.SendReceive.SendGeneric(ss.clientSock, item.Value);
            }

            Engine.InforceComposer ic = new Engine.InforceComposer(100000);
            Tools.SendReceive.SendGeneric(ss.clientSock, ic.GetInforceSet());

            string endMessage = Tools.SendReceive.ReceiveSendGeneric<string>(ss.clientSock);

            Console.Write(@"end");
        }



        static void WorkerJob(object args)
        {
            Thread.Sleep(200);

            Tools.ClientSocket cs = new Tools.ClientSocket(@"192.168.10.100", 7878);

            Dictionary<string, List<Engine.ScenarioSet>> sc = new Dictionary<string, List<Engine.ScenarioSet>>();
                        
            for (int i = 0; i < 6; i++)
            {
                string assetName = Tools.SendReceive.ReceiveSendGeneric<string>(cs.sock);
                List<Engine.ScenarioSet> scn = Tools.SendReceive.ReceiveSendGeneric<List<Engine.ScenarioSet>>(cs.sock);

                sc.Add(assetName, scn);
            }

            List<Engine.Inforce> ic = Tools.SendReceive.ReceiveSendGeneric<List<Engine.Inforce>>(cs.sock);

            Tools.SendReceive.SendGeneric(cs.sock, @"ThankYou");

            Console.Write(@"end");
        }

        static void SocketTest()
        {
            //쓰레드2개 
            //전송해보기
        }

        static void StreamTestList()
        {     //객체 생성
            Engine.ValuationResult res1 = new Engine.ValuationResult(0.0, 0.1, 0.2, 0.3, 0.4);
            Engine.ValuationResult res2 = new Engine.ValuationResult(1.0, 1.1, 1.2, 1.3, 1.4);
            Engine.ValuationResult res3 = new Engine.ValuationResult(2.0, 2.1, 2.2, 2.3, 2.4);

            //테스트1, 객체
            byte[] resb1 = Tools.SerializationUtil.SerializeToByte(res1);
            Engine.ValuationResult res1Returned = (Engine.ValuationResult)Tools.SerializationUtil.DeserializeToObject(resb1);

            //테스트2, List<객체> 
            List<Engine.ValuationResult> listRes = new List<Engine.ValuationResult>();
            listRes.Add(res1);
            listRes.Add(res2);
            listRes.Add(res3);

            byte[] listResB = Tools.SerializationUtil.SerializeToByte(listRes);
            List<Engine.ValuationResult> listResReturned = (List<Engine.ValuationResult>)Tools.SerializationUtil.DeserializeToObject(listResB);

            //테스트3, List<List<객체>> 
            List<Engine.ValuationResult> listRes2 = new List<Engine.ValuationResult>();
            listRes2.Add(res1);
            listRes2.Add(res2);
            listRes2.Add(res3);

            List<List<Engine.ValuationResult>> listOfListRes = new List<List<Engine.ValuationResult>>();
            listOfListRes.Add(listRes);
            listOfListRes.Add(listRes2);

            byte[] listOfListResB = Tools.SerializationUtil.SerializeToByte(listOfListRes);
            List<List<Engine.ValuationResult>> listOfListResReturned = (List<List<Engine.ValuationResult>>)Tools.SerializationUtil.DeserializeToObject(listOfListResB);

            Console.WriteLine();
        }

        static void StreamTestDic()
        { //객체 생성
            Engine.ValuationResult res1 = new Engine.ValuationResult(0.0, 0.1, 0.2, 0.3, 0.4);
            Engine.ValuationResult res2 = new Engine.ValuationResult(1.0, 1.1, 1.2, 1.3, 1.4);
            Engine.ValuationResult res3 = new Engine.ValuationResult(2.0, 2.1, 2.2, 2.3, 2.4);

            //테스트1, 객체
            byte[] resb1 = Tools.SerializationUtil.SerializeToByte(res1);
            Engine.ValuationResult res1Returned = (Engine.ValuationResult)Tools.SerializationUtil.DeserializeToObject(resb1);

            //테스트2, Dic<객체> 
            Dictionary<int, Engine.ValuationResult> dicRes = new Dictionary<int, Engine.ValuationResult>();
            dicRes.Add(1, res1);
            dicRes.Add(2, res2);
            dicRes.Add(3, res3);

            byte[] dicResByte = Tools.SerializationUtil.SerializeToByte(dicRes);
            Dictionary<int, Engine.ValuationResult> dicResReturned = (Dictionary<int, Engine.ValuationResult>)Tools.SerializationUtil.DeserializeToObject(dicResByte);

            //테스트3, List<List<객체>> 
            Dictionary<int, Engine.ValuationResult> dicRes2 = new Dictionary<int, Engine.ValuationResult>();
            dicRes2.Add(1, res1);
            dicRes2.Add(2, res2);
            dicRes2.Add(3, res3);

            Dictionary<int, Dictionary<int, Engine.ValuationResult>> dicOfDicRes = new Dictionary<int, Dictionary<int, Engine.ValuationResult>>();
            dicOfDicRes.Add(1, dicRes);
            dicOfDicRes.Add(2, dicRes2);

            byte[] dicOfDicResByte = Tools.SerializationUtil.SerializeToByte(dicOfDicRes);
            Dictionary<int, Dictionary<int, Engine.ValuationResult>> dicOfDicResReturned = (Dictionary<int, Dictionary<int, Engine.ValuationResult>>)Tools.SerializationUtil.DeserializeToObject(dicOfDicResByte);

            Console.WriteLine();
        }
    }
}
