using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoundChange.Nodes
{
    class RuleNode : Node
    {
        public List<Node> Target { get; private set; }

        public List<Node> Result { get; private set; }

        public List<Node> Environment { get; private set; }

        public RuleNode(List<Node> target, List<Node> result, List<Node> environment)
        {
            Target = target;
            Result = result;
            Environment = environment;
        }

        public override string ToString()
        {
            var target = string.Join(string.Empty, Target.Select(x => x.ToString()));
            var result = string.Join(string.Empty, Result.Select(x => x.ToString()));
            var environment = string.Join(string.Empty, Environment.Select(x => x.ToString()));

            return string.Format("{0}/{1}/{2}", target, result, environment);
        }
    }
}
