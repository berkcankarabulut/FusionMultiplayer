using System.Collections.Generic;

namespace BehaviourSystem
{
    public class PSelector : Node
    {
        Node[] nodeArray;
        bool ordered = false;

        public PSelector(string n)
        {
            name = n;
        }

        void OrderNodes()
        {
            nodeArray = children.ToArray();
            Sort(nodeArray, 0, children.Count - 1);
            children = new List<Node>(nodeArray);
        }

        public override Status Process()
        {
            if (!ordered)
            {
                OrderNodes();
                ordered = true;
            }

            Status childstatus = children[currentChild].Process();
            switch (childstatus)
            {
                case Status.RUNNING:
                    return Status.RUNNING;
                case Status.SUCCESS:
                    currentChild = 0;
                    ordered = false;
                    return Status.SUCCESS;
            }

            currentChild++;
            if (currentChild < children.Count) return Status.RUNNING;
            currentChild = 0;
            ordered = false;
            return Status.FAILURE;
        }

        int Partition(Node[] array, int low,
            int high)
        {
            Node pivot = array[high];

            int lowIndex = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (array[j].sortOrder <= pivot.sortOrder)
                {
                    lowIndex++;

                    (array[lowIndex], array[j]) = (array[j], array[lowIndex]);
                }
            }

            (array[lowIndex + 1], array[high]) = (array[high], array[lowIndex + 1]);

            return lowIndex + 1;
        }

        void Sort(Node[] array, int low, int high)
        {
            if (low < high)
            {
                int partitionIndex = Partition(array, low, high);
                Sort(array, low, partitionIndex - 1);
                Sort(array, partitionIndex + 1, high);
            }
        }
    }
}