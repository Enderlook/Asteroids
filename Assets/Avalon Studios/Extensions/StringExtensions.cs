using UnityEngine;

namespace AvalonStudios.Additions.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns the correct name of the auto serialize property.
        /// </summary>
        /// <returns>
        /// Returns the name of the auto property without "<T>k__BackingField".
        /// </returns>
        public static string RenameAutoProperty(this string name)
        {
            string tempName = name.Replace("k__BackingField", "");
            if (tempName.Length == name.Length)
                tempName = name.Replace("k__Backing Field", "");
            string newName = tempName.Trim('<', '>');
            char firstLetter = newName[0];
            return char.ToUpper(firstLetter) + newName.Substring(1, newName.Length - 1);
        }

        /// <summary>
        /// Returns the name with the format of auto property.
        /// </summary>
        /// <returns>
        /// Returns the name of the property with the format of auto property.
        /// </returns>
        public static string RenamePropertyToAutoProperty(this string name)
            => $"<{name}>k__BackingField";

        /// <summary>
        /// This extension method is specific for Auto Properties names.
        /// Returns a <seealso cref="bool"/> result.
        /// </summary>
        /// <returns>
        /// True if the <seealso cref="string"/> contains the "k__BackingField".
        /// False if the <seealso cref="string"/> not contains the "k__BackingField".
        /// </returns>
        public static bool ContainsBackingField(this string s)
        {
            if (s.Contains("k__BackingField"))
                return true;
            else if (s.Contains("k__Backing Field"))
                return true;
            else
                return false;
        }

        public static Vector3 ToVector3(this string s)
        {
            Vector3 result = Vector3.zero;
            string trimResult = s.Trim('(', ')');
            string[] splitResult = trimResult.Split(',');
            result.Set(float.Parse(splitResult[0], System.Globalization.CultureInfo.InvariantCulture), 
                float.Parse(splitResult[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(splitResult[2], System.Globalization.CultureInfo.InvariantCulture));
            return result;
        }
    }
}
