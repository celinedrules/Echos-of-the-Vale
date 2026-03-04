// Done
using Sirenix.OdinInspector;

namespace Core.Attributes
{
    public class ColorFoldoutGroupAttribute : FoldoutGroupAttribute
    {
        public float R, G, B, A;

        public ColorFoldoutGroupAttribute(string groupName, float r, float g, float b, float a = 1f) : base(groupName)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }
}