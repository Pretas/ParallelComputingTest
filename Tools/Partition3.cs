using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class PartitionTools3
    {
        public static List<NodeInfo> DoNodePartition(int layerCnt, int layerNo, int connCnt, int index, List<NodeInfo> orgList)
        {
            foreach (var item in orgList) item.Addr[layerNo] = index;

            if (layerNo == layerCnt - 3)
            {
                List<NodeInfo> newList = new List<NodeInfo>();
                int counter = 0;
                foreach (var item in orgList)
                {
                    item.Addr[layerNo + 1] = counter; counter++;
                    newList.Add(item);
                }

                return newList;
            }
            else
            {
                List<NodeInfo> newList = new List<NodeInfo>();
                List<List<NodeInfo>> div = GetDiv(orgList, connCnt);
                int counter = 0;

                foreach (List<NodeInfo> item in div)
                {
                    List<NodeInfo> itemNew = DoNodePartition(layerCnt, layerNo + 1, connCnt, counter, item);
                    newList.AddRange(itemNew);
                    counter++;
                }

                return newList;
            }
        }

        public static List<List<T>> GetDiv<T>(List<T> list, int groupCnt)
        {
            List<List<T>> newList = new List<List<T>>();
            
            for (int i = groupCnt; i >= 1; i--)
            {
                int cntByGroup = list.Count / i + (list.Count - list.Count / i * i > list.Count / i / 2 ? 1 : 0);

                var temp = list.GetRange(0, cntByGroup);
                list.RemoveRange(0, cntByGroup);
                newList.Add(temp);
            }

            return newList;            
        }
    }

    public class NodeInfo
    {
        public string IP;
        public bool IsServer;
        public int[] Addr;
    }
}