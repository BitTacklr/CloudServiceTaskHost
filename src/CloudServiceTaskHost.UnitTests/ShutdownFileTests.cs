using System;
using System.IO;
using NUnit.Framework;

namespace CloudServiceTaskHost
{
    [TestFixture]
    public class ShutdownFileTests
    {
        private ShutdownFile _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = ShutdownFile.CreateRandom(new DirectoryInfo(Environment.CurrentDirectory));
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public void CreateRandomDirectoryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => ShutdownFile.CreateRandom(null));
        }

        [Test]
        public void CreateRandomHasExpectedResult()
        {
            using (var sut = ShutdownFile.CreateRandom(new DirectoryInfo(Environment.CurrentDirectory)))
            {
                Assert.That(Path.GetDirectoryName(sut.FullName), Is.EqualTo(Environment.CurrentDirectory));
                Assert.That(Path.GetFileName(sut.FullName), Is.Not.EqualTo(Path.GetFileName(_sut.FullName)));
            }
        }

        [Test]
        public void NotifyCreatesAssociatedFile()
        {
            Assert.That(File.Exists(_sut.FullName), Is.False);
            _sut.Notify();
            Assert.That(File.Exists(_sut.FullName), Is.True);
        }

        [Test]
        public void IsDisposable()
        {
            Assert.That(_sut, Is.InstanceOf<IDisposable>());
        }

        [Test]
        public void DisposeDeletesAssociatedFile()
        {
            File.WriteAllText(_sut.FullName, ".");
            Assert.That(File.Exists(_sut.FullName), Is.True);
            _sut.Dispose();
            Assert.That(File.Exists(_sut.FullName), Is.False);
        }

        [Test]
        public void DisposeDoesNotFailWhenAssociatedFileDoesNotExist()
        {
            Assert.That(File.Exists(_sut.FullName), Is.False);
            Assert.DoesNotThrow(() => _sut.Dispose());
        }
    }
}
