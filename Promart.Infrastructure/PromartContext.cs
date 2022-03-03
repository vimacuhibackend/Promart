using MediatR;
using Microsoft.EntityFrameworkCore;
using Promart.Domain.AggregatesModel.ClienteAggregate;
using Promart.Domain.SeedWork;
using Promart.Infrastructure.EntityConfigurations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Promart.Infrastructure
{
    public class PromartContext : DbContext, IUnitOfWork
    {
        public DbSet<Cliente> Cliente { get; set; }

        private readonly IMediator _mediator;

        private PromartContext(DbContextOptions<PromartContext> options) : base(options) { }

        public PromartContext(DbContextOptions<PromartContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

            System.Diagnostics.Debug.WriteLine("ClienteContext::ctor ->" + this.GetHashCode());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           optionsBuilder.UseMySQL("Server=azr-promart-2022.mysql.database.azure.com;Initial Catalog=db_testpromart;Persist Security Info=False; Connection Timeout=30; User ID=devch@azr-promart-2022;Password=12345Aa@; port=3306");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ClienteEntityTypeConfiguration());
        }


        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            await _mediator.DispatchDomainEventsAsync(this);

            var result = await base.SaveChangesAsync();
            return true;
        }
    }


    public static class Schema
    {
        public static string Dbo = "dbo";
    }
}

