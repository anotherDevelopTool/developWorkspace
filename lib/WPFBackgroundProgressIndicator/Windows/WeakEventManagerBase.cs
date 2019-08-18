// Copyright (c) 2011 Abraham Heidebrecht
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the 
// following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Windows;

namespace Heidesoft.Components.Windows
{
    /// <summary>
    /// Base class for weak event managers.
    /// </summary>
    /// <typeparam name="TManager">The type of the weak event manager.</typeparam>
    /// <typeparam name="TEventRaiser">The type of the class that raises the event.</typeparam>
    /// <remarks>
    /// Based on the idea presented by <a href="http://wekempf.spaces.live.com/">William Kempf</a>
    /// in his article <a href="http://wekempf.spaces.live.com/blog/cns!D18C3EC06EA971CF!373.entry">WeakEventManager</a>.
    /// </remarks>
    public abstract class WeakEventManagerBase<TManager, TEventRaiser> : WeakEventManager
        where TManager : WeakEventManagerBase<TManager, TEventRaiser>, new()
        where TEventRaiser : class
    {
        private static TManager Current
        {
            get
            {
                var manager = WeakEventManager.GetCurrentManager(typeof(TManager)) as TManager;

                if (manager == null)
                {
                    manager = new TManager();
                    WeakEventManager.SetCurrentManager(typeof(TManager), manager);
                }

                return manager;
            }
        }

        public static void AddListener(TEventRaiser source, IWeakEventListener listener)
        {
            Current.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(TEventRaiser source, IWeakEventListener listener)
        {
            Current.ProtectedRemoveListener(source, listener);
        }

        protected override void StartListening(object source)
        {
            Start(source as TEventRaiser);
        }

        protected override void StopListening(object source)
        {
            Stop(source as TEventRaiser);
        }

        protected abstract void Start(TEventRaiser eventSource);
        protected abstract void Stop(TEventRaiser eventSource);
    }
}
