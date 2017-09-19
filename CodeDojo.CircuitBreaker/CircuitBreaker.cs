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

        private readonly int failuresThreshold;

        private readonly TimeSpan halfOpenTimeout;
        private readonly Stopwatch halfOpenTimer = new Stopwatch();

        private readonly List<Exception> exceptions;

        public int FailureCounter => this.exceptions.Count;

        public CircuitBreaker(Func<Task> protectedFunction, int failuresThreshold, TimeSpan halfOpenTimeout)
        {
            if (failuresThreshold <= 0)
            {
                throw new ArgumentException("Failure threshold must be a positive number (1 for 'no failures allowed')", nameof(failuresThreshold));
            }

            this.protectedFunction = protectedFunction;
            this.failuresThreshold = failuresThreshold;
            this.halfOpenTimeout = halfOpenTimeout;
            this.exceptions = new List<Exception>(failuresThreshold);
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

            if (this.exceptions.Count >= this.failuresThreshold)
            {
                throw new AggregateException(this.exceptions);
            }
        }
    }
}