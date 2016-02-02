﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using net.r_eg.vsSBE.Events;
using net.r_eg.vsSBE.Exceptions;
using net.r_eg.vsSBE.SBEScripts;
using net.r_eg.vsSBE.SBEScripts.Components;
using net.r_eg.vsSBE.SBEScripts.Exceptions;

namespace net.r_eg.vsSBE.Test.SBEScripts.Components
{
    /// <summary>
    ///This is a test class for InternalComponentTest and is intended
    ///to contain all InternalComponentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InternalComponentTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        ///A test for parse
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(SyntaxIncorrectException))]
        public void parseTest()
        {
            InternalComponent target = new InternalComponent();
            target.parse("#[vsSBE events.Type.item(1)]");
        }

        /// <summary>
        ///A test for parse
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(SyntaxIncorrectException))]
        public void parseTest2()
        {
            InternalComponent target = new InternalComponent();
            target.parse("vsSBE events.Type.item(1)");
        }

        /// <summary>
        ///A test for parse
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(SubtypeNotFoundException))]
        public void parseTest3()
        {
            InternalComponent target = new InternalComponent();
            target.parse("[vsSBE NoExist.Type]");
        }

        /// <summary>
        ///A test for parse - stEvents
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(OperandNotFoundException))]
        public void stEventsParseTest1()
        {
            InternalComponent target = new InternalComponent();
            target.parse("[vsSBE events.Type.item(name)]");
        }

        /// <summary>
        ///A test for parse - stEvents
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(OperandNotFoundException))]
        public void stEventsParseTest2()
        {
            InternalComponent target = new InternalComponent();
            target.parse("[vsSBE events.Type.item(1).test]");
        }

        /// <summary>
        ///A test for parse - stEventItem - pEnabled
        ///</summary>
        [TestMethod()]
        public void pEnabledParseTest1()
        {
            InternalComponentAccessor target = new InternalComponentAccessor();
            Assert.AreEqual(Value.from(true), target.parse("[vsSBE events.Pre.item(1).Enabled]"));
            Assert.AreEqual(Value.from(true), target.parse("[vsSBE events.Pre.item(\"Name1\").Enabled]"));
            Assert.AreEqual(Value.from(false), target.parse("[vsSBE events.Pre.item(2).Enabled]"));
            Assert.AreEqual(Value.from(false), target.parse("[vsSBE events.Pre.item(\"Name2\").Enabled]"));
        }

        /// <summary>
        ///A test for parse - stEventItem - pEnabled
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IncorrectSyntaxException))]
        public void pEnabledParseTest2()
        {
            InternalComponentAccessor target = new InternalComponentAccessor();
            target.parse("[vsSBE events.Pre.item(1).Enabled = 1true]");
        }

        /// <summary>
        ///A test for parse - stEventItem - pEnabled
        ///</summary>
        [TestMethod()]
        public void pEnabledParseTest3()
        {
            InternalComponentAccessor target = new InternalComponentAccessor();
            Assert.AreEqual(Value.from(true), target.parse("[vsSBE events.Pre.item(1).Enabled]"));
            Assert.AreEqual(Value.Empty, target.parse("[vsSBE events.Pre.item(1).Enabled = false]"));
            Assert.AreEqual(Value.from(false), target.parse("[vsSBE events.Pre.item(1).Enabled]"));

            Assert.AreEqual(Value.from(false), target.parse("[vsSBE events.Pre.item(\"Name2\").Enabled]"));
            Assert.AreEqual(Value.Empty, target.parse("[vsSBE events.Pre.item(\"Name2\").Enabled = true]"));
            Assert.AreEqual(Value.from(true), target.parse("[vsSBE events.Pre.item(\"Name2\").Enabled]"));
        }

        /// <summary>
        ///A test for parse - stEventItem - pStatus
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IncorrectNodeException))]
        public void pStatusParseTest1()
        {
            InternalComponentAccessor target = new InternalComponentAccessor();
            target.parse("[vsSBE events.Pre.item(1).Status.Has Errors]");
        }

        /// <summary>
        ///A test for parse - stEventItem - pStatus
        ///</summary>
        [TestMethod()]
        public void pStatusParseTest2()
        {
            InternalComponentAccessor target = new InternalComponentAccessor();
            Assert.AreEqual("false", target.parse("[vsSBE events.Pre.item(1).Status.HasErrors]"));
        }

        /// <summary>
        ///A test for parse - stEventItem - pStatus
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IncorrectNodeException))]
        public void pStatusParseTest3()
        {
            InternalComponentAccessor target = new InternalComponentAccessor();
            target.parse("[vsSBE events.Pre.item(1).Status.NotExistProp]");
        }

        /// <summary>
        ///A test for parse - stEventItem - pStdout
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IncorrectNodeException))]
        public void pStdoutTest1()
        {
            var target = new InternalComponentAccessor();
            target.parse("[Core events.Pre.item(1).stdout = true]");
        }

        /// <summary>
        ///A test for parse - stEventItem - pStdout
        ///</summary>
        [TestMethod()]
        public void pStdoutTest2()
        {
            var target = new InternalComponentAccessor();
            Assert.AreNotEqual(null, target.parse("[Core events.Pre.item(1).stdout]"));
        }

        /// <summary>
        ///A test for parse - stEventItem - pStderr
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(IncorrectNodeException))]
        public void pStderrTest1()
        {
            var target = new InternalComponentAccessor();
            target.parse("[Core events.Pre.item(1).stderr = true]");
        }

        /// <summary>
        ///A test for parse - stEventItem - pStderr
        ///</summary>
        [TestMethod()]
        public void pStderrTest2()
        {
            var target = new InternalComponentAccessor();
            Assert.AreNotEqual(null, target.parse("[Core events.Pre.item(1).stderr]"));
        }

        private class InternalComponentAccessor: InternalComponent
        {
            private SBEEvent[] evt = null;

            protected override ISolutionEvent[] getEvent(SolutionEventType type)
            {
                if(evt != null) {
                    return evt;
                }

                evt = new SBEEvent[2]{ 
                    new SBEEvent(){
                        Name = "Name1", Enabled = true
                    },
                    new SBEEvent(){
                        Name = "Name2", Enabled = false
                    }
                };
                return evt;
            }
        }
    }
}
