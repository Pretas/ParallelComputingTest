using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class PartitionTools2
    {
        public class CoreConnInfo
        {
            public int Rank;
            public string IP;
            public int[] LayerIdx;
            public int LayerNo;            
            public int UpperRank;
            public int Port;
        }

        public static void SetCorePartition()
        {
            // 노드 파티션 결과 가져오기
            int[,] p = default(int[,]);
            SetNodePartition(324, 17,ref p);

            // IP에 인덱싱
            var ipMatching = new Dictionary<string, int>();
            for (int i = 0; i < p.GetLength(1); i++) ipMatching.Add(string.Format("IP" + (i+1).ToString()), i);

            // 기본 사항 세팅
            int layerCnt = p.GetLength(0) + 1;
            int coreCntByNode = 36;
            int nodeCnt = p.GetLength(1);
            int totalCnt = coreCntByNode * nodeCnt;

            // 최종 결과물 생성
            var partition = new Dictionary<int, CoreConnInfo>();
            
            int ipCounter = 1; 

            // 아이피 이름 쓰기
            for (int i = 0; i < totalCnt; i++)
            {
                var oneCore = new CoreConnInfo() { Rank = i, IP = string.Format("IP" + ipCounter), LayerIdx = new int[layerCnt] };

                partition.Add(i, oneCore);

                if ((i+1)%coreCntByNode==0)
                {
                    ipCounter++;
                }
            }

            // 레이어 파티션
            int counter = -1;

            for (int coreNo = 0; coreNo < totalCnt; coreNo++)
            {
                for (int layerNo = 0; layerNo < layerCnt; layerNo++)
                {
                    // 레이어 1층인 경우
                    if (layerNo == 0)
                    {
                        partition[coreNo].LayerIdx[layerNo] = 0;
                    }
                    // 레이어 마지막층인 경우
                    else if (layerNo == layerCnt - 1)
                    {
                        var oneCore = partition[coreNo];

                        int upper = oneCore.LayerIdx[layerNo - 1];
                        int upperBef = (coreNo == 0 ? -2 : partition[coreNo - 1].LayerIdx[layerNo - 1]);

                        if (upper == -1)
                        {
                            oneCore.LayerIdx[layerNo] = -1;
                        }
                        else
                        {
                            if (upper != upperBef) counter = -1;
                            oneCore.LayerIdx[layerNo] = counter;
                            counter++;
                        }
                    }
                    // 레이어 1층과 마지막층이 아닌 경우
                    else
                    {
                        var oneCore = partition[coreNo];

                        int lookUpValue = p[layerNo, ipMatching[oneCore.IP]];
                        int bef = (coreNo == 0 ? -2 : partition[coreNo - 1].LayerIdx[layerNo]);
                        int upper = oneCore.LayerIdx[layerNo - 1];
                        int upperBef = (coreNo == 0 ? -2 : partition[coreNo - 1].LayerIdx[layerNo - 1]);

                        bool cond1 = upper == -1;
                        bool cond2 = upper != upperBef;
                        //bool cond2 = bef != lookUpValue && (bef != -1 || upper != upperBef);

                        if (cond1 || cond2) oneCore.LayerIdx[layerNo] = -1;
                        else oneCore.LayerIdx[layerNo] = lookUpValue;
                    }
                }
            }

            // 레이어번호, upper 쓰기
            for (int i = 0; i < totalCnt; i++)
            {
                var oneCore = partition[i];

                // 레이어 번호 쓰기
                bool yn = true;
                int layerIdx = oneCore.LayerIdx.Length - 1;
                while (yn)
                {
                    if (oneCore.LayerIdx[layerIdx] == -1 && layerIdx != 0) layerIdx--;
                    else
                    {
                        oneCore.LayerNo = layerIdx;
                        yn = false;
                    }
                }
                
                // upper 쓰기
                // 1층일 경우
                if (oneCore.LayerNo == 0)
                {
                    oneCore.UpperRank = -1;
                }
                // 이외의 층일 경우
                else
                {
                    int myUpper = partition[i].LayerIdx[oneCore.LayerNo-1];

                    for (int j = i - 1; j >= 0; j--)
                    {
                        int val = partition[j].LayerIdx[oneCore.LayerNo];
                        int upperVal = partition[j].LayerIdx[layerIdx - 1];

                        if (val == -1 && upperVal == myUpper)
                        {
                            oneCore.UpperRank = j;
                            break;
                        }                        
                    }
                }                
            }

            for (int i = 0; i < 2000; i++)
            {
                var c = partition[i];
                var l = c.LayerIdx;
                Console.WriteLine(string.Format("{5}, {6} :\t {0}\t{1}\t{2}\t{3}\t layerNo = {4}, upperCore = {7}", l[0], l[1], l[2], l[3], c.LayerNo, c.IP, c.Rank, c.UpperRank));
            }

            Console.Write("end");
        }

        public static void SetNodePartition(int nodeCount, int connCount, ref int[,] p)
        {            
            int layerCount = 1;
            bool isUnder = true;

            while (isUnder)
            {
                isUnder = nodeCount > Convert.ToInt32((Math.Pow(connCount, layerCount - 1)));
                if (isUnder) layerCount++;
                else layerCount--;
            }

            p = new int[layerCount, nodeCount];
            
            for (int layerIdx = 0; layerIdx < layerCount; layerIdx++)
            {
                int unit = 0;

                if (layerIdx == 0) unit = nodeCount;
                else if (layerIdx == layerCount - 1) unit = 1;
                else unit = nodeCount / Convert.ToInt32((Math.Pow(connCount, layerIdx)));

                if (layerIdx == 0) // layer0일 경우
                {
                    SetNodePartitionByGroup(0, 0, nodeCount-1, nodeCount, ref p);
                }
                else // 나머지 layer일 경우
                {
                    int now = 0;

                    for (int nodeIdx = 0; nodeIdx < nodeCount; nodeIdx++)
                    {                        
                        if (p[layerIdx - 1, nodeIdx] != p[layerIdx - 1, Math.Min(nodeIdx + 1, nodeCount-1)] || nodeIdx == nodeCount - 1)
                        {
                            SetNodePartitionByGroup(layerIdx, now, nodeIdx, unit, ref p);
                            now = nodeIdx + 1;
                        }
                    }
                }
            }
        }

        public static void SetNodePartitionByGroup(int layerIdx, int from, int to, int unit, ref int[,] p)
        {
            int n = p.GetLength(1);

            int groupIdx = 0;
            int nodeCounter = 1;            
            int residual = n;

            for (int i = from; i <= to; i++)
            {
                p[layerIdx, i] = groupIdx;
                
                nodeCounter++;
                residual--;

                if (residual > unit / 2 && nodeCounter == unit+1)
                {
                    groupIdx++;
                    nodeCounter = 1;
                }
            }
        }
    }
}
