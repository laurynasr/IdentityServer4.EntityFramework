﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.Stores
{
    public class PersistedGrantStoreTests : IntegrationTest<PersistedGrantStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
    {
        public PersistedGrantStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
        {
            foreach (var options in TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<PersistedGrantDbContext>)y)).ToList())
            {
                using (var context = new PersistedGrantDbContext(options, StoreOptions))
                    context.Database.EnsureCreated();
            }
        }

        private static PersistedGrant CreateTestObject()
        {
            return new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                Type = "authorization_code",
                ClientId = Guid.NewGuid().ToString(),
                SubjectId = Guid.NewGuid().ToString(),
                CreationTime = new DateTime(2016, 08, 01),
                Expiration = new DateTime(2016, 08, 31),
                Data = Guid.NewGuid().ToString()
            };
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task StoreAsync_WhenPersistedGrantStored_ExpectSuccess(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.StoreAsync(persistedGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task GetAsync_WithKeyAndPersistedGrantExists_ExpectPersistedGrantReturned(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                await context.SaveChangesAsync();
            }

            PersistedGrant foundPersistedGrant;
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                foundPersistedGrant = await store.GetAsync(persistedGrant.Key);
            }

            Assert.NotNull(foundPersistedGrant);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task GetAsync_WithSubAndTypeAndPersistedGrantExists_ExpectPersistedGrantReturned(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                await context.SaveChangesAsync();
            }

            IEnumerable<PersistedGrant> foundPersistedGrants;
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                foundPersistedGrants = await store.GetAllAsync(persistedGrant.SubjectId);
            }

            Assert.NotNull(foundPersistedGrants);
            Assert.NotEmpty(foundPersistedGrants);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAsync_WhenKeyOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                await context.SaveChangesAsync();
            }
            
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAsync(persistedGrant.Key);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAsync_WhenSubIdAndClientIdOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                await context.SaveChangesAsync();
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAllAsync(persistedGrant.SubjectId, persistedGrant.ClientId);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task RemoveAsync_WhenSubIdClientIdAndTypeOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                await context.SaveChangesAsync();
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.RemoveAllAsync(persistedGrant.SubjectId, persistedGrant.ClientId, persistedGrant.Type);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task Store_should_create_new_record_if_key_does_not_exist(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.Null(foundGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                await store.StoreAsync(persistedGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public async Task Store_should_update_record_if_key_already_exists(DbContextOptions<PersistedGrantDbContext> options)
        {
            var persistedGrant = CreateTestObject();

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                context.PersistedGrants.Add(persistedGrant.ToEntity());
                await context.SaveChangesAsync();
            }

            var newDate = persistedGrant.Expiration.Value.AddHours(1);
            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
                persistedGrant.Expiration = newDate;
                await store.StoreAsync(persistedGrant);
            }

            using (var context = new PersistedGrantDbContext(options, StoreOptions))
            {
                var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
                Assert.NotNull(foundGrant);
                Assert.Equal(newDate, persistedGrant.Expiration);
            }
        }
    }
}