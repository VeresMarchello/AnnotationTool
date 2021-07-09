namespace AnnotationTool.Model
{
    public enum MarkingType
    {
        /// <summary>
        /// Általános metszés
        /// </summary>
        GeneralPruning = 1,
        /// <summary>
        /// Bizonytalan metszés
        /// </summary>
        UncertainPruning,
        /// <summary>
        /// Tőből metszés
        /// </summary>
        PruningFromStems,
    }
}
