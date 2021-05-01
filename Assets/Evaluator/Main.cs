using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace DungeonEvaluation
{
    public class Export
    {
        string filename = "";

        public struct Data
        {
            public int levelID;
            public struct Space
            {
                public int passableSize;
                public int impassableSize;
                public int playableSize; //biggest space
                public int unreachableCount;
                public int unreachableSize;
            }

            public struct Room
            {
                public int count;
                public float averageSize;
                public float biggestSize;
                public float smallestSize;
                public float decisionsPerRoom;
            }

            public struct Corridor
            {
                public int count;
                public float averageSize;
                public float biggestSize;
                public float smallestSize;
            }


            public Space space;
            public Room room;
            public Corridor corridor;
        }

        public Export(string name)
        {
            filename = Application.dataPath + "/" + name + ".csv";

        }

        public void create_csv()
        {
            if (!File.Exists(filename)) {
                using (StreamWriter sw = File.CreateText(filename)) {
                    sw.WriteLine("Level ID, " +
                    "Passable Space, " +
                    "Impassable Space, " +
                    "Playble Space Size, " +
                    "Unreachable Space Count, " +
                    "Unreachable Space Size," +
                    "Room Count, " +
                    "Average Room Size, " +
                    "Biggest Room Size, " +
                    "Smallest Room Size, " +
                    "Decisions Per Room," +
                    "Corridor Count, " +
                    "Average Corridor Size, " +
                    "Biggest Corridor Size, " +
                    "Smallest Corridor Size");
                }
            }
        }
        public void wrtite_to_csv(Data data)
        {
            using (StreamWriter sw = File.AppendText(filename)) {
                sw.WriteLine(
                data.levelID + "," + 
                data.space.passableSize + "," + 
                data.space.impassableSize + "," +
                data.space.playableSize + "," + 
                data.space.unreachableCount + "," +
                data.space.unreachableSize + "," +
                data.room.count + "," + 
                data.room.averageSize + "," +
                data.room.biggestSize + "," +
                data.room.smallestSize + "," + 
                data.room.decisionsPerRoom + "," +
                data.corridor.count + "," + 
                data.corridor.averageSize + "," +
                data.corridor.biggestSize + "," + 
                data.corridor.smallestSize);
            }
        }
    }


    public class Main
    {
        public Main()
        {

        }

        public void set_generator(Generator generator)
        {
            this.generator = generator;
            var width = generator.Size.x;
            var height = generator.Size.y;

            input = new Layer.Input(width, height);
            traversability = new Layer.Traversability(width, height);
            categorisation = new Layer.Categorisation(width, height);
            output = new Layer.Output(width, height);

            export = new Export(generator.GetType().Name.ToString());
            export.create_csv();
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

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            for(int i = 0; i < 50000; i++){
                reset();

                var level = generator.Layout;


                // Create file
                // start loop

                input.start(level);
                input.feed_matrix_forward(traversability);

                traversability.analyse();

                traversability.feed_matrix_forward(categorisation);
                traversability.feed_caverns_forward(categorisation);

                categorisation.analyse();

                categorisation.feed_matrix_forward(output);

                export.wrtite_to_csv(output.end(i, this));
                // Write to file
                // continue loop

                // close file
            }
            watch.Stop();
            Debug.Log(watch.ElapsedMilliseconds);
        }

        private Generator generator;

        public Layer.Input input;
        public Layer.Traversability traversability;
        public Layer.Categorisation categorisation;
        public Layer.Output output;

        public Export export = null;
    }
}
