using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace DiscourseExamples
{
    public class CrvDeviationBug : Command
    {
        public CrvDeviationBug()
        {
            Instance = this;
        }

        public static CrvDeviationBug Instance { get; private set; }

        public override string EnglishName => "CrvDeviationBug";


        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var crv0RO = doc.Objects.FindId(new Guid("f8d4bfbb-c324-42b9-b9e6-d362bf066726"));
            var crv1RO = doc.Objects.FindId(new Guid("667a871b-362f-4542-abb1-fe5c99b233c0"));
            if (crv0RO == null || crv1RO == null)
            {
                RhinoApp.WriteLine("Couldn't find the example crvs by their GUID.");
                return Result.Failure;
            }
           
            if (crv0RO.Geometry is NurbsCurve crv0 && crv1RO.Geometry is NurbsCurve crv1)
            {
                var min = 0.0;
                var minT0 = 0.0;
                var minT1 = 0.0;
                var max = 0.0;
                var maxT0 = 0.0;
                var maxT1 = 0.0;
                if (Curve.GetDistancesBetweenCurves(crv0, crv1, 0.001,
                        out max, out maxT0, out maxT1,
                        out min, out minT0, out minT1))
                {
                    var d0 = crv0.Domain.T1 - crv0.Domain.T0;
                    var d1 = crv1.Domain.T1 - crv0.Domain.T0;
                    
                    var t0 = crv0.Domain.NormalizedParameterAt(maxT0);
                    var t1 = crv1.Domain.NormalizedParameterAt(maxT1);
                    var pt0 = crv0.PointAtNormalizedLength(t0);
                    var pt1 = crv1.PointAtNormalizedLength(t1);
                    
                    RhinoApp.WriteLine("Max: {0:N3}, MaxT0: {1:N3}, MaxT1: {2:N3}.", max, maxT0, maxT1);
                    RhinoApp.WriteLine("Domain - Crv0: {0:N3}, Crv0: {1:N3}", d0, d1);
                    RhinoApp.WriteLine("Normalized t - Crv0: {0:N3}, Crv1: {1:N3}", t0, t1);

                    var line = new Line(pt0, pt1);
                    doc.Objects.AddLine(line);
                    RhinoApp.WriteLine("Distance from Pts at Normalized t's: {0:N3}", line.Length);
                    doc.Views.Redraw();
                    
                }
                
                return Result.Success;
            }
            RhinoApp.WriteLine("Couldn't find the example crvs by their GUID.");
            return Result.Failure;
        }

       
    }
}
