using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //객체 생성
            Engine.ValuationResult res1 = new Engine.ValuationResult(0.0, 0.1, 0.2, 0.3, 0.4);
            Engine.ValuationResult res2 = new Engine.ValuationResult(1.0, 1.1, 1.2, 1.3, 1.4);
            Engine.ValuationResult res3 = new Engine.ValuationResult(2.0, 2.1, 2.2, 2.3, 2.4);

            //테스트1, 객체
            byte[] resb1 = Serialization.SerializationUtil.SerializeToByte(res1);
            Engine.ValuationResult res1Returned = (Engine.ValuationResult)Serialization.SerializationUtil.DeserializeToObject(resb1);

            //테스트2, List<객체> 
            List<Engine.ValuationResult> listRes = new List<Engine.ValuationResult>();
            listRes.Add(res1);
            listRes.Add(res2);
            listRes.Add(res3);

            byte[] listResB = Serialization.SerializationUtil.SerializeToByte(listRes);
            List<Engine.ValuationResult> listResReturned = (List<Engine.ValuationResult>)Serialization.SerializationUtil.DeserializeToObject(listResB);

            //테스트2, List<List<객체>> 
            List<Engine.ValuationResult> listRes2 = new List<Engine.ValuationResult>();
            listRes2.Add(res1);
            listRes2.Add(res2);
            listRes2.Add(res3);

            List<List<Engine.ValuationResult>> listOfListRes = new List<List<Engine.ValuationResult>>();
            listOfListRes.Add(listRes);
            listOfListRes.Add(listRes2);

            byte[] listOfListResB = Serialization.SerializationUtil.SerializeToByte(listOfListRes);
            List<List<Engine.ValuationResult>> listOfListResReturned = (List<List<Engine.ValuationResult>>)Serialization.SerializationUtil.DeserializeToObject(listOfListResB);

            Console.WriteLine();
        }
    }
}
