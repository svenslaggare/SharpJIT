using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// Helper method for settings
    /// </summary>
    public static class SettingsHelpers
    {
        /// <summary>
        /// Returns the value for the given setting
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="settings">The settings</param>
        /// <param name="key">The key</param>
        /// <returns>The value or null</returns>
        public static T? GetSetting<T>(this IDictionary<string, object> settings, string key)
            where T : struct
        {
            object setting;

            if (settings.TryGetValue(key, out setting))
            {
                return (T)setting;
            }

            return null;
        }
    }
}
