using SoundChange.Parser.Nodes;

namespace SoundChange.StateMachines.RuleMachine
{
    /// <summary>
    /// Represents a transformation on an input symbol when transforming a word.
    /// </summary>
    class Transformation
    {
        /// <summary>
        /// Gets or sets the result of the transformation.
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// Gets or sets the type of node that matches the input symbol.
        /// </summary>
        public Node From { get; private set; }

        /// <summary>
        /// Gets or sets the type of node that matches the result of the transformation.
        /// </summary>
        public Node To { get; private set; }

        /// <summary>
        /// Gets or sets the literal input symbol that matched this transformation.
        /// </summary>
        public string FromLiteral { get; set; }

        public Transformation(string result, Node from, Node to)
        {
            Result = result;
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"{FromLiteral} ({From.ToString()}) → {Result} ({To.ToString()})";
        }
    }
}
