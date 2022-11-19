using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCEL
{
    internal interface IOcelHandler
    {
        static abstract OcelLog? Deserialize(string json);

        static abstract string? Serialize(OcelLog log);

        static abstract bool Validate(string json);

        static abstract (bool, IEnumerable<string>) ValidateWithErrorMessages(string json);
    }
}
