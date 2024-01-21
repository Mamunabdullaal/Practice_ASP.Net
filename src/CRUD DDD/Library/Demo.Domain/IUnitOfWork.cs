using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Domain
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        void Save();
        Task SaveAsync();
    }
}
