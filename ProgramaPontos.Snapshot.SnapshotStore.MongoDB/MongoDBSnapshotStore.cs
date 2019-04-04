﻿using MongoDB.Driver;
using ProgramaPontos.Domain.Core.Aggregates;
using ProgramaPontos.Domain.Core.Snapshot;
using System;

namespace ProgramaPontos.Snapshot.SnapshotStore.MongoDB
{
    public class MongoDBSnapshotStore : ISnapshotStore
    {
        private IMongoCollection<SnapshotItem> collection;


        public MongoDBSnapshotStore(MongoDBSnapshotStoreSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            collection = database.GetCollection<SnapshotItem>(nameof(SnapshotItem));
        }

        public ISnapshot<T> GetSnapshotFromAggreate<T>(Guid aggregateId) where T : IAggregateRoot
        {
            var result = collection.Find(f => f.AggregateId == aggregateId.ToString()).FirstOrDefault();
            if (result == null) return null;
            return SnapshotItem.ToSnapshot<T>(result);
        }

        public void SaveSnapshot<T>(T aggregateRoot) where T : IAggregateRoot
        {
            var snapshot = new Snapshot<T>(aggregateRoot);
            var snapshotItem = SnapshotItem.FromDomainSnapshot(snapshot);
            collection.ReplaceOne(f => f.AggregateId == aggregateRoot.Id.ToString(), snapshotItem, new UpdateOptions() { IsUpsert = true });

        }
    }
}