using NetComponents;
using System;
using System.Collections.Generic;

namespace Modules
{
    public class SCVProjectionFamily : ProjectionFamily
    {
        public InputManagerSCV im = new InputManagerSCV();
        public SeedLoaderSCV sl;
        public SeedManagerSCV sm;
        public ResultManagerSCV rm;
        public ProjectorSCV projector;
        //public InputContainerSCV inputContainer;
        //public SeedIndexSCV seedIndex;
        //public SeedIndexCompartSCV sic;
        //public SeedContainerSCV seedContainer;
        //public ResultSCV result;
        //public ProjectionDataSCV pjd;
    }

    public class InputManagerSCV : IInputManager
    {
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
        int sPol, ePol, cPol;
        int sScn, eScn, cScn;

        public void Init()
        {
            Console.WriteLine("Init Total Seed List");
        }

        public bool CheckLackOfSeed()
        {
            Console.WriteLine("Check Lack of Seed, return boolType");
            return true;
        }

        public void InsertSeedRequired()
        {
            throw new NotImplementedException();
        }

        public void LoadSeed()
        {
            throw new NotImplementedException();
        }

        Tuple<List<SeedIndex>, SeedContainer> ISeedLoader.LoadSeed()
        {
            throw new NotImplementedException();
        }

        public bool IsFinished()
        {
            throw new NotImplementedException();
        }

        Tuple<SeedIndex, SeedContainer> ISeedLoader.LoadSeed()
        {
            throw new NotImplementedException();
        }
    }

    public class SeedManagerSCV : ISeedManager
    {
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
    }

    public class ResultManagerSCV : IResultManager
    {
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
    { }

    public class SeedIndexCompartSCV : SeedIndexCompart
    { }

    public class SeedContainerSCV : SeedContainer
    { }

    public class ResultSCV
    { }
        
    public class ProjectionDataSCV
    { }


    



    public class TestProjector :  NetComponents.IProjector
    { 
        
    }

    


    public class SeedManager : Singleton, ISeedManager
    {
        public SeedContainer SC;
        public List<SeedIndex> SeedNotAllocated;
        public Dictionary<int, List<SeedIndex>> SeedAllocated;
        public bool CompleteLoadingYN = false;

        public void InsertSeedIndex(SeedContainer sc)
        {
            Console.WriteLine("Insert SeedIndex");
        }

        public void InsertSeed(SeedContainer sc)
        {
            Console.WriteLine("Insert Seed to Container");
        }

        public void AllocateSeed(int coreNo, List<SeedIndex> si)
        {
            Console.WriteLine(string.Format("Allocate Seed to {0}", coreNo));
        }

        public void ReturnBackSeed(int coreNo, List<SeedIndex> si)
        {
            Console.WriteLine(string.Format("ReturnBack Seed to {0}", coreNo));
        }

        public void InsertSeedIndex(SeedIndex si)
        {
            throw new NotImplementedException();
        }

        public void RemoveSeedAllocated(List<SeedIndex> si)
        {
            throw new NotImplementedException();
        }

        public void InsertSeed(List<SeedIndex> si, SeedContainer sc)
        {
            throw new NotImplementedException();
        }

        public void AllocateSeed(int coreNo)
        {
            throw new NotImplementedException();
        }

        public void ReturnBackSeed(int coreNo)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllocatedSeed(int coreNo, List<SeedIndex> si)
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public List<SeedIndex> PickUpAndAllocateSeed(int coreNo)
        {
            throw new NotImplementedException();
        }

        public void RearrangeSeedContainer()
        {
            throw new NotImplementedException();
        }

        public SeedContainer GetSeed()
        {
            throw new NotImplementedException();
        }

        public SeedContainer GetSeedRequiredFromLowerLayer(SeedIndexCompart sic)
        {
            throw new NotImplementedException();
        }

        public void SetIsMoreSeedFromUpperLayer(bool isFinished)
        {
            throw new NotImplementedException();
        }

        public bool GetIsMoreSeedFromUpperLayer()
        {
            throw new NotImplementedException();
        }

        public bool IsLackOfSeed()
        {
            throw new NotImplementedException();
        }

        public SeedIndexCompart GetSeedIndexNotInSeedContainer(List<SeedIndex> si)
        {
            throw new NotImplementedException();
        }
    }



    
}
