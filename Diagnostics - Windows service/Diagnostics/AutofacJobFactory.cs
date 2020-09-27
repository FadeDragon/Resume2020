using System.Diagnostics.CodeAnalysis;
using Autofac;
using FluentScheduler;

namespace Diagnostics.Service
{
    /// <summary>
    /// Implementation of Autofac Factory for FluentScheduler to resolve dependencies
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AutofacJobFactory : IJobFactory
    {
        private readonly IContainer _container;

        public AutofacJobFactory(IContainer container)
        {
            _container = container;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _container.Resolve<T>();
        }
    }
}
