using Autofac;
using Promart.Api.Application.Queries;
using Promart.Domain.AggregatesModel.ClienteAggregate;
using Promart.Infrastructure;
using Promart.Infrastructure.Repositories;

namespace Promart.Api.Infrastructure.AutofacModules
{
    public class ApplicationModule
        : Autofac.Module
    {

        public string QueriesConnectionString { get; }

        public ApplicationModule(string qconstr)
        {
            QueriesConnectionString = qconstr;

        }

        protected override void Load(ContainerBuilder builder)
        {
            #region Queries
            builder.RegisterType<ClienteQueries>()
                .As<IClienteQueries>()
                .InstancePerLifetimeScope();
            #endregion

            #region Repository
            builder.RegisterType<ClienteRepository>()
            .As<IClienteRepository>()
            .InstancePerLifetimeScope();

            #endregion

            #region Unidad de Trabajo
            builder.RegisterType<UnitOfWork>().InstancePerLifetimeScope();
            #endregion
        }
    }
}

