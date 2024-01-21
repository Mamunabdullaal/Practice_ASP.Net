using Demo.Domain.Entities;
using Demo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Infrastructure.Repositories
{
    public class BookRepository : Repository<Book, Guid>, IBookRepostitory
    {
        public BookRepository(DbContext context) : base(context)
        {
        }
    }
}
