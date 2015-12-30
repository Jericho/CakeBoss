﻿#region Using Statements
    using System;
    using System.Collections.Generic;

    using global::Nancy;
    using global::Nancy.Bootstrapper;
    using global::Nancy.Diagnostics;
#endregion



namespace LightInject.Nancy
{
    /// <summary>
    /// A Nancy bootstrapper for LightInject.
    /// </summary>
    public class LightInjectNancyBootstrapper : NancyBootstrapperBase<IServiceContainer>
    {
        private IServiceContainer serviceContainer;

        /// <summary>
        /// Gets an <see cref="INancyModule"/> instance.
        /// </summary>
        /// <param name="moduleType">The type of <see cref="INancyModule"/> to get.</param>
        /// <param name="context">The current <see cref="NancyContext"/>.</param>
        /// <returns>An <see cref="INancyModule"/> instance.</returns>
        public override INancyModule GetModule(Type moduleType, NancyContext context)
        {
            EnsureScopeIsStarted(context);
            return serviceContainer.GetInstance<INancyModule>(moduleType.FullName);
        }

        /// <summary>
        /// Gets all <see cref="INancyModule"/> instances.
        /// </summary>
        /// <param name="context">The current <see cref="NancyContext"/>.</param>
        /// <returns>All <see cref="INancyModule"/> instances.</returns>
        public override IEnumerable<INancyModule> GetAllModules(NancyContext context)
        {
            EnsureScopeIsStarted(context);
            return serviceContainer.GetAllInstances<INancyModule>();
        }

        /// <summary>
        /// Gets the diagnostics for initialization.
        /// </summary>
        /// <returns>An <see cref="IDiagnostics"/> instance.</returns>      
        protected override IDiagnostics GetDiagnostics()
        {
            return serviceContainer.GetInstance<IDiagnostics>();
        }

        /// <summary>
        /// Gets the <see cref="INancyEngine"/> instance.
        /// </summary>
        /// <returns><see cref="INancyEngine"/></returns>
        protected override INancyEngine GetEngineInternal()
        {
            return serviceContainer.GetInstance<INancyEngine>();
        }

        /// <summary>
        /// Initializes the <see cref="IServiceContainer"/> instance.
        /// </summary>
        /// <returns><see cref="IServiceContainer"/>.</returns>
        protected override IServiceContainer GetApplicationContainer()
        {
            serviceContainer = GetServiceContainer();
            foreach (var requestStartupType in RequestStartupTasks)
            {
                serviceContainer.Register(typeof(IRequestStartup), requestStartupType, requestStartupType.FullName);
            }

            return serviceContainer;
        }

        /// <summary>
        /// Registers the <see cref="INancyModuleCatalog"/> into the underlying <see cref="IServiceContainer"/> instance.
        /// </summary>
        /// <param name="container">The <see cref="IServiceContainer"/> to register into.</param>
        protected override void RegisterBootstrapperTypes(IServiceContainer container)
        {
            container.RegisterInstance<INancyModuleCatalog>(this);
        }

        /// <summary>
        /// Registers the <paramref name="typeRegistrations"/> into the underlying <see cref="IServiceContainer"/>.
        /// </summary>
        /// <param name="container">The <see cref="IServiceContainer"/> to register into.</param>
        /// <param name="typeRegistrations">Each <see cref="TypeRegistration"/> represents a service 
        /// to be registered.</param>
        protected override void RegisterTypes(IServiceContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            foreach (var typeRegistration in typeRegistrations)
            {
                switch (typeRegistration.Lifetime)
                {
                    case Lifetime.Transient:
                        RegisterTransient(typeRegistration.RegistrationType, typeRegistration.ImplementationType, string.Empty);
                        break;
                    case Lifetime.Singleton:
                        RegisterSingleton(typeRegistration.RegistrationType, typeRegistration.ImplementationType, string.Empty);
                        break;
                    case Lifetime.PerRequest:
                        RegisterPerRequest(typeRegistration.RegistrationType, typeRegistration.ImplementationType, string.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets all registered application startup tasks
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances.
        /// </returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return serviceContainer.GetAllInstances<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all <see cref="IRequestStartup"/> instances.
        /// </summary>
        /// <param name="container">The target <see cref="IServiceContainer"/>.</param>
        /// <param name="requestStartupTypes">Not used in this method.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IRequestStartup"/> instances.</returns>
        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(IServiceContainer container, Type[] requestStartupTypes)
        {
            return container.GetAllInstances<IRequestStartup>();
        }

        /// <summary>
        /// Gets all <see cref="IRegistrations"/> instances.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IRegistrations"/> instances.</returns>
        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return serviceContainer.GetAllInstances<IRegistrations>();
        }

        /// <summary>
        /// Registers multiple implementations of a given interface.
        /// </summary>
        /// <param name="container">The <see cref="IServiceContainer"/> to register into.</param>
        /// <param name="collectionTypeRegistrations">A list of <see cref="CollectionTypeRegistration"/> instances
        /// where each instance represents an abstraction and its implementations.</param>
        protected override void RegisterCollectionTypes(IServiceContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (Type implementingType in collectionTypeRegistration.ImplementationTypes)
                {
                    switch (collectionTypeRegistration.Lifetime)
                    {
                        case Lifetime.Transient:
                            RegisterTransient(collectionTypeRegistration.RegistrationType, implementingType, implementingType.FullName);
                            break;
                        case Lifetime.Singleton:
                            RegisterSingleton(collectionTypeRegistration.RegistrationType, implementingType, implementingType.FullName);
                            break;
                        case Lifetime.PerRequest:
                            RegisterPerRequest(collectionTypeRegistration.RegistrationType, implementingType, implementingType.FullName);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Register the given <paramref name="moduleRegistrationTypes"/> into the <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The <see cref="IServiceContainer"/> to register into.</param>
        /// <param name="moduleRegistrationTypes">The list of <see cref="ModuleRegistration"/> that 
        /// represents an <see cref="INancyModule"/> registration.</param>
        protected override void RegisterModules(IServiceContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                container.Register(
                    typeof(INancyModule),
                    moduleRegistrationType.ModuleType,
                    moduleRegistrationType.ModuleType.FullName,
                    new PerScopeLifetime());
            }
        }

        /// <summary>
        /// Register the given instances into the container
        /// </summary>
        /// <param name="container">The <see cref="IServiceContainer"/> to register into.</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(IServiceContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
            {
                container.RegisterInstance(
                    instanceRegistration.RegistrationType,
                    instanceRegistration.Implementation);
            }
        }

        /// <summary>
        /// Gets all <see cref="IRequestStartup"/> instances from the <see cref="IServiceContainer"/>
        /// and calls the <see cref="IRequestStartup.Initialize"/> method.
        /// </summary>
        /// <param name="context">The current <see cref="NancyContext"/>.</param>
        /// <returns><see cref="IPipelines"/>.</returns>
        protected override IPipelines InitializeRequestPipelines(NancyContext context)
        {
            var pipelines = new Pipelines(ApplicationPipelines);
            EnsureScopeIsStarted(context);

            var requestStartupTasks = serviceContainer.GetAllInstances<IRequestStartup>();
            foreach (var requestStartupTask in requestStartupTasks)
            {
                requestStartupTask.Initialize(pipelines, context);
            }

            return pipelines;
        }

        /// <summary>
        /// Returns the <see cref="IServiceContainer"/> instance.
        /// </summary>
        /// <returns><see cref="IServiceContainer"/>.</returns>
        protected virtual IServiceContainer GetServiceContainer()
        {
            var container = new ServiceContainer();
            container.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
            return container;
        }

        private void EnsureScopeIsStarted(NancyContext context)
        {
            object contextObject;
            context.Items.TryGetValue("LightInjectScope", out contextObject);
            var scope = contextObject as Scope;

            if (scope == null)
            {
                scope = serviceContainer.BeginScope();
                context.Items["LightInjectScope"] = scope;
            }
        }

        private void RegisterTransient(Type serviceType, Type implementingType, string serviceName)
        {
            if (typeof(IDisposable).IsAssignableFrom(implementingType))
            {
                serviceContainer.Register(serviceType, implementingType, serviceName, new PerRequestLifeTime());
            }
            else
            {
                serviceContainer.Register(serviceType, implementingType, serviceName);
            }
        }

        private void RegisterPerRequest(Type serviceType, Type implementingType, string serviceName)
        {
            serviceContainer.Register(serviceType, implementingType, serviceName, new PerScopeLifetime());
        }

        private void RegisterSingleton(Type serviceType, Type implementingType, string serviceName)
        {
            serviceContainer.Register(serviceType, implementingType, serviceName, new PerContainerLifetime());
        }
    }
}