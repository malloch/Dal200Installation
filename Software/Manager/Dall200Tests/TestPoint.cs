using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dal200Instalation.Model;

namespace Dall200Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestPoint
    {
        [TestMethod]
        public void TestInside()
        {
            var p1 = new Point(10, 20);
            var p2 = new Point(10,21);

            Assert.IsTrue(Point.IsInsideRaidus(p1,p2,1));
        }

        [TestMethod]
        public void TestOutside()
        {
            var p1 = new Point(10,20);
            var p2 = new Point(40,50);

            Assert.IsFalse(Point.IsInsideRaidus(p1,p2,1));
        }
    }
}
