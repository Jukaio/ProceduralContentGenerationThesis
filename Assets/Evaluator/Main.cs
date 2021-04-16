using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DungeonEvaluation
{
    public class Main
    {
        public Main(Generator generator)
        {
            var width = generator.Size.x;
            var height = generator.Size.y;

            input = new Layer.Input(width, height);
            traversability = new Layer.Traversability(width, height);
            categorisation = new Layer.Categorisation(width, height);
            output = new Layer.Output(width, height);

            this.generator = generator;
        }

        public void reset()
        {
            input.reset();
            traversability.reset();
            categorisation.reset();
            output.reset();

            generator.reset();
            generator.generate();
        }

        public void run()
        {
            reset();

            var level = generator.Layout;

            input.start(level);
            input.feed_matrix_forward(traversability);

            traversability.analyse();
            traversability.feed_matrix_forward(categorisation);
            traversability.feed_caverns_forward(categorisation);

            categorisation.analyse();
            categorisation.feed_matrix_forward(output);

            output.end();
        }

        private Generator generator;

        public Layer.Input input;
        public Layer.Traversability traversability;
        public Layer.Categorisation categorisation;
        public Layer.Output output;
    }
}
