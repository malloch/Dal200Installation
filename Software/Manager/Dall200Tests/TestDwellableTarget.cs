using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Dal200Instalation.Model;
using Dal200Instalation.Model.Dwellable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dall200Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestDwellableTarget
    {
        
        [TestMethod]
        public void TestTrueDwell()
        {
            var tgtPoint = new Point(100, 100);
            var tgt = new DwellableTarget(tgtPoint,"testDwell",0,0);
            var tracked = new Tracked(0,100,101);

            Thread.Sleep(3*1000);
            Assert.IsTrue(tgt.DetectDwell(tracked, 2, TimeSpan.FromSeconds(3)));
        }

        [TestMethod]
        public void TestNotDwell()
        {
            var tgtPoint = new Point(100, 100);
            var tgt = new DwellableTarget(tgtPoint,"notDwell",0,0);
            var tracked = new Tracked(0, 100, 101);
            Thread.Sleep(1 * 1000);
            Assert.IsFalse(tgt.DetectDwell(tracked, 2, TimeSpan.FromSeconds(2)));
        }

        [TestMethod]
        public void TestInOutInDwell()
        {
            var tgtPoint = new Point(100, 100);
            var tgt = new DwellableTarget(tgtPoint,"testDwell",0,0);
            var tracked = new Tracked(0, 100, 101);
            Thread.Sleep(1 * 1000);
            Assert.IsFalse(tgt.DetectDwell(tracked, 2, TimeSpan.FromSeconds(2)));

            tracked = new Tracked(0,100,400);
            Thread.Sleep(3 * 1000);
            Assert.IsFalse(tgt.DetectDwell(tracked, 2, TimeSpan.FromSeconds(2)));

            Thread.Sleep(1 * 1000);
            tracked = new Tracked(0, 100, 101);
            Assert.IsFalse(tgt.DetectDwell(tracked, 2, TimeSpan.FromSeconds(2)));
            Thread.Sleep(2 * 1000);
            Assert.IsTrue(tgt.DetectDwell(tracked, 2, TimeSpan.FromSeconds(2)));


        }
    }
}
