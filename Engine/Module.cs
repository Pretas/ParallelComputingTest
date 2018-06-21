using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Engine
{
    public class Module
    {
        public Module(int scnCount, int infCount, Engine.ScenarioComposer sc, Engine.InforceComposer ic)
        {
            Dictionary<int, ValuationResult> aggregatedResult = new Dictionary<int, ValuationResult>();

            for (int i = 1; i <= infCount; i++)
            {
                aggregatedResult.Add(i, new ValuationResult(0.0, 0.0, 0.0, 0.0, 0.0));
            }

            //시나리오 1000개 루프
            for (int i = 1; i <= scnCount; i++)
            {
                List<ScenarioSet> scnOneNumber = new List<ScenarioSet>();

                foreach (var item in sc.scnFullSet)
                {
                    ScenarioSet tempScn = item.Value.Find(x => x.ScenarioNo == i);
                    scnOneNumber.Add(tempScn);
                }

                //인포스 루프
                for (int j = 1; j <= infCount; j++)
                {
                    Inforce tempInforce = ic.GetInforceSet().Find(x => x.Seq == j);
                    Calculator calc = new Calculator(tempInforce, scnOneNumber, 1200);
                    calc.Calculate();
                    ValuationResult sumUpRes = MeasureUp(calc.Res);
                    SummingUp.SumUpByInf(tempInforce.Seq, scnCount, sumUpRes, ref aggregatedResult);
                }
            }

            Console.WriteLine(@"end");
        }

        private static ValuationResult MeasureUp(Dictionary<int, double[]> res)
        {
            double[] sumUpRes = new double[5];
            foreach (var item in res)
            {
                sumUpRes[0] += item.Value[0];
                sumUpRes[1] += item.Value[1];
                sumUpRes[2] += item.Value[2];
                sumUpRes[3] += item.Value[3];
                sumUpRes[4] += item.Value[4];
            }

            return new ValuationResult(sumUpRes[0], sumUpRes[1], sumUpRes[2], sumUpRes[3], sumUpRes[4]);
        }
    }

    public class SummingUp
    {
        public static void SumUpByInf(int seq, int scnCount, ValuationResult res, ref Dictionary<int, ValuationResult> totalRes)
        {
            totalRes[seq].SumUp(res.Var1 / scnCount, res.Var2 / scnCount, res.Var3 / scnCount, res.Var4 / scnCount, res.Var5 / scnCount);
        }
    }


    public class Calculator
    {
                
        public Inforce Rec;
        public List<ScenarioSet> Scn;
        public int LoopNo;
        public Dictionary<int, double[]> Res = new Dictionary<int, double[]>();

        public Calculator(Inforce rec, List<ScenarioSet> scn, int loopNo)
        {
            Rec = rec;
            Scn = scn;
            LoopNo = loopNo;
        }

        public void Calculate()
        {
            for (int i = 1; i <= LoopNo; i++)
            {
                double[] previousRes = new double[Rec.RecData.Length];
                if (i == 1)
                { previousRes = (double[])Rec.RecData.Clone(); }
                else
                { Res.TryGetValue(i - 1, out previousRes); }

                double[] currentRes = new double[Rec.RecData.Length];

                double yield = 0.0;
                foreach (var item in Scn)
                { yield = yield + item.scenarioData[i - 1]; }
                yield = yield / Scn.Count;

                for (int j = 0; j < Rec.RecData.Length / 5; j++)
                {
                    currentRes[j] = previousRes[j] * (1.0 + yield);
                }

                Res.Add(i, currentRes);
            }
        }
    }
}
