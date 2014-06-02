using System;

namespace MiniMax.Attributes
{
    public class MiniMaxTargetAttribute : Attribute
    {
        public MiniMaxTargetAttribute(Target target)
        {
            Target = target;
        }

        public Target Target { get; set; }
    }
}