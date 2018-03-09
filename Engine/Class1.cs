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

    public class Inforce
    {
        public int Seq;
        public double Val001;
        public double Val002;
        public double Val003;
        public double Val004;
        public double Val005;
        public double Val006;
        public double Val007;
        public double Val008;
        public double Val009;
        public double Val010;
        public double Val011;
        public double Val012;
        public double Val013;
        public double Val014;
        public double Val015;
    }

    public class ScenarioSet
    {
        public string ID;
        public string Asset;
        public string ScenarioNo;
        public Dictionary<int, double[]> scenarioData;
    }

    public class Calculator
    {
        public Inforce Rec;
        public ScenarioSet Scn;
        public int LoopNo;
        public Dictionary<int, double> Res;

        public Calculator(Inforce rec, ScenarioSet scn, int loopNo)
        {
            Rec = rec;
            Scn = scn;
            LoopNo = loopNo;
        }

        public void Calculate()
        {
            for (int i = 0; i < LoopNo; i++)
            {

            }
        }
    }
}
