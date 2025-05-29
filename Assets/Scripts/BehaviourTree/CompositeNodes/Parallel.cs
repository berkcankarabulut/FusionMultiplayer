using System.Collections.Generic;

namespace BehaviourSystem
{
    public class Parallel : Node
    {
        public enum Policy
        {
            RequireOne,  
            RequireAll,  
            RequireCount  
        }

        public Policy successPolicy = Policy.RequireOne;
        public Policy failurePolicy = Policy.RequireAll;
        public int requiredSuccessCount = 1;
        public int requiredFailureCount = 1;

        private List<Status> childrenStatus;
        private int successCount = 0;
        private int failureCount = 0;
        private int runningCount = 0;

        public Parallel(string n) : base(n)
        {
            childrenStatus = new List<Status>();
        }

        public Parallel(string n, Policy success, Policy failure) : base(n)
        {
            successPolicy = success;
            failurePolicy = failure;
            childrenStatus = new List<Status>();
        }

        public Parallel(string n, int successCount, int failureCount) : base(n)
        {
            successPolicy = Policy.RequireCount;
            failurePolicy = Policy.RequireCount;
            requiredSuccessCount = successCount;
            requiredFailureCount = failureCount;
            childrenStatus = new List<Status>();
        }

        private void InitializeChildrenStatus()
        {
            if (childrenStatus.Count == children.Count) return;
            childrenStatus.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                childrenStatus.Add(Status.RUNNING);
            }
        }

        public override Status Process()
        {
            InitializeChildrenStatus();

            successCount = 0;
            failureCount = 0;
            runningCount = 0;

            for (int i = 0; i < children.Count; i++)
            {
                if (childrenStatus[i] == Status.RUNNING)
                {
                    childrenStatus[i] = children[i].Process();
                }

                switch (childrenStatus[i])
                {
                    case Status.SUCCESS:
                        successCount++;
                        break;
                    case Status.FAILURE:
                        failureCount++;
                        break;
                    case Status.RUNNING:
                        runningCount++;
                        break;
                }
            }

            if (CheckSuccessPolicy())
            {
                Reset();
                return Status.SUCCESS;
            }

            if (CheckFailurePolicy())
            {
                Reset();
                return Status.FAILURE;
            }

            if (runningCount > 0)
            {
                return Status.RUNNING;
            }

            Reset();
            return Status.FAILURE;
        }

        private bool CheckSuccessPolicy()
        {
            switch (successPolicy)
            {
                case Policy.RequireOne:
                    return successCount >= 1;
                case Policy.RequireAll:
                    return successCount >= children.Count;
                case Policy.RequireCount:
                    return successCount >= requiredSuccessCount;
                default:
                    return false;
            }
        }

        private bool CheckFailurePolicy()
        {
            switch (failurePolicy)
            {
                case Policy.RequireOne:
                    return failureCount >= 1;
                case Policy.RequireAll:
                    return failureCount >= children.Count;
                case Policy.RequireCount:
                    return failureCount >= requiredFailureCount;
                default:
                    return false;
            }
        }

        public override void Reset()
        {
            base.Reset();
            InitializeChildrenStatus();
            for (int i = 0; i < childrenStatus.Count; i++)
            {
                childrenStatus[i] = Status.RUNNING;
            }

            successCount = 0;
            failureCount = 0;
            runningCount = 0;
        }
    }
}