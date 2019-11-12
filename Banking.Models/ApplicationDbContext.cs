using Microsoft.EntityFrameworkCore;
using Banking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace RevatureProj1.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserBanking>
    {
        //public readonly IHttpContextAccessor _httpContextAccessor;


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            

        }

        public DbSet<CheckingAccount> CheckingAccounts { get; set; }
        public DbSet<Transaction> AccountTransactions { get; set; }
        public DbSet<BusinessAccount> BusinessAccounts { get; set; }
        public DbSet<LoanAccount> LoanAccounts { get; set; }
        public DbSet<CDAccounts> TermAccounts { get; set; }

    }
}
