namespace CodeDojo.CircuitBreaker.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// As a systems maintainer
    /// I want my calls to be resilient to failure
    /// So I don't need to turn features on and off

    /// - Implement the circuit breaker pattern
    ///    - Fault tolerance should be configurable
    ///    - Half-open timeouts should be configurable
    /// </summary>
    [TestFixture]
    public class CircuitBreaker
    {

        /// <summary>
        /// As a developer
        /// I want to count failures when I make calls
        /// A failure count is incremented
        /// </summary>
        [Test]
        public void MonitoringFailure()
        {

        }

        /// <summary>
        /// As a developer
        /// When my failure count exceeds a configured tolerance
        /// All subsequent calls throw an exception and block the call
        /// </summary>
        [Test]
        public void OpeningCircuit()
        {
            
        }

        /// <summary>
        /// As a developer
        /// When my code is blocking calls and a configured timeout passes
        /// I allow a single call to proceed
        /// - Single call succeeding closes circuit
        /// - Single call failing resets some kind of timeout
        /// </summary>
        [Test]
        public void HalfOpenedState()
        {

        }
    }
}