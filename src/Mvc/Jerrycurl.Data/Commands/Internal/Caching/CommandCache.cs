﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Jerrycurl.Data.Commands.Internal.Compilation;
using Jerrycurl.Data.Metadata;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Commands.Internal.Caching
{
    internal static class CommandCache
    {
        private static readonly ConcurrentDictionary<CommandCacheKey, BufferWriter> writerMap = new ConcurrentDictionary<CommandCacheKey, BufferWriter>();
        private static readonly ConcurrentDictionary<CommandCacheKey, BufferConverter> converterMap = new ConcurrentDictionary<CommandCacheKey, BufferConverter>();

        public static BufferWriter GetWriter(IReadOnlyList<ColumnName> columnNames)
        {
            CommandCacheKey cacheKey = new CommandCacheKey(columnNames);

            return writerMap.GetOrAdd(cacheKey, k =>
            {
                CommandCompiler compiler = new CommandCompiler();

                return compiler.Compile(k.Columns);
            });
        }

        public static BufferConverter GetConverter(MetadataIdentity metadata, ColumnMetadata columnMetadata)
        {
            CommandCacheKey cacheKey = new CommandCacheKey(metadata, columnMetadata);

            return converterMap.GetOrAdd(cacheKey, k =>
            {
                CommandCompiler compiler = new CommandCompiler();

                return compiler.Compile(metadata, columnMetadata);
            });
        }
    }
}
