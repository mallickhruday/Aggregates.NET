﻿using Aggregates.Contracts;
using NEventStore;
using NServiceBus;
using NServiceBus.ObjectBuilder;
using NServiceBus.ObjectBuilder.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregates.Unit.UnitOfWork
{
    [TestFixture]
    public class CommitTests
    {
        private Moq.Mock<IBuilder> _builder;
        private Moq.Mock<IStoreEvents> _eventStore;
        private Moq.Mock<IBus> _bus;
        private Moq.Mock<IRepository<IEventSource<Guid>>> _guidRepository;
        private Moq.Mock<IRepository<IEventSource<Int32>>> _intRepository;
        private Moq.Mock<IRepositoryFactory> _repoFactory;
        private IUnitOfWork _uow;

        [SetUp]
        public void Setup()
        {
            _builder = new Moq.Mock<IBuilder>();
            _eventStore = new Moq.Mock<IStoreEvents>();
            _repoFactory = new Moq.Mock<IRepositoryFactory>();
            _bus = new Moq.Mock<IBus>();
            _guidRepository = new Moq.Mock<IRepository<IEventSource<Guid>>>();
            _intRepository = new Moq.Mock<IRepository<IEventSource<Int32>>>();
            _guidRepository.Setup(x => x.Commit(Moq.It.IsAny<Guid>(), Moq.It.IsAny<IDictionary<String, String>>())).Verifiable();
            _intRepository.Setup(x => x.Commit(Moq.It.IsAny<Guid>(), Moq.It.IsAny<IDictionary<String, String>>())).Verifiable();

            _repoFactory.Setup(x => x.Create<IEventSource<Guid>>(Moq.It.IsAny<IBuilder>(), Moq.It.IsAny<IStoreEvents>())).Returns(_guidRepository.Object);
            _repoFactory.Setup(x => x.Create<IEventSource<Int32>>(Moq.It.IsAny<IBuilder>(), Moq.It.IsAny<IStoreEvents>())).Returns(_intRepository.Object);

            _builder.Setup(x => x.CreateChildBuilder()).Returns(_builder.Object);
            _uow = new Aggregates.Internal.UnitOfWork(_builder.Object, _eventStore.Object, _repoFactory.Object);
        }

        [Test]
        public void Commit_no_events()
        {
            Assert.DoesNotThrow(() => _uow.Commit());
        }

        [Test]
        public void Commit_one_repo()
        {
            var repo = _uow.For<IEventSource<Guid>>();
            Assert.DoesNotThrow(() => _uow.Commit());
            _guidRepository.Verify(x => x.Commit(Moq.It.IsAny<Guid>(), Moq.It.IsAny<IDictionary<String, String>>()), Moq.Times.Once);
        }

        [Test]
        public void Commit_multiple_repo()
        {
            var repo = _uow.For<IEventSource<Guid>>();
            var repo2 = _uow.For<IEventSource<Int32>>();
            Assert.DoesNotThrow(() => _uow.Commit());
            _guidRepository.Verify(x => x.Commit(Moq.It.IsAny<Guid>(), Moq.It.IsAny<IDictionary<String, String>>()), Moq.Times.Once);
            _intRepository.Verify(x => x.Commit(Moq.It.IsAny<Guid>(), Moq.It.IsAny<IDictionary<String, String>>()), Moq.Times.Once);
        }

        [Test]
        public void end_calls_commit()
        {
            var repo = _uow.For<IEventSource<Guid>>();
            _uow.End();
            _guidRepository.Verify(x => x.Commit(Moq.It.IsAny<Guid>(), Moq.It.IsAny<IDictionary<String, String>>()), Moq.Times.Once);
        }

    }
}