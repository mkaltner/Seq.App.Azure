using System;
using System.Linq;
using Seq.Apps;

namespace Seq.App.Azure
{
    public class BaseReactor : Reactor
    {
        /// <summary>
        /// GetValue attempts to transform the event property objects into concrete types prior to serialization.
        /// This allows consumers of said data to treat serialized data as their actual type instead of a string.
        /// This may not be required for all properties as it is an object but it depends on configuration, usage and other factors.
        /// I find it best to just try and parse/translate it myself to cover all scenarios.
        /// </summary>
        /// <param name="value">The value to be transformed/parsed.</param>
        /// <returns>A translated representation of the real object type instead of a string.</returns>
        protected static object GetValue(object value)
        {
            if (!(value is string))
                return value;

            var str = value.ToString();

            long longBuffer;
            if (long.TryParse(str, out longBuffer))
                return longBuffer;

            decimal decimalBuffer;
            if (decimal.TryParse(str, out decimalBuffer))
                return decimalBuffer;

            DateTime dateBuffer;
            if (DateTime.TryParse(str, out dateBuffer))
                return dateBuffer;

            return value;
        }
    }
}
