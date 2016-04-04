using System;
using System.Linq;
using Seq.Apps;

namespace Seq.App.Azure
{
    public class BaseReactor : Reactor
    {
        protected static object GetValue(string value)
        {
            var returnValue = value as object;

            decimal decimalBuffer;
            int intBuffer;
            DateTime dateBuffer;

            if (value.Contains('.') && decimal.TryParse(value, out decimalBuffer))
                returnValue = decimalBuffer;
            else if (int.TryParse(value, out intBuffer))
                returnValue = intBuffer;
            else if (DateTime.TryParse(value, out dateBuffer))
                returnValue = dateBuffer;

            return returnValue;
        }
    }
}
