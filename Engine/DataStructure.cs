using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Modules
{
    [Serializable]
    public class ProductInfo
    {
        public int ProductCode { get; set; }
        public double GmxbRatio { get; set; }
    }

    [Serializable]
    public class Loadings
    {
        public int ProductCode { get; set; }
        public double AlphaRatio { get; set; }
    }

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

        public void SumUp(double var1, double var2, double var3, double var4, double var5)
        {
            Var1 += var1;
            Var2 += var2;
            Var3 += var3;
            Var4 += var4;
            Var5 += var5;
        }
    }

    [Serializable]
    public class Inforce : ICloneable
    {
        public int Seq;
        public double[] RecData;
        
        object ICloneable.Clone()
        {
            Inforce cloneInf = new Inforce();
            cloneInf.Seq = this.Seq;

            cloneInf.RecData = new double[this.RecData.Length];
            for (int i = 0; i < this.RecData.Length; i++)
            {
                cloneInf.RecData[i] = this.RecData[i];
            }

            return cloneInf;
        }
    }

    [Serializable]
    public struct ScenarioData : ICloneable
    {
        public int ScenarioNo;
        public double[] scenarioData;

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public struct ScenarioSet
    {
        public string ScnName;
        public List<ScenarioData> ScnValue;
    }

    
}