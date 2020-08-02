using Microsoft.EntityFrameworkCore;

namespace BillManagerServerless.Data
{
    public class BillManagerDBContext : DbContext
    {
        public BillManagerDBContext(DbContextOptions<BillManagerDBContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Person { get; set; }
        public DbSet<Bill> Bill { get; set; }
        public DbSet<PersonBillShare> PersonBill { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PersonBillShare>()
                .HasOne(x => x.Person)
                .WithMany(x => x.PersonBillShares)
                .HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.SetNull);
                //.WillCascadeOnDelete(false);


            builder.Entity<PersonBillShare>()
                .HasOne(x => x.Bill)
                .WithMany(x => x.PersonBillShares)
                .HasForeignKey(x => x.BillId);
        }
    }
}
