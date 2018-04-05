using System;
using Dal200Instalation.Model;
using Dal200Instalation.Model.Dwellable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dall200Tests
{
    [TestClass]
    public class TestTargetEntriesIO
    {
        
        private DwellableCollection loadedCollection;
        
        [TestMethod]
        public void TestLoad()
        {
            loadedCollection = new DwellableCollection(3, TimeSpan.Zero);
            loadedCollection.LoadTargetsFromFile("testTargets");
            Assert.AreNotEqual(loadedCollection.dwellableTargets.Count,0);
        }
    }
}
