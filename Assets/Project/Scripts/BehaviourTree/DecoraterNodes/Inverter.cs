namespace BehaviourSystem
{
    public class Inverter : Node
    {
        public Inverter(string n)
        {
            name = n;
        }

        public override Status Process()
        {
            Status childstatus = children[0].Process();
            return childstatus switch
            {
                Status.RUNNING => Status.RUNNING,
                Status.FAILURE => Status.SUCCESS,
                _ => Status.FAILURE
            };
        }
    }
}