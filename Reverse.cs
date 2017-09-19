using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algorithms
{
    class Reverse
    {
        public static void ReverseList(int[] list)
        {
            if (list.Length > 0)
            {
                int mid = list.Length / 2;
                for (int i = 0; i < mid; i++)
                {
                    int temp = list[i];
                    list[i] = list[list.Length - 1 - i];
                    list[list.Length - 1 - i] = temp;
                }
            }
        }
    }
}
