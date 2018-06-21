using NetComponents;
using System;
using System.Collections.Generic;

namespace Modules
{
    public class SCVProjectionFamily
    {
        public InputManagerSCV im;
        public DBConnectorSCV db;
        public SeedManagerSCV sm;
        public ResultManagerSCV rm;
        public ProjectorSCV projector;        
    }

    public class InputManagerSCV : InputManager
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

        public override void LoadInput()
        {
            IC = new InputContainerSCV();

            IC.productsInfo.Add(1, new ProductInfo { ProductCode = 1, GmxbRatio = 0.005 });
            IC.productsInfo.Add(2, new ProductInfo { ProductCode = 2, GmxbRatio = 0.006 });

            IC.loadings.Add(1, new Loadings { ProductCode = 1, AlphaRatio = 1.0 });
            IC.loadings.Add(2, new Loadings { ProductCode = 2, AlphaRatio = 2.0 });
        }

        public override void InsertInput(InputContainer ic)
        {
            InputContainerSCV icNew = (InputContainerSCV)ic;
            InputManagerSCV im = InputManagerSCV.GetSingleton();

            foreach (var item in icNew.productsInfo) im.IC.productsInfo.Add(item.Key, item.Value);
            foreach (var item in icNew.loadings) im.IC.loadings.Add(item.Key, item.Value);
        }

        public override bool GetCompleteLoading()
        {
            return IsCompletedLoading;
        }

        public override void SetCompleteLoading()
        {
            IsCompletedLoading = true;
        }

        public override InputContainer GetInput()
        {
            return IC;
        }               
    }

    public class DBConnectorSCV : DBConnector
    {
        SeedIndexSCV SeedIndexResidual = new SeedIndexSCV();
        Dictionary<int, Tuple<int, int>> PolIndexTotal = new Dictionary<int, Tuple<int, int>>();
        Dictionary<int, string> ScnIndexTotal = new Dictionary<int, string>();        
        int LoadUnit = 50;

        private bool IsFinished = false;

        public override void Init()
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
                    PolIndexTotal.Add(groupNoCounter, Tuple.Create(start, end));
                    groupNoCounter++;
                    counter = 0;
                }
                counter++;
            }

            ScnIndexTotal.Add(1, "Base");
            ScnIndexTotal.Add(2, "EqUp");
            ScnIndexTotal.Add(3, "EqDn");
            ScnIndexTotal.Add(4, "IrUp");
            ScnIndexTotal.Add(5, "IrDn");
            ScnIndexTotal.Add(6, "VolUp");
            ScnIndexTotal.Add(7, "VolDn");

            foreach (var pol in PolIndexTotal)
            {
                foreach (var scn in ScnIndexTotal)
                {
                    SeedIndexResidual.List.Add(Tuple.Create(pol.Key, scn.Key));
                }
            }
        }

        public override bool GetIsLackOfSeed()
        {
            SeedManagerSCV sm = SeedManagerSCV.GetSingleton();
            if (sm.GetSeedCountNotAllocated() < LoadUnit) return true;
            else return false;
        }
           
        public override void LoadSeed(out SeedIndex si, out SeedContainer sc)
        {
            // 유닛 만큼 시드인덱스 가져오기            
            int cnt = Math.Max(SeedIndexResidual.List.Count, LoadUnit);
            SeedIndexSCV siNew = new SeedIndexSCV();
            siNew.List.GetRange(0, cnt - 1); // 원하는 만큼 가져오기
            SeedIndexResidual.List.RemoveRange(0, cnt - 1); // 가져온 부분 삭제

            // 필요한 시드를 받음
            SeedManagerSCV sm = SeedManagerSCV.GetSingleton();
            SeedIndexCompartSCV sic = (SeedIndexCompartSCV)sm.GetSeedIndexNotInSeedContainer(siNew);

            // 씨드 로딩
            SeedContainerSCV scNew = new SeedContainerSCV();
            // --인포스 로딩
            foreach (int item in sic.InforceSeedIndex)
            {
                InforceComposer ic = new InforceComposer(PolIndexTotal[item].Item2 - PolIndexTotal[item].Item1 + 1);
                scNew.InforceSeed.Add(item, ic.GetInforceSet());
            }
            // --시나리오 로딩
            foreach (int item in sic.ScenarioSeedIndex)
            {
                ScenarioComposer scnComposer = new ScenarioComposer(1000, 1200);
                scNew.ScenarioSeed.Add(item, scnComposer.scnFullSet);
            }

            si = siNew;
            sc = scNew;
        }

        public override void InsertResultToDB(Result result)
        {
            Console.WriteLine("Insert Result");
        }

        public override void SetIsFinished(bool yn)
        {
            IsFinished = yn;
        }

        public override bool GetIsFinished()
        {
            if (SeedIndexResidual.List.Count == 0) return true;
            else return false;
        }
    }

    public class SeedManagerSCV : SeedManager
    {
        // ***** 싱글턴 구현부
        protected static SeedManagerSCV ST;
        protected SeedManagerSCV() { }
        protected static object SyncLock = new object();

        public static SeedManagerSCV GetSingleton()
        {
            if (ST == null) lock (SyncLock) if (ST == null) ST = new SeedManagerSCV();
            return ST;
        }
        // *****

        private SeedIndexSCV SeedNotAllocated = new SeedIndexSCV();
        private Dictionary<int, SeedIndexSCV> SeedAllocated = new Dictionary<int, SeedIndexSCV>();
        private SeedContainerSCV SC = new SeedContainerSCV();
        private bool IsMoreSeedFromUpperLayer = true;

        public override void InsertSeed(SeedIndex si, SeedContainer sc)
        {
            SeedIndexSCV seedIndexFrom = (SeedIndexSCV)si;
            SeedContainerSCV seedContainerFrom = (SeedContainerSCV)sc;

            SeedNotAllocated.List.AddRange(seedIndexFrom.List);
            foreach (var item in seedContainerFrom.InforceSeed)
            {
                SC.InforceSeed.Add(item.Key, item.Value);
            }
            foreach (var item in seedContainerFrom.ScenarioSeed)
            {
                SC.ScenarioSeed.Add(item.Key, item.Value);
            }
        }

        public override void PickUpAndAllocateSeed(int coreNo, int unit, out SeedIndex si)
        {
            // SeedNotAllocated 에서 골라와 저장
            SeedIndexSCV siPicked = new SeedIndexSCV();
            int cnt = Math.Max(SeedNotAllocated.List.Count, unit);
            siPicked.List.AddRange(SeedNotAllocated.List.GetRange(0, cnt - 1));

            // SeedNotAllocated 삭제
            SeedNotAllocated.List.RemoveRange(0, cnt - 1);

            // SeedAllocate에 저장
            if (!SeedAllocated.ContainsKey(coreNo)) SeedAllocated.Add(coreNo, new SeedIndexSCV());
            foreach (var item in siPicked.List)
            {
                SeedAllocated[coreNo].List.Add(item);
            }

            // 골라온 부분 리턴
            si = siPicked;
        }

        public override void RemoveAllocatedSeed(int coreNo, SeedIndex si)
        {
            SeedIndexSCV siCopy = (SeedIndexSCV)si;

            if (SeedAllocated.ContainsKey(coreNo))
            {
                foreach (var item in siCopy.List)
                {
                    int index = SeedAllocated[coreNo].List.IndexOf(item);
                    if (index == -1) throw new Exception(string.Format(@"'{0}, {1}'는 없는 객체입니다.", item.Item1.ToString(), item.Item2.ToString()));
                    SeedAllocated[coreNo].List.RemoveAt(index);
                }
            }
        }

        public override void RearrangeSeedContainer()
        {
            // SeedContainer에 필요없는 씨드 삭제            
        }

        public override void ReturnBackSeed(int coreNo)
        {
            throw new NotImplementedException();
        }
        
        public override SeedContainer GetSeedRequiredFromLowerLayer(SeedIndexCompart sic)
        {
            throw new NotImplementedException();
        }

        public override void SetIsMoreSeedFromUpperLayer(bool isFinished)
        {
            throw new NotImplementedException();
        }

        public override bool GetIsMoreSeedFromUpperLayer()
        {
            throw new NotImplementedException();
        }

        public override bool IsEmpty()
        {
            throw new NotImplementedException();
        }
        
        public override SeedIndexCompart GetSeedIndexNotInSeedContainer(SeedIndex si)
        {
            throw new NotImplementedException();
        }

        public override int GetSeedCountNotAllocated()
        {
            throw new NotImplementedException();
        }

        public override bool GetIsLackOfSeed()
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

        public bool GetIsEmpty()
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
            foreach (SeedIndex item in siList.List)
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
        public List<Tuple<int, int>> List = new List<Tuple<int, int>>();       
        
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
        public Dictionary<int, List<ScenarioSet>> ScenarioSeed = new Dictionary<int, List<ScenarioSet>>();
    }

    public class ResultSCV
    { }
        
    public class ProjectionDataSCV
    { }    
}
