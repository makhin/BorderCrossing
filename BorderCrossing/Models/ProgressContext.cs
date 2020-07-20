using System;
using System.Collections;
using System.Collections.Generic;

namespace BorderCrossing.Models
{
    public class ProgressArgs : EventArgs
    {
        public ProgressArgs(int count)
        {
            this.Count = count;
        }

        public int Count { get; private set; }
    }

    public class ProgressContext<T> : IEnumerable<T>
    {
        private IEnumerable<T> source;

        public ProgressContext(IEnumerable<T> source)
        {
            this.source = source;
        }

        public event EventHandler<ProgressArgs> UpdateProgress;

        protected virtual void OnUpdateProgress(int count)
        {
            EventHandler<ProgressArgs> handler = this.UpdateProgress;
            if (handler != null)
                handler(this, new ProgressArgs(count));
        }

        public IEnumerator<T> GetEnumerator()
        {
            int count = 0;
            foreach (var item in source)
            {
                // The yield holds execution until the next iteration,
                // so trigger the update event first.
                OnUpdateProgress(++count);
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
