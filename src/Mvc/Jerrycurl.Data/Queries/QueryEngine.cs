﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Jerrycurl.Collections;
using System.Data.Common;
using System.Threading;
using System.Runtime.CompilerServices;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Data.Buf2;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries
{
    public class QueryEngine
    {
        public QueryOptions Options { get; }

        public QueryEngine(QueryOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #region " Aggregate "

        public T Aggregate<T>(Query query) => this.Aggregate<T>(new[] { query });
        public T Aggregate<T>(IEnumerable<Query> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema store found.");

            QueryBuffer buffer = new QueryBuffer(this.Options.Schemas.GetSchema(typeof(List<T>)), QueryType2.Aggregate);
            //AggregateBuffer<T> buffer = new AggregateBuffer<T>(this.Options.Schemas);

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (IBatch batch in this.FilterBatches(queries))
            {
                foreach (IDataReader dataReader in connection.Execute(batch))
                    buffer.Insert(dataReader);
            }

            return (T)buffer.Commit();
        }

        public Task<T> AggregateAsync<T>(Query query, CancellationToken cancellationToken = default) => this.AggregateAsync<T>(new[] { query }, cancellationToken);

        public async Task<T> AggregateAsync<T>(IEnumerable<Query> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            QueryBuffer buffer = new QueryBuffer(this.Options.Schemas.GetSchema(typeof(List<T>)), QueryType2.Aggregate);
            //AggregateBuffer<T> buffer = new AggregateBuffer<T>(this.Options.Schemas);

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (IBatch batch in this.FilterBatches(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(batch, cancellationToken).ConfigureAwait(false))
                    await buffer.InsertAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return (T)buffer.Commit();
        }

        #endregion

        #region " List "

        public IList<T> List<T>(Query query) => this.List<T>(new[] { query });
        public IList<T> List<T>(IEnumerable<Query> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema store found.");

            QueryBuffer buffer = new QueryBuffer(this.Options.Schemas.GetSchema(typeof(List<T>)), QueryType2.List);
            //ListBuffer<TItem> buffer = new ListBuffer<TItem>(this.Options.Schemas);

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (IBatch batch in this.FilterBatches(queries))
            { 
                foreach (IDataReader dataReader in connection.Execute(batch))
                    buffer.Insert(dataReader);
            }

            return (List<T>)buffer.Commit();
        }

        public Task<IList<T>> ListAsync<T>(Query query, CancellationToken cancellationToken = default) => this.ListAsync<T>(new[] { query }, cancellationToken);

        public async Task<IList<T>> ListAsync<T>(IEnumerable<Query> queries, CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            QueryBuffer buffer = new QueryBuffer(this.Options.Schemas.GetSchema(typeof(List<T>)), QueryType2.List);
            //ListBuffer<TItem> buffer = new ListBuffer<TItem>(this.Options.Schemas);

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (IBatch batch in this.FilterBatches(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(batch, cancellationToken).ConfigureAwait(false))
                    await buffer.InsertAsync(dataReader, cancellationToken).ConfigureAwait(false);
            }

            return (List<T>)buffer.Commit();
        }

        #endregion

        #region " Enumerate "

        public IAsyncEnumerable<QueryReader> EnumerateAsync(Query query, CancellationToken cancellationToken = default) => this.EnumerateAsync(query, cancellationToken);
        public async IAsyncEnumerable<QueryReader> EnumerateAsync(IEnumerable<Query> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            await using IAsyncSession connection = this.Options.GetAsyncSession();

            foreach (IBatch batch in this.FilterBatches(queries))
            {
                await foreach (DbDataReader dataReader in connection.ExecuteAsync(batch, cancellationToken).ConfigureAwait(false))
                    yield return new QueryReader(this.Options.Schemas, dataReader);
            }
        }

        public IAsyncEnumerable<TItem> EnumerateAsync<TItem>(Query query, CancellationToken cancellationToken = default) => this.EnumerateAsync<TItem>(new[] { query }, cancellationToken);
        public async IAsyncEnumerable<TItem> EnumerateAsync<TItem>(IEnumerable<Query> queries, [EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            await foreach (QueryReader queryReader in this.EnumerateAsync(queries, cancellationToken).ConfigureAwait(false))
            {
                await foreach (TItem item in queryReader.ReadAsync<TItem>(cancellationToken).ConfigureAwait(false))
                    yield return item;
            }
        }

        public IEnumerable<TItem> Enumerate<TItem>(Query query) => this.Enumerate<TItem>(new[] { query });
        public IEnumerable<TItem> Enumerate<TItem>(IEnumerable<Query> queries) => this.Enumerate(queries).SelectMany(r => r.Read<TItem>());

        public IEnumerable<QueryReader> Enumerate(Query query) => this.Enumerate(new[] { query });
        public IEnumerable<QueryReader> Enumerate(IEnumerable<Query> queries)
        {
            if (queries == null)
                throw new ArgumentNullException(nameof(queries));

            if (this.Options.Schemas == null)
                throw new InvalidOperationException("No schema builder found.");

            using ISyncSession connection = this.Options.GetSyncSession();

            foreach (IBatch batch in this.FilterBatches(queries))
            {
                foreach (IDataReader reader in connection.Execute(batch))
                    yield return new QueryReader(this.Options.Schemas, reader);
            }
        }

        #endregion

        private IEnumerable<IBatch> FilterBatches(IEnumerable<Query> queries)
            => queries.NotNull().Where(d => !string.IsNullOrWhiteSpace(d.QueryText));
    }
}
