using System;
using System.Collections.Generic;
using System.Text;

namespace SubsLtrMarker
{
    internal class Disposable:IDisposable
    {
        private Action _action;

        public Disposable(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Dispose()
        {
            _action?.Invoke();
            _action = null;
        }
    }
}
