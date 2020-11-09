﻿using System;
using System.Diagnostics;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Diagnostics;
using HashCode = Jerrycurl.Diagnostics.HashCode;

namespace Jerrycurl.Data.Queries.Internal.Caching
{
    [DebuggerDisplay("{GetType().Name,nq}: {Name}")]
    internal class DataAttribute
    {
        public string Name { get; }

        public DataAttribute(string name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
