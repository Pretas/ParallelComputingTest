using NetComponents;
using System;
using System.Collections.Generic;

namespace Modules
{
    public class InputManagerSCV : Singleton, IInputLoader
    {
        public InputContainerSCV IC;
        public bool CompleteLoadingYN = false;

        public void LoadInput()
        {
            Console.WriteLine("Load From DB");
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

        public SeedIndexCompart GetSeedRequired(ISeedManager sm)
        {
            Console.WriteLine("Check Lack of Seed, return boolType");
            return new SeedIndexCompartSCV();
        }

        public SeedContainer LoadSeed(SeedIndexCompart sic, ref ISeedManager sm)
        {
            Console.WriteLine("Load Seed From DB, return SeedContainerType");
            return new SeedContainerSCV();
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
    }

}
