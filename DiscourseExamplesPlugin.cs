using Rhino;
using System;
using System.Runtime.CompilerServices;
using Rhino.PlugIns;

namespace DiscourseExamples
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class DiscourseExamplesPlugin : Rhino.PlugIns.PlugIn
    {
        public DiscourseExamplesPlugin()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the DiscourseExamplesPlugin plug-in.</summary>
        public static DiscourseExamplesPlugin Instance { get; private set; }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.
        //override LoadTime => PlugInLoadTime.AtStartup;
        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
    }
}
