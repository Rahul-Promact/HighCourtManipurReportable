using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCourtManipurReportable
{
    public class AppDbContext:DbContext
    {
        public DbSet<caseDetail> CaseDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Database=ManipurCourtTest;User Id=postgres;Password=root;");
        }
    }
}
