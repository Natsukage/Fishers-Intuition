using System.Collections;
using Nhaama.FFXIV.Actor.Model;

namespace Nhaama.FFXIV
{
    public class ActorTableEnumerator : IEnumerator
    {
        private readonly ActorEntry[] _entries;
        private int _pos = 0;
        
        public ActorTableEnumerator(ActorEntry[] entries)
        {
            _entries = entries;
        }

        public bool MoveNext()
        {
            _pos++;
            return _pos != _entries.Length;
        }

        public void Reset()
        {
            _pos = 0;
        }

        public object Current => _entries[_pos];
    }
}