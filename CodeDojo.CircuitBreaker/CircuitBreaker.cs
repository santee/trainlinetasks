namespace CodeDojo.CircuitBreaker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class CircuitBreaker : IDisposable
    {
        private readonly Func<Task> protectedFunction;

        private readonly int failuresAllowed;

        private readonly TimeSpan halfOpenTimeout;
        private readonly Stopwatch halfOpenTimer = new Stopwatch();

        private readonly List<Exception> exceptions;

        private readonly SemaphoreSlim csExecution = new SemaphoreSlim(1, 1);

        public CircuitBreaker(Func<Task> protectedFunction, int failuresAllowed, TimeSpan halfOpenTimeout)
        {
            if (failuresAllowed < 0)
            {
                throw new ArgumentException("Failure threshold must be a positive number (0 for 'no failures allowed')", nameof(failuresAllowed));
            }

            this.protectedFunction = protectedFunction;
            this.failuresAllowed = failuresAllowed;
            this.halfOpenTimeout = halfOpenTimeout;
            this.exceptions = new List<Exception>(failuresAllowed);
        }

        public int FailureCounter { get; private set; } = 0;

        public async Task ExecuteAsync()
        {
            await this.csExecution.WaitAsync().ConfigureAwait(false);
            try
            {
                this.ThrowIfCircuitOpen();

                try
                {
                    await this.protectedFunction().ConfigureAwait(false);
                    this.CloseCircuit();
                }
                catch (Exception ex)
                {
                    this.NoteErrorOccurrence(ex);
                    this.ThrowIfCircuitOpen();
                }
            }
            finally
            {
                this.csExecution.Release();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.csExecution.Dispose();
            }
        }

        private void CloseCircuit()
        {
            this.FailureCounter = 0;
            this.exceptions.Clear();
            this.halfOpenTimer.Reset();
        }

        private void NoteErrorOccurrence(Exception exception)
        {
            this.FailureCounter++;
            this.exceptions.Add(exception);
            this.halfOpenTimer.Restart();
        }

        private void ThrowIfCircuitOpen()
        {
            if (this.halfOpenTimer.Elapsed > this.halfOpenTimeout)
            {
                return;
            }

            if (this.exceptions.Count > this.failuresAllowed)
            {
                throw new AggregateException(this.exceptions);
            }
        }
    }
}