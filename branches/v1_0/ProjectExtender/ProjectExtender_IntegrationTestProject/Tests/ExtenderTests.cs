using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.IntegrationTestLibrary;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using FSharp.ProjectExtender;
using System.Threading;

namespace IntegrationTests
{


    [TestClass]
    public class ExtenderTests
    {
        #region fields
        private delegate void ThreadInvoker();
        /// <summary>
        /// TextContext Instances: 
        /// TestContexts are different for each test method, with no sharing between test methods

        /// </summary>
        private static TestContext testContext;
        #endregion

        #region ctors
        public ExtenderTests()
        {
        }
        #endregion

        #region Additional test attributes
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void ExtenderInitialize(TestContext ctx)
        {
            testContext = ctx;
            string path = ctx.TestDir.Substring(0, ctx.TestDir.IndexOf("TestResults"));
            testContext.Properties.Add("slnfile", path + "TestProject\\TestProject.sln");
            testContext.Properties.Add("suo", path + "TestProject\\TestProject.suo");
            testContext.Properties.Add("cleanproj", path + "TestProject\\TestProject\\TestProject.fsproj.clean");
            testContext.Properties.Add("testproj", path + "TestProject\\TestProject\\TestProject.fsproj");

        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void ExtenderCleanup()
        {
            testContext.Properties.Clear();
        }

        // Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        /*public void ControlInitialize() 
        {
              File.Copy(testContext.Properties["projfile"].ToString(), testContext.Properties["testfile"].ToString(), true);
              IVsHierarchy hier;
              IVsSolution sln = VsIdeTestHostContext.ServiceProvider.GetService(typeof(IVsSolution)) as IVsSolution;
              sln.OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, testContext.Properties["slnfile"].ToString());
              sln.GetProjectOfUniqueName(testContext.Properties["testfile"].ToString(), out hier);
              Assert.IsNotNull(hier,"Project is not IProjectManager");
              CompileOrderViewer viewer = new CompileOrderViewer((IProjectManager)hier);
              Assert.IsNotNull(viewer, "Fail to create Viewer");
              testContext.Properties["viewer"] = viewer;
              testContext.Properties["solution"] = sln;
              testContext.Properties["hierarchy"] = hier;
        }*/

        // Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        /*public void ControlCleanup() 
        {
              ((IProjectManager)testContext.Properties["hierarchy"]).BuildManager.FixupProject();
              ((CompileOrderViewer)testContext.Properties["viewer"]).Dispose();
              ((IVsSolution)testContext.Properties["solution"]).CloseSolutionElement
                  ((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave, null, 0);


        }*/

        #endregion

        [TestMethod]
        [HostType("VS IDE")]
        public void InsideSameFolder()
        {
            //"Program3.fs"
            //"Folder1\\File3.fs"
            //"Folder1\\File4.fs"
            //"Folder1\\Sub1\File2.fs"
            //"Folder1\\Sub1\\SubSub1\\File1.fs"
            //"Folder1\\Sub2\\File1.fs"
            //"Folder1\\File1.fs"
            //"Folder2\\File1.fs"
            //"Program.fs"
            //"Program2.fs"
            //"Folder3\\CompileFile1.fs",
            //"Folder3\\ContentFile1.fs",
            //"Folder3\\NoneFile1.fs"

            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                //"It'd be more optimal if root level files won't be swapped in this tests.
                //But they are swapped (manual check,not yet automized)
                new UseCase("InsideSameFolder", testContext)
                    .Apply(new Action(1, Move.Down))
                    .Apply(new Action(3, Move.Down))
                    .ExpectedOrder(
                        "Program3.fs",
                        "Folder1\\File4.fs",
                        "Folder1\\File3.fs",
                        "Folder1\\Sub1\\SubSub1\\File1.fs",
                        "Folder1\\Sub1\\File2.fs",
                        "Folder1\\Sub2\\File1.fs",
                        "Folder1\\File1.fs",
                        "Folder2\\File1.fs",
                        "Program.fs",
                        "Program2.fs",
                        "Folder3\\CompileFile1.fs"//,
                        //"Folder3\\ContentFile1.fs",
                        //"Folder3\\NoneFile1.fs"
                        )
                    .Run();
            });
        }


        [TestMethod]
        [HostType("VS IDE")]
        [Description("This test will fail if string path = '\\' + Path.GetDirectoryName(item.Path) in FixUp")]
        public void BetweenDiffFolders1()
        {
            //"Program3.fs"
            //"Folder1\\File3.fs"
            //"Folder1\\File4.fs"
            //"Folder1\\Sub1\File2.fs"
            //"Folder1\\Sub1\\SubSub1\\File1.fs"
            //"Folder1\\Sub2\\File1.fs"
            //"Folder1\\File1.fs"
            //"Folder2\\File1.fs"
            //"Program.fs"
            //"Program2.fs"
            //"Folder3\\CompileFile1.fs",
            //"Folder3\\ContentFile1.fs",
            //"Folder3\\NoneFile1.fs"


            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                new UseCase("BetweenDiffFolders", testContext)
                    .Apply(new Action(3, Move.Up))
                    .Apply(new Action(2, Move.Up))
                    .Apply(new Action(1, Move.Up))
                    .ExpectedOrder(
                        "Folder1\\Sub1\\File2.fs",
                        "Program3.fs",
                        "Folder1\\File3.fs",
                        "Folder1\\File4.fs",
                        "Folder1\\Sub1\\SubSub1\\File1.fs",
                        "Folder1\\Sub2\\File1.fs",
                        "Folder1\\File1.fs",
                        "Folder2\\File1.fs",
                        "Program.fs",
                        "Program2.fs",
                        "Folder3\\CompileFile1.fs"//,
                        //"Folder3\\ContentFile1.fs",
                        //"Folder3\\NoneFile1.fs"
                    )
                    .Run();
            });


        }

        [TestMethod]
        [HostType("VS IDE")]
        public void BetweenDiffFolders2()
        {
            //"Program3.fs"
            //"Folder1\\File3.fs"
            //"Folder1\\File4.fs"
            //"Folder1\\Sub1\File2.fs"
            //"Folder1\\Sub1\\SubSub1\\File1.fs"
            //"Folder1\\Sub2\\File1.fs"
            //"Folder1\\File1.fs"
            //"Folder2\\File1.fs"
            //"Program.fs"
            //"Program2.fs"
            //"Folder3\\CompileFile1.fs",
            //"Folder3\\ContentFile1.fs",
            //"Folder3\\NoneFile1.fs"
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                new UseCase("BetweenDiffFolders2", testContext)
                    .Apply(new Action(2, Move.Down))
                    .Apply(new Action(7, Move.Up))
                    .Apply(new Action(6, Move.Up))
                    .Apply(new Action(5, Move.Up))
                    .ExpectedOrder(
                           "Program3.fs",
                           "Folder1\\File3.fs",
                           "Folder1\\Sub1\\File2.fs",
                           "Folder1\\File4.fs",
                           "Folder2\\File1.fs",
                           "Folder1\\Sub1\\SubSub1\\File1.fs",
                           "Folder1\\Sub2\\File1.fs",
                           "Folder1\\File1.fs",
                           "Program.fs",
                           "Program2.fs",
                           "Folder3\\CompileFile1.fs"//,
                           //"Folder3\\ContentFile1.fs",
                           //"Folder3\\NoneFile1.fs"
                    )
                    .Run();
            });


        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PullDown()
        {
  
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                new UseCase("PullDown", testContext)
                    .Apply(new Action(4, Move.Down))
                    .Apply(new Action(5, Move.Down))
                    .Apply(new Action(6, Move.Down))
                    .Apply(new Action(7, Move.Down))
                    .Apply(new Action(9, Move.Down))
                    .Apply(new Action(7, Move.Up))
                    .ExpectedOrder(
                            "Program3.fs",
                            "Folder1\\File3.fs",
                            "Folder1\\File4.fs",
                            "Folder1\\Sub1\\File2.fs",
                            "Folder1\\Sub2\\File1.fs",
                            "Folder1\\File1.fs",
                            "Program.fs",
                            "Folder2\\File1.fs",
                            "Folder1\\Sub1\\SubSub1\\File1.fs",
                            "Folder3\\CompileFile1.fs",
                            //"Folder3\\ContentFile1.fs",
                            //"Folder3\\NoneFile1.fs",
                            "Program2.fs"

                        )
                    .Run();
            });


        }

        [TestMethod]
        [Description("The test swaps file stored in the folder where items with 'Content','None' build action exist")]
        [HostType("VS IDE")]
        public void NoneFilesOrder()
        {
            //[0]"Program3.fs",
            //[1]"Folder1\\File3.fs",
            //[2]"Folder1\\File4.fs",
            //[3]"Folder1\\Sub1\File2.fs",
            //[4]"Folder1\\Sub1\\SubSub1\\File1.fs",
            //[5]"Folder1\\Sub2\\File1.fs",
            //[6]"Folder1\\File1.fs",
            //[7]"Folder2\\File1.fs",
            //[8]"Program.fs",
            //[9]"Program2.fs",
            //[10]"Folder3\\CompileFile1.fs",
            //[11]"Folder3\\ContentFile1.fs",
            //[12]"Folder3\\NoneFile1.fs"
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                new UseCase("NoneFilesOrder", testContext)
                    .Apply(new Action(10, Move.Up))
                    .Apply(new Action(9, Move.Up))
                    .Apply(new Action(8, Move.Up))
                    .Apply(new Action(7, Move.Up))
                    .Apply(new Action(6, Move.Up))
                    .ExpectedOrder(
                        "Program3.fs",
                        "Folder1\\File3.fs",
                        "Folder1\\File4.fs",
                        "Folder1\\Sub1\\File2.fs",
                        "Folder1\\Sub1\\SubSub1\\File1.fs",
                        "Folder3\\CompileFile1.fs",
                        "Folder1\\Sub2\\File1.fs",
                        //"Folder3\\ContentFile1.fs",
                        //"Folder3\\NoneFile1.fs"
                        "Folder1\\File1.fs",
                        "Folder2\\File1.fs",
                        "Program.fs",
                        "Program2.fs"//,
                    )
                    .Run();
            });
        }

        [TestMethod]
        [HostType("VS IDE")]
        [Description("Now it fails because items of the root level 'are always joined'" )]
        public void NoChangesMade()
        {
            //[0]"Program3.fs",
            //[1]"Folder1\\File3.fs",
            //[2]"Folder1\\File4.fs",
            //[3]"Folder1\\Sub1\File2.fs",
            //[4]"Folder1\\Sub1\\SubSub1\\File1.fs",
            //[5]"Folder1\\Sub2\\File1.fs",
            //[6]"Folder1\\File1.fs",
            //[7]"Folder2\\File1.fs",
            //[8]"Program.fs",
            //[9]"Program2.fs",
            //[10]"Folder3\\CompileFile1.fs",
            //[11]"Folder3\\ContentFile1.fs",
            //[12]"Folder3\\NoneFile1.fs"
            UIThreadInvoker.Invoke((ThreadInvoker)delegate()
            {
                new UseCase("NoneFilesOrder", testContext)
                    .ExpectedOrder(
                         "Program3.fs",
                         "Folder1\\File3.fs",
                         "Folder1\\File4.fs",
                         "Folder1\\Sub1\\File2.fs",
                         "Folder1\\Sub1\\SubSub1\\File1.fs",
                         "Folder1\\Sub2\\File1.fs",
                         "Folder1\\File1.fs",
                         "Folder2\\File1.fs",
                         "Program.fs",
                         "Program2.fs",
                         "Folder3\\CompileFile1.fs"//,
                         //"Folder3\\ContentFile1.fs",
                         //"Folder3\\NoneFile1.fs"
                         )
                    .Run();


            });
        }
    }
}
