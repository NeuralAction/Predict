using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsTests
{
    class Numpy
    {
        public float[] CumSum(float[] arr)
        {
            int size = arr.Length;
            float temp = 0;
            float[] result = new float[size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j <= i; j++)
                {

                    temp += arr[j];
                }
                result[i] = temp;
                temp = 0;
            }
            return result;
        }

        public int SearchSorted(float[] arr, double v)
        {
            int size = arr.Length;

            for (int i = 0; i < size; i++)
            {
                if (i == 0)
                {
                    if (arr[i] == v || arr[i] > v)
                    {
                        return 0;
                    }
                }

                if (arr[i] < v)
                {
                    continue;
                }
                else if (arr[i] == v)
                {
                    return i;
                }
                else if (arr[i] > v)
                {
                    return i - 1;
                }

                if (i == size - 1)
                {
                    if (arr[i] == v || arr[i] < v)
                    {
                        return i - 1;
                    }
                }
                else if (arr[i] > v)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
