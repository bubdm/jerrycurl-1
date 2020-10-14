﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Jerrycurl.Data.Commands;
using Jerrycurl.Data.Queries;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Data.V11;
using Jerrycurl.Relations;
using Jerrycurl.Relations.Language;

namespace Jerrycurl.Data.V11.Language
{
    public static class BufferExtensions
    {
        public static void Insert(this IQueryBuffer buffer, IRelation2 relation, params string[] insertHeader)
            => buffer.Insert(relation, (IEnumerable<string>)insertHeader);

        public static void Insert(this IQueryBuffer buffer, IRelation2 relation, IEnumerable<string> insertHeader)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            using IDataReader dataReader = relation.GetDataReader(insertHeader);

            buffer.Insert(dataReader);
        }

        public static void Insert(this IQueryBuffer buffer, IRelation2 relation)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (relation == null)
                throw new ArgumentNullException(nameof(buffer));

            using IDataReader dataReader = relation.GetDataReader();

            buffer.Insert(dataReader);
        }

        public static void Insert<TSource>(this IQueryBuffer buffer, IEnumerable<TSource> data, params string[] selectHeader)
            => buffer.Insert(data, (IEnumerable<string>)selectHeader);

        public static void Insert<TSource>(this IQueryBuffer buffer, IEnumerable<TSource> data, IEnumerable<string> selectHeader)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            IRelation2 relation = buffer.Store.From(data).Select(selectHeader);

            buffer.Insert(relation);
        }

        public static void Insert<TSource>(this IQueryBuffer buffer, IEnumerable<TSource> data, params (string Select, string Insert)[] mappingHeader)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            IEnumerable<string> selectHeader = mappingHeader.Select(t => t.Select);
            IEnumerable<string> insertHeader = mappingHeader.Select(t => t.Insert);

            buffer.Insert(data, selectHeader, insertHeader);
        }

        public static void Insert<TSource>(this IQueryBuffer buffer, IEnumerable<TSource> data, IEnumerable<string> selectHeader, IEnumerable<string> insertHeader)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            IRelation2 relation = buffer.Store.From(data).Select(selectHeader);

            buffer.Insert(relation, insertHeader);
        }
    }
}
