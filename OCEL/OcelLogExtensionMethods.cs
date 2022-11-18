using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCEL
{
    public static class OcelLogExtensionMethods
    {
        /// <summary>
        /// Get the objects that the events ID's map to.
        /// </summary>
        /// <param name="event">The event</param>
        /// <param name="log">The entire OCEL log</param>
        /// <exception cref="InvalidOperationException">If any object ID is not in the OCEL log's list of objects</exception>
        /// <returns>The objects that the ID's map to</returns>
        public static IEnumerable<Object> GetObjects(this Event @event, OcelLog log)
        {
            return @event.ObjectIds.Select(oid => log.Objects.First(o => o.Id == oid));
        }
    }
}
