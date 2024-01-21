using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Domain.Entities
{
    public class Book : IEntity<Guid>
    {
       public Guid Id { get; set; }
       public string Title { get; set; }
       public DateTime PublishDate { get; set; }
       public string AuthorName { get; set; }
    }
}
