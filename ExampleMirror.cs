using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace DiscourseExamples
{
    public class ExampleMirror : Command
    {
        public ExampleMirror()
        {
            Instance = this;
        }

        public static ExampleMirror Instance { get; private set; }

        public override string EnglishName => "ExampleMirror";

        private ObjRef[] GetGeometry(ObjectType geoType)
        {
            GetObject go = new GetObject();
            go.GeometryFilter = geoType;
            go.AcceptEnterWhenDone(false);
            go.EnablePreSelect(true, true);
            go.SubObjectSelect = false;
            go.SetCommandPrompt("Pick objects for mirror");
            go.GetMultiple(1, 0);

            if (go.CommandResult() == Result.Success)
                return go.Objects();
            else 
                return null;
        }

        private GeometryBase MirrorObject(GeometryBase obj)
        {
            if (obj == null)
                return null;

            var copy = obj.Duplicate();
            copy.Transform(Transform.Mirror(new Point3d(0, 0, 0), new Vector3d(1, 0, 0)));
            return copy;
        }

        private HistoryRecord WriteHistory(ObjRef obj)
        {
            const int version = 1;
            var histRecord = new HistoryRecord(this, version);
            histRecord.SetObjRef(0, obj);
            return histRecord;
        }

        private ObjRef ReadHistory(ReplayHistoryData histReplay)
        {
            return histReplay.GetRhinoObjRef(0);
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var objRefs = GetGeometry(ObjectType.Point | ObjectType.InstanceReference);
            if (objRefs == null || objRefs.Length < 1)
                return Result.Failure;

            foreach (var obj in objRefs)
            {
                var mirrored = MirrorObject(obj.Geometry());
                if (mirrored == null)
                    continue;

                var histRecord = WriteHistory(obj);
                switch (mirrored)
                {
                    case Rhino.Geometry.Point pt:
                    {
                        doc.Objects.AddPoint(pt.Location, null, histRecord, false);
                        continue;
                    }
                    case Rhino.Geometry.InstanceReferenceGeometry block:
                    {
                        var blockDef = doc.InstanceDefinitions.FindId(block.ParentIdefId);

                        // TODO I'm not sure what the reference parameter is referring to with normal geometry.
                        // bool reference: True if the object is from a reference file. Reference objects do not persist in archives
                        // https://developer.rhino3d.com/api/rhinocommon/rhino.docobjects.tables.objecttable/addpoint
                        //
                        // But since blocks can be a reference to an external file, I'm passing in IsReference from the block def.
                        doc.Objects.AddInstanceObject(blockDef.Index, block.Xform, null, histRecord, blockDef.IsReference); 
                        continue;

                        // TODO This is what I need help getting around.  
                        // Call _What on the mirrored block.  There is no history record.
                    }
                }
            }
            return Result.Success;
        }

        protected override bool ReplayHistory(ReplayHistoryData replayData)
        {
            base.ReplayHistory(replayData);

            var objRef = ReadHistory(replayData);
            if (replayData.Results.Length != 1 || objRef == null)
                return false;

            var existing = replayData.Results[0].ExistingObject;
            var mirrored = MirrorObject(objRef.Geometry());
            switch (mirrored)
            {
                case Point pt:
                    replayData.Results[0].UpdateToPoint(pt.Location, existing.Attributes);
                    return true;
                case InstanceReferenceGeometry block:
                    // TODO I have a bad feeling Replace is not going to support an instance ref 
                    // also, there's no documentation for ignore modes
                    // https://mcneel.github.io/rhinocommon-api-docs/api/RhinoCommon/html/M_Rhino_DocObjects_Tables_ObjectTable_Replace_26.htm
                    existing.SetCopyHistoryOnReplace(true);
                    RhinoDoc.ActiveDoc.Objects.Replace(existing.Id, block, true);
                    return true;
            }
            return false;
        }
    }
}
