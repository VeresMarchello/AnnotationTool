using System.Xml.Serialization;

namespace AnnotationTool.Model
{
    public enum MarkingType
    {
        [XmlEnum("None")]
        None = 0,
        /// <summary>
        /// Általános metszés
        /// </summary>
        [XmlEnum("GeneralPruning")]
        GeneralPruning = 1,
        /// <summary>
        /// Bizonytalan metszés
        /// </summary>
        [XmlEnum("UncertainPruning")]
        UncertainPruning,
        /// <summary>
        /// Tőből metszés
        /// </summary>
        [XmlEnum("PruningFromStems")]
        PruningFromStems,
    }
}
