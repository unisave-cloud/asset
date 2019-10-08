using System;
using System.Threading;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;
using Unisave.Editor.Tests.Database.Support.DatabaseProxy;
using Unisave.Exceptions;

namespace Unisave.Editor.Tests.Database
{
    [TestFixture]
    public class TransactionTest : DatabaseTestFixture
    {
        [Test]
        public void ItCreatesEntityInsideTransaction()
        {
            Database.StartTransaction();
            
            var a = new RawEntity { type = "MyEntity" };
            Database.SaveEntity(a);
            
            Database.CommitTransaction();
            
            Assert.NotNull(Database.LoadEntity(a.id));
        }
        
        [Test]
        public void ItRollsBackTransaction()
        {
            // MySQL only test
            if (Mode != TestMode.DatabaseProxy)
            {
                Assert.IsTrue(true);
                return;
            }
            
            Database.StartTransaction();
            
            var a = new RawEntity { type = "MyEntity" };
            Database.SaveEntity(a);
            
            Database.RollbackTransaction();
            
            Assert.IsNull(Database.LoadEntity(a.id));
        }

        [Test]
        public void ItRollsBackNestedTransactionOnly()
        {
            // MySQL only test
            if (Mode != TestMode.DatabaseProxy)
            {
                Assert.IsTrue(true);
                return;
            }

            RawEntity a, b;
            
            Database.StartTransaction();
            
            a = new RawEntity { type = "MyEntity" };
            Database.SaveEntity(a);

            {
                Database.StartTransaction();
                
                b = new RawEntity { type = "MyEntity" };
                Database.SaveEntity(b);
                
                Database.RollbackTransaction();
            }
            
            Database.CommitTransaction();
            
            Assert.IsNotNull(Database.LoadEntity(a.id));
            Assert.IsNull(Database.LoadEntity(b.id));
        }

        [Test]
        public void ItTracksTransactionLevel()
        {
            Assert.AreEqual(0, Database.TransactionLevel());
            Database.StartTransaction();
            Assert.AreEqual(1, Database.TransactionLevel());
            Database.StartTransaction();
            Assert.AreEqual(2, Database.TransactionLevel());
            Database.RollbackTransaction();
            Assert.AreEqual(1, Database.TransactionLevel());
            Database.CommitTransaction();
            Assert.AreEqual(0, Database.TransactionLevel());
            
            Database.CommitTransaction(); // no effect
            Assert.AreEqual(0, Database.TransactionLevel());
            
            Database.RollbackTransaction(); // no effect
            Assert.AreEqual(0, Database.TransactionLevel());
        }

        [Test]
        public void ItThrowsOnDeadlock()
        {
            // MySQL only test
            if (Mode != TestMode.DatabaseProxy)
            {
                Assert.IsTrue(true);
                return;
            }
            
            var deadlockSetup = PrepareForDeadlock();

            Assert.Catch<DatabaseDeadlockException>(() => {
                CreateDeadlock(deadlockSetup);
            });
        }

        [Test]
        public void DeadlockSetsTransactionLevelToZero()
        {
            // MySQL only test
            if (Mode != TestMode.DatabaseProxy)
            {
                Assert.IsTrue(true);
                return;
            }

            var deadlockSetup = PrepareForDeadlock();
            
            Database.StartTransaction();
            Database.StartTransaction();
            
            Assert.AreEqual(2, Database.TransactionLevel());
            
            Assert.Catch<DatabaseDeadlockException>(() => {
                CreateDeadlock(deadlockSetup);
            });
            
            Assert.AreEqual(0, Database.TransactionLevel());
        }

        private Tuple<string, string> PrepareForDeadlock()
        {
            // these entities have to be inserted into the database
            // for sure. Not in a transaction that may be rolled back
            // in the future. That would cause an actual deadlock while
            // trying to emulate a deadlock.
            Assert.AreEqual(0, Database.TransactionLevel());
            
            var entityA = new RawEntity { type = "A" };
            Database.SaveEntity(entityA);
            
            var entityB = new RawEntity { type = "B" };
            Database.SaveEntity(entityB);
            
            return new Tuple<string, string>(entityA.id, entityB.id);
        }

        private void CreateDeadlock(Tuple<string, string> ids)
        {
            // setup another proxy connection (connection B)
            using (var proxyConnection = new DatabaseProxyConnection())
            {
                proxyConnection.Open(
                    mySqlTestFixture.ExecutionId,
                    Config.DatabaseProxyIp,
                    Config.DatabaseProxyPort
                );
                
                // both connections start a transaction
                Database.StartTransaction();
                proxyConnection.StartTransaction();
                
                // connection A locks entity A
                Database.LoadEntity(ids.Item1, "for_update");
                
                // connection B locks entity B
                proxyConnection.LoadEntity(ids.Item2, "for_update");
                
                // connection B attempts to lock entity A and starts waiting
                proxyConnection.GetUnderlyingConnection().GetParrot()
                    .SendTextMessage(
                        203,
                        new JsonObject()
                            .Add("entity_id", ids.Item1)
                            .Add("lock_type", "for_update")
                            .ToString()
                    );
                // do not wait for the response, we would wait forever
                
                // but do wait for a while to make sure the response from
                // client B arrives before the following request by client A
                Thread.Sleep(200);
                
                // finally connection A attempts to lock entity B
                // which causes a deadlock
                try
                {
                    Database.LoadEntity(ids.Item2, "for_update");
                }
                finally
                {
                    // but after the deadlock we can finish the other request
                    // just to keep the connection proper
                    proxyConnection.GetUnderlyingConnection().GetParrot()
                        .ReceiveMessageType(204);
                    
                    // (otherwise DB proxy tries to send response but it logs
                    // an exception that the connection has ended already)
                }
            }
        }
    }
}