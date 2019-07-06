using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace HRS.HealthCheckMonitor.UnitTests.Core
{
    public class XUnitTestLogger : ILogger
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly string _name;
        private IScope _scope;

        public XUnitTestLogger(ITestOutputHelper outputHelper, string name)
        {
            _outputHelper = outputHelper;
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var scope = new Scope<TState>(this, state, _scope);
            _scope = scope;
            return scope;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _outputHelper.WriteLine($"Name:{_name} Scope:{_scope} Level:{logLevel} EventId:{eventId} Message:{formatter(state, exception)}");
        }

        private interface IScope : IDisposable
        {}

        private class Scope<T> : IScope
        {
            private readonly XUnitTestLogger _logger;
            private readonly IScope _parent;
            private readonly T _state;
            private bool _isDisposed;

            public Scope(XUnitTestLogger logger, T state, IScope parent)
            {
                _logger = logger;
                _parent = parent;
                _state = state;
            }

            public override string ToString()
            {
                return $"{_parent}:{_state}";
            }

            public void Dispose()
            {
                if(!_isDisposed)
                {
                    _isDisposed = true;
                    _logger._scope = _parent;
                }
            }
        }
    }

    public class XUnitTestLogger<T> : XUnitTestLogger, ILogger<T>
    {
        public XUnitTestLogger(ITestOutputHelper outputHelper) : base(outputHelper, typeof(T).Name)
        {}
    }
}
