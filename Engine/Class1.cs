using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Engine
{
    [Serializable]
    public class ValuationResult
    {
        public double Var1 { get; private set; }
        public double Var2 { get; private set; }
        public double Var3 { get; private set; }
        public double Var4 { get; private set; }
        public double Var5 { get; private set; }

        public ValuationResult(double var1, double var2, double var3, double var4, double var5)
        {
            this.Var1 = var1;
            this.Var2 = var2;
            this.Var3 = var3;
            this.Var4 = var4;
            this.Var5 = var5;
        }
    }

    [Serializable]
    public struct Inforce
    {
        public int Seq;
        public double[] RecData;
    }

    [Serializable]
    public struct ScenarioSet
    {
        public string ID;
        public string Asset;
        public string ScenarioNo;        
        public double[] scenarioData;
    }

    public class Calculator
    {
        public Inforce Rec;
        public ScenarioSet Scn;
        public int LoopNo;
        public Dictionary<int, double[]> Res;

        public Calculator(Inforce rec, ScenarioSet scn, int loopNo)
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
                { Res.TryGetValue(i, out previousRes); }

                double[] currentRes = new double[Rec.RecData.Length];

                Random rnd = new Random();
                for (int j = 0; j < Rec.RecData.Length; j++)
                {
                    currentRes[j] = previousRes[j] * (rnd.NextDouble() + 0.5);
                }

                Res.Add(i, currentRes);
            }
        }
    }
}
