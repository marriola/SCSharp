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
        public NullTransformation()
            : base(string.Empty, null, null)
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
