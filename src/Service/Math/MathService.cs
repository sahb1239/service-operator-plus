using System.Linq;

namespace Service
{
    public class MathService : IMathService
    {
        public OperationOutput Execute(Operation operation)
        {
            return new OperationOutput
            {
                Output = operation.Operands.Aggregate(0M, (arg1, arg2) => arg1 + arg2)
            };
        }
    }
}