// (c) DeNA Co., Ltd.

namespace Waffle.ModelProxy;

/// <summary>
/// Extension methods on <see cref="string"/> used by the Waffle source generators.
/// </summary>
internal static class StringExtensions
{
    /// <param name="self">The string to convert.</param>
    extension(string self)
    {
        /// <summary>
        /// Converts the string to a private field naming convention by prepending <c>_</c> and
        /// lower-casing the first character.  Returns the original string unchanged if it is
        /// null/empty or already starts with <c>_</c>.
        /// </summary>
        /// <returns>The private-field-formatted string.</returns>
        public string ToPrivateFieldName()
        {
            if (string.IsNullOrEmpty(self))
            {
                return self;
            }

            if (self[0] is '_')
            {
                return self;
            }

            return $"_{char.ToLower(self[0])}{self[1..]}";
        }

        /// <summary>
        /// Removes the specified prefix from the string if it starts with that prefix;
        /// otherwise returns the string unchanged.
        /// </summary>
        /// <param name="prefix">The prefix to remove.</param>
        /// <returns>The string with the prefix removed, or the original string if the prefix was not present.</returns>
        public string RemovePrefix(string prefix)
        {
            if (self.StartsWith(prefix))
            {
                return self[prefix.Length..];
            }

            return self;
        }

        /// <summary>
        /// Removes the specified suffix from the string if it ends with that suffix;
        /// otherwise returns the string unchanged.
        /// </summary>
        /// <param name="suffix">The suffix to remove.</param>
        /// <returns>The string with the suffix removed, or the original string if the suffix was not present.</returns>
        public string RemoveSuffix(string suffix)
        {
            if (self.EndsWith(suffix))
            {
                return self[..^suffix.Length];
            }

            return self;
        }
    }
}
