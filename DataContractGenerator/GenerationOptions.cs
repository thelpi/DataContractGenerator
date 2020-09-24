namespace DataContractGenerator
{
    /// <summary>
    /// Options of generation.
    /// </summary>
    public class GenerationOptions
    {
        /// <summary>
        /// Default maximal recursion.
        /// </summary>
        public const int MAX_RECURSION_DEPTH = 20;
        /// <summary>
        /// Default minimal list size.
        /// </summary>
        public const int MIN_LIST_COUNT = 1;
        /// <summary>
        /// Default maximal list size.
        /// </summary>
        public const int MAX_LIST_COUNT = 10;
        /// <summary>
        /// Default minimal string length.
        /// </summary>
        public const int MIN_STRING_LENGTH = 3;
        /// <summary>
        /// Default maximal string length.
        /// </summary>
        public const int MAX_STRING_LENGTH = 20;

        /// <summary>
        /// Maximal recursion depth.
        /// </summary>
        public int MaximalRecursionDepth { get; set; } = MAX_RECURSION_DEPTH;
        /// <summary>
        /// Minimal list size.
        /// </summary>
        public int MinListCount { get; set; } = MIN_LIST_COUNT;
        /// <summary>
        /// Maximal list size.
        /// </summary>
        public int MaxListCount { get; set; } = MAX_LIST_COUNT;
        /// <summary>
        /// Minimal string length.
        /// </summary>
        public int MinStringLength { get; set; } = MIN_STRING_LENGTH;
        /// <summary>
        /// Maximal string length.
        /// </summary>
        public int MaxStringLength { get; set; } = MAX_STRING_LENGTH;
    }
}
