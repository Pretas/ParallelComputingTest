using NetComponents;
using System;
using System.Collections.Generic;

namespace Modules
{
    public class SCVProjectionFamily : ProjectionFamily
    {
        public InputManagerSCV im;
        public SeedLoaderSCV sl;
        public SeedManagerSCV sm;
        public ResultManagerSCV rm;
        public ProjectorSCV projector;
    }

    public class InputManagerSCV : IInputManager
    {
        // ***** 싱글턴 구현부
        protected static InputManagerSCV ST;
        protected InputManagerSCV() { }
        protected static object SyncLock = new object();

        public static InputManagerSCV GetSingleton()
        {
            if (ST == null) lock (SyncLock) if (ST == null) ST = new InputManagerSCV();
            return ST;
        }
        // *****

        private InputContainerSCV IC;
        private bool IsCompletedLoading = false;

        public void LoadInput()
        {
            IC = new InputContainerSCV();

            IC.productsInfo.Add(1, new ProductInfo { ProductCode = 1, GmxbRatio = 0.005 });
            IC.productsInfo.Add(2, new ProductInfo { ProductCode = 2, GmxbRatio = 0.006 });

            IC.loadings.Add(1, new Loadings { ProductCode = 1, AlphaRatio = 1.0 });
            IC.loadings.Add(2, new Loadings { ProductCode = 2, AlphaRatio = 2.0 });            
        }

        public bool GetCompleteLoading()
        {
            return IsCompletedLoading;
        }

        public void SetCompleteLoading()
        {
            IsCompletedLoading = true;
        }

        public InputContainer GetInput()
        {
            return IC;
        }
    }

    public class SeedLoaderSCV : ISeedLoader
    {
        Dictionary<int, Tuple<int, int>> PolList = new Dictionary<int, Tuple<int, int>>();
        Dictionary<int, string> ScnList = new Dictionary<int, string>();
        int CurrentPolListNo;
        int CurrentScnListNo;

        public void Init()
        {
            int ePol, cPol;

            ePol = 5000000; cPol = 0;

            int polUnit = 1000;
            int counter = 1;
            int groupNoCounter = 1;
            int start = 0; int end = 0;
            for (int i = 1; i <= ePol; i++)
            {
                cPol = i;
                if (counter == 1)
                { start = cPol; }

                if (counter == polUnit || cPol==ePol)
                {
                    end = cPol;
                    PolList.Add(groupNoCounter, Tuple.Create(start, end));
                    groupNoCounter++;
                    counter = 0;
                }
                counter++;
            }

            ScnList.Add(1, "Base");
            ScnList.Add(2, "EqUp");
            ScnList.Add(3, "EqDn");
            ScnList.Add(4, "IrUp");
            ScnList.Add(5, "IrDn");
            ScnList.Add(6, "VolUp");
            ScnList.Add(7, "VolDn");
        }

        public bool IsLackOfSeed()
        {
            SeedManagerSCV sm = SeedManagerSCV.GetSingleton();
            if (sm.GetSeedCountNotAllocated() < 20)
            { return true; }
            else return false;
        }

        public bool IsFinished()
        {
            bool isFinishedPol = cPol == ePol;
            bool isFinishedScn = cScn == eScn;
            if (isFinishedPol && isFinishedScn) return true;
            else return false;
        }
        
        public void LoadSeed(ref SeedIndex si, ref SeedContainer sc)
        {
            

            
        }

        public Tuple<SeedIndex, SeedContainer> LoadSeed()
        {
            SeedIndexSCV si2 = new SeedIndexSCV();
            SeedContainerSCV sc2 = new SeedContainerSCV();

            si2.SeedIndex.Add(Tuple.Create(1, 1));
            sc2.InforceSeed.Add(1, new List<Inforce>());
            sc2.ScenarioSeed.Add(1, new Dictionary<string, ScenarioFullSet>());

            return Tuple.Create(si2 as SeedIndex, sc2 as SeedContainer);
        }
    }

    public class SeedManagerSCV : ISeedManager
    {
        protected static SeedManagerSCV ST;
        protected SeedManagerSCV() { }
        protected static object SyncLock = new object();

        public static SeedManagerSCV GetSingleton()
        {
            if (ST == null) lock (SyncLock) if (ST == null) ST = new SeedManagerSCV();
            return ST;
        }

        private SeedContainerSCV SC = new SeedContainerSCV();
        private SeedIndexSCV SeedNotAllocated = new SeedIndexSCV();
        private Dictionary<int, SeedIndexSCV> SeedAllocated = new Dictionary<int, SeedIndexSCV>();
        private bool IsMoreSeedFromUpperLayer = true;

        public bool GetIsMoreSeedFromUpperLayer()
        {
            throw new NotImplementedException();
        }

        public SeedContainer GetSeed()
        {
            throw new NotImplementedException();
        }

        public SeedIndexCompart GetSeedIndexNotInSeedContainer(SeedIndex si)
        {
            throw new NotImplementedException();
        }

        public SeedContainer GetSeedRequiredFromLowerLayer(SeedIndexCompart sic)
        {
            throw new NotImplementedException();
        }

        public void InsertSeed(SeedIndex si, SeedContainer sc)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public bool IsLackOfSeed()
        {
            throw new NotImplementedException();
        }

        public SeedIndex PickUpAndAllocateSeed(int coreNo)
        {
            throw new NotImplementedException();
        }

        public void RearrangeSeedContainer()
        {
            throw new NotImplementedException();
        }

        public void RemoveAllocatedSeed(int coreNo, SeedIndex si)
        {
            throw new NotImplementedException();
        }

        public void ReturnBackSeed(int coreNo)
        {
            throw new NotImplementedException();
        }

        public void SetIsMoreSeedFromUpperLayer(bool isFinished)
        {
            throw new NotImplementedException();
        }

        public int GetSeedCountNotAllocated()
        {
            throw new NotImplementedException();
        }
    }

    public class ResultManagerSCV : IResultManager
    {
        protected static ResultManagerSCV ST;
        protected ResultManagerSCV() { }
        protected static object SyncLock = new object();

        public static ResultManagerSCV GetSingleton()
        {
            if (ST == null) lock (SyncLock) if (ST == null) ST = new ResultManagerSCV();
            return ST;
        }

        public bool CheckNeedSumUp()
        {
            throw new NotImplementedException();
        }

        public void ClearResult()
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public void StackResult(SeedIndex resIndex, Result resReal)
        {
            throw new NotImplementedException();
        }

        public Tuple<SeedIndex, Result> SumUp()
        {
            throw new NotImplementedException();
        }

        public void UploadResult()
        {
            throw new NotImplementedException();
        }
    }

    public class ProjectorSCV : IProjector
    {
        protected InputContainer IC;

        public ProjectorSCV(InputContainer ic)
        { IC = ic; }

        protected object GetSpecificInput(SeedIndex si) { return null; }

        public void Init(InputContainer ic, Func<InputContainer, SeedContainer, ProjectionData> funcRun)
        { }

        public Result Execute()
        {
            foreach (SeedIndex item in siList)
            {
                //object input = GetSpecificInput(item);
                //ProjectionData pjd = Run(item);
                //SumUp(pjd, ref baseResult);
            }

            return null;
        }

        public void SumUp(ProjectionData pjd, ref Result baseResult)
        {
            throw new NotImplementedException();
        }        
    }
    
    public class InputContainerSCV : InputContainer
    {
        public Dictionary<int, ProductInfo> productsInfo = new Dictionary<int, ProductInfo>();
        public Dictionary<int, Loadings> loadings = new Dictionary<int, Loadings>();
    }

    public class SeedIndexSCV : SeedIndex
    {
        // List<Tuple<Inforce Index, Scenario Index>>
        public List<Tuple<int, int>> SeedIndex = new List<Tuple<int, int>>();
    }

    public class SeedIndexCompartSCV : SeedIndexCompart
    {
        // Inforce : List<Index>
        public List<int> InforceSeedIndex = new List<int>();
        // Scenario : List<Index>
        public List<int> ScenarioSeedIndex = new List<int>();
    }

    public class SeedContainerSCV : SeedContainer
    {
        // Inforce : Dic<Index, Seed>
        public Dictionary<int, List<Inforce>> InforceSeed = new Dictionary<int, List<Inforce>>();
        // Scenario : Dic<Index, Seed>
        public Dictionary<int, Dictionary<string, ScenarioFullSet>> ScenarioSeed = new Dictionary<int, Dictionary<string, ScenarioFullSet>>();
    }

    public class ResultSCV
    { }
        
    public class ProjectionDataSCV
    { }


    

    


    
}
