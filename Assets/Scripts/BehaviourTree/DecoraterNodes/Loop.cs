namespace BehaviourSystem
{
    public class Loop : Node
    {
        BehaviourTree dependancy;

        public Loop(string n, BehaviourTree d)
        {
            name = n;
            dependancy = d;
        }

        public override Status Process()
        {
            if (dependancy.Process() == Status.FAILURE)
            {
                return Status.SUCCESS;
            }

            Status childstatus = children[currentChild].Process();
            switch (childstatus)
            {
                case Status.RUNNING:
                    return Status.RUNNING;
                case Status.FAILURE:
                    return childstatus;
            }

            currentChild++;
            if (currentChild >= children.Count)
            {
                currentChild = 0;
            }

            return Status.RUNNING;
        }
    }
}