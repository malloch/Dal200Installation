using System;
using Dal200Instalation.Model;
using Dal200Instalation.Model.Dwellable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dall200Tests
{
    [TestClass]
    public class TestTargetEntriesIO
    {
        private DwellableCollection savedCollection;
        private DwellableCollection loadedCollection;

        [TestMethod]
        public void TestCreate()
        {
            savedCollection = new DwellableCollection(3, TimeSpan.Zero);
            for (int i = 0; i < 5; i++)
            {
                var tgt = new Dal200Instalation.Model.Dwellable.DwellableTarget( new Point(i * 7, i * 13));
                savedCollection.AddTarget(tgt);
            }

            savedCollection.SaveTargetsToFile("testTargets");
        }
        [TestMethod]
        public void TestLoad()
        {
            loadedCollection = new DwellableCollection(3, TimeSpan.Zero);
            loadedCollection.LoadTargetsFromFile("testTargets");
        }
        [TestMethod]
        public void TestCompare()
        {
            Assert.AreEqual(savedCollection, loadedCollection);
        }
    }
}
