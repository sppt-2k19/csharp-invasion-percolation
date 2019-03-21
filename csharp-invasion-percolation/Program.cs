using System;
using System.Reflection.Metadata.Ecma335;
using C5;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Rt = ResistanceType.Type;

namespace csharp_invasion_percolation
{
    class Program
    {
        static Color[,] ToColors(Rt.FillOrResist[,] matrix, int len)
        {
            var col = new Color[len, len];
            for (int i = 0; i < len; i++)
                for (int j = 0; j < len; j++)
                    col[i,j] = matrix[i,j].IsFilled ? Color.Black : Color.White;

            return col;
        }
        
        static Bitmap ToBmp(Color[,] colors, int heightAndWidth)
        {
            var bmp = new Bitmap(heightAndWidth, heightAndWidth);
            for (int i = 0; i < heightAndWidth; i++)
                for (int j = 0; j < heightAndWidth; j++)
                    bmp.SetPixel(i,j,colors[i,j]);

            return bmp;
        }

        static Rt.FillOrResist[,] MatrixBuilder(int n, int seed, int R)
        {
            var rnd = new Random(seed);
            var arr = new Rt.FillOrResist[n,n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    
                        var val = rnd.Next() % R;
                        var resis = Rt.FillOrResist.NewResistance(val);
                        arr[i,j] = resis;
                }
            

            return arr;
        }

        static (Rt.FillOrResist[,], IntervalHeap<(int, int, int)>) 
        FindPrioQueue(Rt.FillOrResist[,] matrix, int n, IntervalHeap<(int, int, int)> queue)
        {
            var comIndex = new (int, int)[4]
            {
                (0, -1), (-1, 0), (1, 0), (0, 1)
            };

            var m = matrix;
            var q = queue;
            
            var (value,posx,posy) = q.DeleteMin();
            m[posx,posy] = Rt.FillOrResist.Filled;
            
            foreach (var pair in comIndex)
            {
                var x = pair.Item1 + posx;
                var y = pair.Item2 + posy;

                if (x < 0 || x >= n || y < 0 || y >= n) continue;
                var a = m[x,y];
                if (a.IsResistance)
                    if (!q.Exists(tuple => Rt.FillOrResist.NewResistance(tuple.Item1).Equals(a)
                                           && tuple.Item2 == x
                                           && tuple.Item3 == y))
                    {
                        int aint = ((Rt.FillOrResist.Resistance) a).Item;
                        q.Add((aint, x, y));
                    }
            }
            
            
            return (m, q);

        }

        private static Rt.FillOrResist[,]
         InvPerPrioHelper(Rt.FillOrResist[,] matMask, int n, int nfill, IntervalHeap<(int, int, int)> queue)
        {
            if(nfill == 0)
                return matMask;
            
            var (m,q) = FindPrioQueue(matMask, n, queue);
            return InvPerPrioHelper(m,n,(nfill - 1), q);
            
        }

        private static Rt.FillOrResist[,]
            InvasionPercolationPriorityQueue(int n, int nfill, int dummy)
        {
            var R = 5000;
            var matrixMask = MatrixBuilder(n, dummy, R);
            var p = new IntervalHeap<(int, int, int)> {(R * 2, n / 2, n / 2)};
            return InvPerPrioHelper(matrixMask, n, nfill, p);
        }

        public static float InvPercoBenchmark(int dummy)
        {
            var res = InvasionPercolationPriorityQueue(200, 6000, dummy);
            return res.Length;
        }
        
        
        static void Main(string[] args)
        {
            var dummy = 6;
            var f = InvPercoBenchmark(dummy);
            var mm = InvasionPercolationPriorityQueue(200, 6000, dummy);
            
            var cols = ToColors(mm, 200);
            var bmp = ToBmp(cols, 200);
            bmp.Save($"Img-{dummy}.png", ImageFormat.Png);
            
            
                
        }
    }
}