using SoundChange.Parser.Nodes;

namespace SoundChange.StateMachines.RuleMachine
{
    /// <summary>
    /// Represents a transformation that produces no output.
    /// </summary>
    /// <remarks>
    /// This type of transformation is created by the state machine generator to keep <c>ApplyTo</c> from
    /// re-emitting an input symbol when the state machine is in the middle of matching a multi-character
    /// segment.
    /// </remarks>
    class NullTransformation : Transformation
    {
        public NullTransformation(Node from)
            : base(string.Empty, from, null)
        {
        }

        public override string ToString()
        {
            return From is UtteranceNode uNode
                ? $"{FromLiteral} → Ø"
                : $"{FromLiteral} ({From}) → Ø";
        }
    }
}
