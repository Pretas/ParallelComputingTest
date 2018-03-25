using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class PartitionTools
    {
        /* 테스트환경
        static void Main(string[] args)
        {
            int depth = 4;
            int coreNo = 3003;

            int unit = PickUnit(depth, 3000);

            int[] cnt = GetN(depth, coreNo, unit);
            string[,] parti = GetPartitonArray(depth, coreNo, cnt);

            for (int i = 0; i < parti.GetLength(0); i++)
            {
                Console.Write("{0}번\t|  ", i);

                for (int j = 0; j < parti.GetLength(1); j++)
                {
                    Console.Write(string.Format("\t{0}", parti[i, j]));

                }

                Console.WriteLine();
            }

            int test1 = GetUpper(parti, 22);
            int test2 = GetUpper(parti, 1147);
            int test3 = GetUpper(parti, 1267);
            int test4 = GetUpper(parti, 0);
            int test5 = GetUpper(parti, 3002);

            List<int> tes1 = GetLower(parti, 0);
            List<int> tes2 = GetLower(parti, 2111);
            List<int> tes3 = GetLower(parti, 1795);
            List<int> tes4 = GetLower(parti, 2752);

            Console.ReadKey();

        }
        */

        public static int PickUnit(int depth, int coreNo)
        {
            int unit = 0;
            int diff = 0;
            int diffBef = coreNo;

            for (int n = 5; n < 30; n++)
            {
                int calc = 0;
                for (int i = 1; i <= depth - 1; i++)
                {
                    calc += Convert.ToInt16(Math.Pow(Convert.ToByte(n), Convert.ToByte(i)));
                }

                diffBef = diff;
                diff = coreNo - calc;

                if (diff <= 0)
                {
                    if (Math.Abs(diffBef) <= Math.Abs(diff))
                    { unit = n - 1; }
                    else
                    { unit = n; }
                    break;
                }
            }

            return unit;
        }

        public static int[] GetN(int depth, int total, int unit)
        {
            int[] res = new int[depth];
            for (int i = res.Length - 1; i >= 0; i--)
            {
                if (i == res.Length - 1) res[i] = 1;
                else if (i == 0) res[i] = total;
                else res[i] = res[i + 1] * unit + 1;
            }

            return res;
        }

        public static string[,] GetPartitonArray(int depth, int total, int[] cnt)
        {
            string[,] res = new string[total, depth];

            for (int i = 0; i < total; i++) res[i, 0] = "0";

            for (int l = 1; l < depth; l++)
            {
                int index = 0, count = 0, residual = total;
                string upperIndex = "", upperIndexBef = "-1";

                for (int i = 0; i < total; i++)
                {
                    upperIndex = res[i, l - 1];
                    if (upperIndex == upperIndexBef && upperIndex != "")
                    {
                        //카운팅 다했을 경우, 
                        if (count == cnt[l] && residual > cnt[l] / 2)
                        {
                            index++; count = 0;
                        }
                        res[i, l] = index.ToString();
                        count++;
                    }
                    else
                    {
                        index = 0; count = 0;
                        res[i, l] = "";
                    }

                    residual--; upperIndexBef = upperIndex;
                }
            }

            return res;
        }

        public static int GetLayerIndex(string[,] table, int rank)
        {
            //랭크의 해당 레이어 찾기
            int endLayerIndex = table.GetLength(1) - 1;
            int layer = endLayerIndex;
            for (int i = 0; i < endLayerIndex; i++)
            {
                if (table[rank, i + 1] == "")
                {
                    layer = i;
                    break;
                }
            }

            return layer;
        }

        public static int GetUpper(string[,] table, int rank)
        {
            int upper = 0;

            //랭크의 해당 레이어 찾기
            int layer = GetLayerIndex(table, rank);

            //랭크0이면 널 반환
            if (layer == 0)
            {
                upper = -1;
            }
            else
            {
                string upperStr = table[rank, layer - 1];
                for (int i = rank; i > 0; i--)
                {
                    if (table[i - 1, layer - 1] != upperStr)
                    {
                        upper = i;
                        break;
                    }
                }
            }

            return upper;
        }

        public static List<int> GetLower(string[,] table, int rank)
        {
            List<int> lowers = new List<int>();

            int layer = GetLayerIndex(table, rank);

            if (layer < table.GetLength(1) - 1)
            {
                string indexString = table[rank, layer];
                string indexBef = "";

                for (int i = rank; i < table.GetLength(0) - 1; i++)
                {
                    if (table[i + 1, layer] != indexString)
                    {
                        break;
                    }
                    else
                    {
                        string index = table[i + 1, layer + 1];
                        if (index != indexBef)
                        {
                            lowers.Add(i + 1);
                        }
                        indexBef = table[i + 1, layer + 1];
                    }
                }
            }

            return lowers;
        }
    }
}
