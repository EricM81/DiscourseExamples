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
            var go = new GetObject { GeometryFilter = ObjectType.Curve };
            go.SetCommandPrompt("Select curves to test");
            go.GetMultiple(2, 2);
            if (go.CommandResult() != Result.Success)
                return go.CommandResult();
            
            var curveA = go.Object(0).Curve();
            var curveB = go.Object(1).Curve();
            if (null == curveA || null == curveB)
                return Result.Failure;

            if (Curve.GetDistancesBetweenCurves(curveA, curveB, 0.001,
                    out var max, out var maxAt, out var maxBt,
                    out var min, out _, out _))
            {
                // This is what I thought was bugged.  Converting it to the domain is not as simple as 
                // using maxAt / (T1 - T0).  The knot vector gives the domain a non linear distribution.  
                // This applies to both uniform and non-uniform crvs with the exception of single-spans. 
                var d0 = curveA.Domain.T1 - curveA.Domain.T0;
                var d1 = curveB.Domain.T1 - curveA.Domain.T0;
                
                var t0 = curveA.Domain.NormalizedParameterAt(maxAt);
                var t1 = curveB.Domain.NormalizedParameterAt(maxBt);
                var pt0 = curveA.PointAtNormalizedLength(t0);
                var pt1 = curveB.PointAtNormalizedLength(t1);
                
                RhinoApp.WriteLine("Max: {0:N3}, MaxT0: {1:N3}, MaxT1: {2:N3}.", max, maxAt, maxBt);
                RhinoApp.WriteLine("Domain - Crv0: {0:N3}, Crv0: {1:N3}", d0, d1);
                RhinoApp.WriteLine("Normalized t - Crv0: {0:N3}, Crv1: {1:N3}", t0, t1);
        
                var lineEric = new Line(pt0, pt1);
                doc.Objects.AddLine(lineEric);
                RhinoApp.WriteLine("Distance from Pts at Normalized t's: {0:N3}", lineEric.Length);
                
                // From @dale
                // using PointAt finds the correct spot in the domain.
                var line = new Line
                {
                    From = curveA.PointAt(maxAt),
                    To = curveB.PointAt(maxBt)
                };
                
                doc.Objects.AddLine(line);
                doc.Objects.AddPoint(line.From);
                doc.Objects.AddPoint(line.To);
                
                RhinoApp.WriteLine("Minimum deviation = {0}", min);
                RhinoApp.WriteLine("Maximum deviation = {0}", max);
                
                doc.Views.Redraw();
                
            }
            
            return Result.Success;
       
        }

       
    }
}
