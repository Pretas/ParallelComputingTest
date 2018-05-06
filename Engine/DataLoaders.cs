using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class ScenarioGenerator
    {
        private ScenarioSet scnSet = new ScenarioSet();
        //public ScenarioSet scnSet { get; private set; }

        public ScenarioGenerator(int scnNo, int period)
        {
            scnSet = new ScenarioSet();
            scnSet.ScenarioNo = scnNo;
            scnSet.scenarioData = new double[period];

            Random rnd = new Random(scnNo);
            for (int i = 0; i < period; i++)
            {
                scnSet.scenarioData[i] = rnd.NextDouble() - 0.5;
            }
        }

        public ScenarioSet GetScenarioSet()
        { return scnSet; }
    }

    public class ScenarioComposer
    {
        public int ScnCount { get; private set; }
        public int Period { get; private set; }

        public Dictionary<string, List<ScenarioSet>> scnFullSet = new Dictionary<string, List<ScenarioSet>>();

        public ScenarioComposer(int scnCount, int period)
        {
            ScnCount = scnCount;
            Period = period;

            string[] scnNames = new string[7] { "DSCRT", "StockKorea1", "StockKorea2", "StockUSA", "StockEuro", "MMF", "FI" };

            for (int i = 0; i < scnNames.Length; i++)
            {
                List<ScenarioSet> scnFullSetOneAsset = new List<ScenarioSet>();

                for (int j = 0; j < ScnCount; j++)
                {
                    ScenarioGenerator scnSet = new ScenarioGenerator(j + 1, period);
                    scnFullSetOneAsset.Add(scnSet.GetScenarioSet());
                }

                scnFullSet.Add(scnNames[i], scnFullSetOneAsset);
            }
        }
    }

    public class InforceComposer
    {
        public int InfCount { get; private set; }
        List<Inforce> Inf = new List<Inforce>();

        public InforceComposer(int infCount)
        {
            InfCount = infCount;
            for (int i = 1; i <= InfCount; i++)
            {
                Inforce rec = new Inforce();
                rec.Seq = i;
                int colCount = 50;
                rec.RecData = new double[colCount];
                for (int j = 0; j < colCount; j++)
                {
                    rec.RecData[j] = 1000000.0;
                }
                Inf.Add(rec);
            }
        }

        public List<Inforce> GetInforceSet()
        { return Inf; }
    }
}
