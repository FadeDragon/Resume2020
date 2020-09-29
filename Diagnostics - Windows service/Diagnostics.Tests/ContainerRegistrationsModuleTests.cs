using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Diagnostics.Service;
using NUnit.Framework;

namespace Diagnostics.Tests
{
    /// <summary>
    ///   To warn and remind developers to register any newly created dependencies
    /// </summary>
    [TestFixture]
    public class ContainerRegistrationsModuleTests
    {
        [Test, Category(TestCategories.Unit)]
        public void Check_All_Types_Can_Be_Resolved()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<ContainerRegistrationsModule>();

            var container = builder.Build();

            var typesRegistered = GetTypesRegisteredInModule(container);

            foreach (var type in typesRegistered)
            {
                container.TryResolve(type, out var instance);

                Assert.That(instance, Is.Not.Null, $"Type {type.Name} was not resolved. Expected: {type.FullName}");
            }
        }

        private IEnumerable<Type> GetTypesRegisteredInModule(IContainer container)
        {
            var componentRegistry = container.ComponentRegistry;

            var typesRegistered =
            componentRegistry.Registrations.SelectMany(x => x.Services)
                             .OfType<IServiceWithType>()
                             .Where(x => x is TypedService)
                             .Cast<TypedService>()
                             .Select(x => x.ServiceType);

            return typesRegistered;
        }
    }
}
