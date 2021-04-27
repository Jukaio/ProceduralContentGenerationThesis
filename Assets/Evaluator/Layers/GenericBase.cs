using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonEvaluation.Layer
{
    public class Base<HandleIn, HandleOut, In, Out>
               : InputOutput<HandleIn[,], HandleOut[,]>
               where HandleIn : MooreCell<In>, new()
               where HandleOut : MooreCell<Out>, new()
    {
        public delegate void IndexAction(int x, int y);
        public static void for_each(Vector2Int size, IndexAction index)
        {
            for (int y = 0; y < size.y; y++) {
                for (int x = 0; x < size.x; x++)
                    index(x, y);
            }
        }

        public Base(int width, int height, In in_default, Out out_default)
        {
            var in_data = new HandleIn[width, height];
            var out_data = new HandleOut[width, height];

            this.in_default = in_default;
            this.out_default = out_default;

            Size = new Vector2Int(width, height);

            for_each(Size,
            (int x, int y) =>
            {
                in_data[x, y] = new HandleIn();
                in_data[x, y].set(new Vector2Int(x, y));
                in_data[x, y].set(in_default);
                out_data[x, y] = new HandleOut();
                out_data[x, y].set(new Vector2Int(x, y));
                out_data[x, y].set(out_default);
            });
            for_each(Size,
            (int x, int y) =>
            {
                in_data[x, y].find_and_set_neighbours(in_data[x, y].Index, in_data); // should be static...
                out_data[x, y].find_and_set_neighbours(in_data[x, y].Index, out_data); // should be static...
            });

            Input = in_data;
            Output = out_data;
        }

        public void feed_matrix_forward<ToHandleIn, ToHandleOut, ToIn, ToOut>(Base<ToHandleIn, ToHandleOut, ToIn, ToOut> to)
            where ToIn : Out
            where ToHandleIn : MooreCell<ToIn>, new()
            where ToHandleOut : MooreCell<ToOut>, new()
        {
            for_each(Size,
            (int x, int y) =>
            {
                to.Input[x, y].set((ToIn)Output[x, y].Value);
            });
        }

        public virtual void feed_forward<T, ToHandleIn, ToHandleOut, ToIn, ToOut>(T to)
            where T : Base<ToHandleIn, ToHandleOut, ToIn, ToOut>
            where ToIn : Out
            where ToHandleIn : MooreCell<ToIn>, new()
            where ToHandleOut : MooreCell<ToOut>, new()
        { }

        public virtual void reset()
        {
            for_each(Size,
            (int x, int y) =>
            {
                Input[x, y].set((In)in_default);
                Output[x, y].set((Out)out_default);
            });
        }

        public Vector2Int Size { get; private set; }
        private In in_default;
        private Out out_default;
    }
}
