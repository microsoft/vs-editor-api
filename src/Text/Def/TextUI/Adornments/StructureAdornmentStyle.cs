using System.Windows.Media;

namespace Microsoft.VisualStudio.Text.Adornments
{
    /// <summary>
    /// Defines a set of properties that will be used to style the default structural block tool tip.
    /// </summary>
    /// <remarks>
    /// This is a MEF component part, and should be exported with the following attributes:
    /// [Export(typeof(StructureAdornmentStyle))]
    /// [Name]
    /// [Order]
    /// </remarks>
    public class StructureAdornmentStyle
    {
        /// <summary>
        /// Gets a <see cref="Brush"/> that will be used to paint the borders in the structure block tool tip.
        /// </summary>
        public virtual Brush BorderBrush { get; protected set; }

        /// <summary>
        /// Gets a <see cref="Brush"/> that will be used to paint the background of the structure block tool tip.
        /// </summary>
        public virtual Brush BackgroundBrush { get; protected set; }
    }
}
