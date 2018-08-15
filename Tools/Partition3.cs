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
            // 해당 층의 인덱스 적기
            foreach (var item in orgList) item.Addr[layerNo] = index;

            List<NodeInfo> newList = new List<NodeInfo>();

            // "마지막층-2" 층인 경우
            if (layerCnt - layerNo == 3)
            {
                // "마지막층-1" 층의 인덱스 적기
                int counter = 0;
                foreach (var item in orgList)
                {
                    item.Addr[layerNo + 1] = counter; counter++;
                    newList.Add(item);
                }

                return newList;
            }
            // "마지막층-3" 이하 층인 경우
            else if (layerCnt - layerNo > 3)
            {
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
            else
            {
                throw new Exception("The Node Partition couldn't be executed when layerCnt is less than 3");
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

        
        public static List<CoreInfo> DoCorePartition(int layerCnt, int layerNo, ref List<NodeInfo> nodeList, List<CoreInfo> listOrg)
        {
            // 첫번째 층(layerNo==0)일 경우 0층 Addr에 0 입력
            if (layerNo == 0) for (int i = 0; i < listOrg.Count; i++) listOrg[i].Addr[0] = 0;

            // 반환할 리스트 생성
            List<CoreInfo> listNew = new List<CoreInfo>();

            // "마지막층-1" 층인 경우
            if (layerCnt - layerNo == 2)
            {
                // 마지막층의 Addr 적기                
                for (int i = 0; i < listOrg.Count; i++)
                {
                    listOrg[i].Addr[layerNo + 1] = i;
                    listOrg[i].layerNo = layerCnt - 1;
                    listNew.Add(listOrg[i]);
                }
            }
            // "마지막층-2" 이하 층인 경우
            else
            {
                // 아래층의 Addr 적기
                List<CoreInfo> listOrgOrdered = listOrg.OrderBy(x => x.IP).ToList();
                                
                string ipBef = "";
                int addr = 0;

                int cnt = listOrgOrdered.Count();
                for (int i = 0; i < cnt; i++)
                {
                    if (ipBef != listOrgOrdered[i].IP)
                    {
                        NodeInfo oneNode = nodeList.Where(x => x.IP == listOrgOrdered[i].IP).First();
                        addr = oneNode.Addr[layerNo + 1];
                        ipBef = oneNode.IP;
                    }

                    listOrgOrdered[i].Addr[layerNo + 1] = addr;
                }

                // 아래층의 코어파티션 수행
                //   아래층 그루핑하기
                List<IGrouping<int, CoreInfo>> div = listOrgOrdered.GroupBy(x => x.Addr[layerNo + 1]).ToList();

                //   아래층 파티션 수행, listNew에 저장
                int groupCnt = div.Count;
                for (int i = 0; i < groupCnt; i++)
                {
                    List<CoreInfo> temp = DoCorePartition(layerCnt, layerNo + 1, ref nodeList, div[i].ToList());
                    listNew.AddRange(temp);                    
                }
            }

            // server, layerNo 정하기
            //   서버가 될 노드 선정
            List<string> ipsThisGroup = listNew.Select(x => x.IP).Distinct().ToList();
            NodeInfo nodeWillBeServer = nodeList.Where(x => ipsThisGroup.Contains(x.IP)).Where(x => x.IsServer == false).First();

            //   노드인포 업데이트
            if (layerNo <= layerCnt - 3) nodeWillBeServer.IsServer = true;
            nodeList.RemoveAll(x => x.IP == nodeWillBeServer.IP);
            nodeList.Add(nodeWillBeServer);

            //   서버가 될 코어 선정, "서버가 될 노드"의 코어 중에서 "layerNo가 마지막 층(아직 Server가 아닌)"인 코어 고름
            CoreInfo coreWillBeServer = listNew.Where(x => x.IP == nodeWillBeServer.IP && x.layerNo == layerCnt - 1).First();

            //   코어인포 업데이트
            for (int i = layerNo + 1; i < layerCnt; i++) coreWillBeServer.Addr[i] = -1;         
            coreWillBeServer.layerNo = layerNo;
            listNew.RemoveAll(x => x.HpcName == coreWillBeServer.HpcName && x.rankNo == coreWillBeServer.rankNo);
            listNew.Add(coreWillBeServer);

            return listNew;
        }

        public static List<CoreInfo> DoCorePartitionOld(int layerCnt, int layerNo, ref List<NodeInfo> nodeList, List<CoreInfo> listOrg)
        {
            // 첫번째 층(layerNo==0)일 경우 0층 Addr에 0 입력
            if (layerNo == 0) for (int i = 0; i < listOrg.Count; i++) listOrg[i].Addr[0] = 0;

            // 아래층의 Addr 적기
            List<CoreInfo> listOrgOrdered = listOrg.OrderBy(x => x.IP).ToList();

            int cnt = listOrgOrdered.Count();
            string ipBef = "";
            int addr = 0;
            for (int i = 0; i < cnt; i++)
            {
                if (ipBef != listOrgOrdered[i].IP)
                {
                    NodeInfo oneNode = nodeList.Where(x => x.IP == listOrgOrdered[i].IP).First();
                    addr = oneNode.Addr[layerNo + 1];
                    ipBef = oneNode.IP;
                }

                listOrgOrdered[i].Addr[layerNo + 1] = addr;
            }

            // 아래층의 코어파티션 수행
            //   아래층 그루핑하기
            List<IGrouping<int, CoreInfo>> div0 = listOrgOrdered.GroupBy(x => x.Addr[layerNo + 1]).ToList();
            List<List<CoreInfo>> div = new List<List<CoreInfo>>();
            foreach (var item in div0) div.Add(item.ToList());

            //   아래층 파티션 수행, listNew에 저장
            List<CoreInfo> listNew = new List<CoreInfo>();
            int groupCnt = div.Count;
            for (int i = 0; i < groupCnt; i++)
            {
                if (layerNo <= layerCnt - 3)
                {
                    List<CoreInfo> temp = DoCorePartition(layerCnt, layerNo + 1, ref nodeList, div[i]);
                    listNew.AddRange(temp);
                }
                else
                {
                    int coreCnt = div[i].Count;
                    int counter = 0;
                    for (int j = 0; j < coreCnt; j++)
                    {
                        div[i][j].Addr[layerNo + 1] = counter;
                        div[i][j].layerNo = layerCnt - 1;
                        listNew.Add(div[i][j]);
                        counter++;
                    }
                }
            }

            // server, layerNo 정하기
            //   서버가 될 노드 선정
            List<string> ipsThisGroup = listNew.Select(x => x.IP).Distinct().ToList();
            NodeInfo nodeWillBeServer = nodeList.Where(x => ipsThisGroup.Contains(x.IP)).Where(x => x.IsServer == false).First();

            //   노드인포 업데이트
            if (layerNo <= layerCnt - 3) nodeWillBeServer.IsServer = true;
            nodeList.RemoveAll(x => x.IP == nodeWillBeServer.IP);
            nodeList.Add(nodeWillBeServer);

            //   서버가 될 코어 선정
            CoreInfo coreWillBeServer = listNew.Where(x => x.IP == nodeWillBeServer.IP && x.layerNo == layerCnt - 1).First();
            for (int i = layerNo + 1; i < layerCnt; i++) coreWillBeServer.Addr[i] = -1;

            //   코어인포 업데이트
            coreWillBeServer.layerNo = layerNo;
            listNew.RemoveAll(x => x.HpcName == coreWillBeServer.HpcName && x.rankNo == coreWillBeServer.rankNo);
            listNew.Add(coreWillBeServer);

            return listNew;
        }
    }

    public class NodeInfo
    {
        public string HpcName;
        public string IP;
        public bool IsServer;
        public int[] Addr;
    }

    public class CoreInfo
    {
        public string HpcName;
        public bool IsMainHpc;
        public int rankNo;
        public string IP;
        public int layerNo;
        public int[] Addr;
    }
}