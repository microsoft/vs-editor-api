using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media
{
    public class GeometryGroup : System.Windows.Media.Geometry
    {
        public List<Geometry> Children { get; } = new List<Geometry>();

        public override Rect Bounds
        {
            get
            {
                if (Children.Count == 0)
                    return Rect.Empty;
                var union = Children[0].Bounds;
                foreach (var c in Children.Skip(1))
                    union.Union(c.Bounds);
                return union;
            }
        }
        public override bool IsEmpty()
        {
            return !Children.Any(c => !c.IsEmpty());
        }
    }
}