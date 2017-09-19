namespace CodeDojo.CircuitBreaker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class CircuitBreaker
    {

        private readonly Func<Task> protectedFunction;

        private readonly int failuresAllowed;

        private readonly TimeSpan halfOpenTimeout;
        private readonly Stopwatch halfOpenTimer = new Stopwatch();

        private readonly List<Exception> exceptions;

        public int FailureCounter => this.exceptions.Count;

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

        public async Task ExecuteAsync()
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

        private void CloseCircuit()
        {
            this.exceptions.Clear();
            this.halfOpenTimer.Reset();
        }

        private void NoteErrorOccurrence(Exception exception)
        {
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