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
}
