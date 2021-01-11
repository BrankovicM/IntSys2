using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Filters;
using GenericParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntSys2
{
    class Program
    {
        static Codification codebook;
        static DataTable data;
        static void Main(string[] args)
        {
            data = new DataTable("What should I do today?");

            data.Columns.Add("Weather");
            data.Columns.Add("Time of the day");
            data.Columns.Add("Homework");
            data.Columns.Add("Bored");
            data.Columns.Add("Activity");

            using (GenericParser parser = new GenericParser())
            {
                parser.SetDataSource("C:\\Users\\Marija\\source\\repos\\IntSys2\\WhatToDo4.csv");

                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.TextQualifier = '\"';

                while (parser.Read())
                {
                    data.Rows.Add(parser[0].ToString(), parser[1].ToString(), parser[2].ToString(), parser[3].ToString(), parser[4].ToString());
                }
            }

            codebook = new Codification(data);

            DataTable symbols = codebook.Apply(data);
            int[][] inputs = symbols.ToArray<int>(data.Columns[0].ColumnName, data.Columns[1].ColumnName, data.Columns[2].ColumnName, data.Columns[3].ColumnName);
            int[] outputs = symbols.ToArray<int>(data.Columns[4].ColumnName);

            var id3learning = new ID3Learning()
            {
                new DecisionVariable(data.Columns[0].ColumnName, 3),
                new DecisionVariable(data.Columns[1].ColumnName, 2),
                new DecisionVariable(data.Columns[2].ColumnName, 2),
                new DecisionVariable(data.Columns[3].ColumnName, 2)
            };

            DecisionTree id3Tree = id3learning.Learn(inputs, outputs);

            PrintTree(id3Tree.Root, "", id3Tree.Root.IsLeaf);

            int[] query = codebook.Transform(new string[,]
            {
                { data.Columns[0].ColumnName, "Rainy" },
                { data.Columns[1].ColumnName, "Daytime" },
                { data.Columns[2].ColumnName, "No homework" },
                { data.Columns[3].ColumnName, "Bored" }
            });

            int predicted = id3Tree.Decide(query);

            string answer = codebook.Revert(data.Columns[4].ColumnName, predicted);
            Console.WriteLine("-------------------------");
            Console.WriteLine("Id3 resenje: " + answer);
            Console.WriteLine("-------------------------");

            //var c45learning = new C45Learning()
            //{
            //    new DecisionVariable(data.Columns[0].ColumnName, 3),
            //    new DecisionVariable(data.Columns[1].ColumnName, 2),
            //    new DecisionVariable(data.Columns[2].ColumnName, 2),
            //    new DecisionVariable(data.Columns[3].ColumnName, 2)
            //};

            //DecisionTree c45Tree = c45learning.Learn(inputs, outputs);

            //PrintTree(c45Tree.Root, "", c45Tree.Root.IsLeaf);

            //int c45predicted = c45Tree.Decide(query);

            //answer = codebook.Revert(data.Columns[4].ColumnName, c45predicted);
            //Console.WriteLine("-------------------------");
            //Console.WriteLine("C45  resenje: " + answer);
            //Console.WriteLine("-------------------------");

            Console.ReadLine();
        }

        public static void PrintTree(DecisionNode node, String indent, bool last)
        {
            Console.WriteLine();
            string[] spstr = { " == " };
            var values = node.ToString().Split(spstr, StringSplitOptions.None);
            string nodeValue = node.ToString();
            if (values.Count() == 2)
                nodeValue = values[0] + " == " + codebook.Revert(values[0].Trim(), Int32.Parse(values[1].Trim())).ToString();
            Console.Write(indent + "+- " + nodeValue);
            indent += last ? "   " : "|  ";

            if (last)
            {
                Console.Write(" => " + codebook.Revert("Activity", (int)node.Output));
            }

            for (int i = 0; i < node.Branches.Count; i++)
            {

                PrintTree(node.Branches[i], indent, node.Branches[i].IsLeaf);

            }
        }
    }
}
