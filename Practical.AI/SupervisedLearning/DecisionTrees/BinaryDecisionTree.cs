using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Practical.AI.SupervisedLearning.DecisionTrees
{
    public class BinaryDecisionTree
    {
        private BinaryDecisionTree LeftChild { get; set; }
        private BinaryDecisionTree RightChild { get; set; }
        private int Value { get; set; }

        public BinaryDecisionTree()
        { }
    }
}
