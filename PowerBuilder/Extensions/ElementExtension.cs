using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerBuilder.Extensions {
    public static class ElementExtension {

        public static bool IsSameOrSubclass(this Element E, Type Candidate) {
            return E.GetType().IsSubclassOf(Candidate) || Candidate == E.GetType();
        }
    }
}
