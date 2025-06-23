using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpseLib.Scripts.Context
{
    public class FunctionObject(AFunction function, int[] tags, int[] comments) : EnvironmentObject(function.Signature.ID, tags, comments)
    {
        private readonly AFunction m_Function = function;

        internal AFunction Function => m_Function;

        public override bool IsValid() => true;
    }
}
