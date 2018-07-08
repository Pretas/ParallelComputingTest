using NetComponentTest;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Tools;

namespace Modules
{    
    public class InputContainerSCV : InputContainer
    {
        public Dictionary<int, ProductInfo> productsInfo = new Dictionary<int, ProductInfo>();
        public Dictionary<int, Loadings> loadings = new Dictionary<int, Loadings>();
    }

    public class SeedListSCV : SeedList
    {
    }

    public class SeedContainerSCV : SeedContainer
    {
        // Inforce : Dic<Index, Seed>
        public Dictionary<int, List<Inforce>> InforceSeed = new Dictionary<int, List<Inforce>>();
        // Scenario : Dic<Index, Seed>
        public Dictionary<int, List<ScenarioSet>> ScenarioSeed = new Dictionary<int, List<ScenarioSet>>();
    }

    public class SeedIndexSCV : SeedIndex
    {
        // List<Tuple<Inforce Index, Scenario Index>>
        public List<Tuple<int, int>> List = new List<Tuple<int, int>>();

    }

    public class ResultContainerSCV : ResultContainer
    { }

    public class CommActionsSCV : ICommActions
    {
        public void SetIsDoneLoadingInput(ref NetComponents nC, Role role)
        {
            throw new NotImplementedException();
        }

        void ICommActions.AcheiveSeed(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        int ICommActions.GetCount(SeedIndex si)
        {
            throw new NotImplementedException();
        }

        bool ICommActions.GetIsInProcess(NetComponents nc, Role role)
        {
            throw new NotImplementedException();
        }

        void ICommActions.InsertResultToDB(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.ListUpSeed(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.LoadInputFromDB(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.LoadSeedFromDB(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.ProjectionExecute(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.ProjectionInit(NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.PutResult(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.ReceiveInput(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.ReceiveResult(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.ReceiveSeed(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.sendInput(NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.SendResult(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.SendSeed(ref NetComponents nc)
        {
            throw new NotImplementedException();
        }

        void ICommActions.SetIsInProcessFrom(ref NetComponents nC, Role role)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleSCV
    {
        public void Init()
        {
            NetComponents nc = new NetComponents();

            nc.ICon = new InputContainerSCV();
            nc.SiBef = new SeedIndexSCV();
            nc.SiAllo = new SeedIndexSCV();
            nc.SiAft = new SeedIndexSCV();
            nc.SCon = new SeedContainerSCV();
            nc.RCon = new ResultContainerSCV();
        }
    }
}
        


