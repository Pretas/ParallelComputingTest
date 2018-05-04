using NetComponents;
using System;
using System.Collections.Generic;

namespace Modules
{
    public class InputManagerSCV : Singleton, IInputManager
    {
        public InputContainerSCV IC;
        public bool CompleteLoadingYN = false;

        public bool GetCompleteLoading()
        {
            throw new NotImplementedException();
        }

        public InputContainer GetInput()
        {
            throw new NotImplementedException();
        }

        public void InsertInput(InputContainer ic)
        {
            throw new NotImplementedException();
        }

        public void LoadInput()
        {
            Console.WriteLine("Load From DB");
        }

        public void SetCompleteLoading()
        {
            throw new NotImplementedException();
        }
    }

    public class InputContainerSCV : InputContainer
    { }

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

    public class SeedContainerSCV : SeedContainer
    { }

    public class SeedIndexSCV : SeedIndex
    { }

    public class SeedIndexCompartSCV : SeedIndexCompart
    { }


    public class ResultManagerSCV : Singleton, IResultManager
    {
        public List<SeedIndex> ResStacked;
        public int SumUpPoint { get; set; }

        public void SumUp(Result source, ref Result baseResult)
        {
            throw new NotImplementedException();
        }

        public void ClearIndex()
        {
            throw new NotImplementedException();
        }

        public List<SeedIndex> GetAllStackedResult()
        {
            throw new NotImplementedException();
        }

        public bool CheckNeedSumUp()
        {
            throw new NotImplementedException();
        }

        public void StackResult(List<SeedIndex> resIndex, List<Result> resReal)
        {
            throw new NotImplementedException();
        }

        public void ClearResult()
        {
            throw new NotImplementedException();
        }

        public void SumUp()
        {
            throw new NotImplementedException();
        }

        public void UploadResult()
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public void StackResult(List<SeedIndex> resIndex, Result resReal)
        {
            throw new NotImplementedException();
        }

        Tuple<List<SeedIndex>, Result> IResultManager.SumUp()
        {
            throw new NotImplementedException();
        }
    }
}
