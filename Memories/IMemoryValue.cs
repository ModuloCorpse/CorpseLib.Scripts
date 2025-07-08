using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpseLib.Scripts.Memories
{
    public interface IMemoryValue
    {
        public IMemoryValue Clone();
        public bool Equals(IMemoryValue other);
    }
}
