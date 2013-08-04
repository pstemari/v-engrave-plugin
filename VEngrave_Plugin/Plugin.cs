/* -*- mode: csharp; c-basic-offset: 2 -*- 
 * Copyright 2013 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using CamBam;
using CamBam.CAD;
using CamBam.CAM;
using CamBam.Geom;
using CamBam.UI;
using CamBam.Values;

[assembly: InternalsVisibleTo("VEngrave_UnitTests")]

namespace VEngraveForCamBam {

  using CamBamExtensions;

  internal interface Logger {
    void Log(string msg);
    void Log(int level, string msg);
    void Log(string format, params object[] vars);
    void Log(int level, string format, params object[] vars);
  }

  internal class CamBamLogger : Logger {
    public void Log(string msg) {
      ThisApplication.AddLogMessage(msg);
    }
    public void Log(int level, string msg) {
      ThisApplication.AddLogMessage(level, msg);
    }
    public void Log(string format, params object[] vars) {
      ThisApplication.AddLogMessage(format, vars);
    }
    public void Log(int level, string format, params object[] vars) {
      ThisApplication.AddLogMessage(level, format, vars);
    }
  }

  internal class NullLogger : Logger {
    public void Log(string msg) { }
    public void Log(int level, string msg) { }
    public void Log(string format, params object[] vars) { }
    public void Log(int level, string format, params object[] vars) { }
  }

  public class Plugin {
    private CamBamUI _ui;
    private Logger _log = new CamBamLogger();

    public Plugin() : this(CamBamUI.MainUI)  {}

    public Plugin(CamBamUI ui) {
      _ui = ui;
      AttachToUI();
    }

    private void AttachToUI() {
      var aboutCommand = new ToolStripMenuItem();
      aboutCommand.Text = "About V-Engrave...";
      aboutCommand.Click += About;
      _ui.Menus.mnuPlugins.DropDownItems.Add(aboutCommand);

      var insertMOPCommand = new ToolStripMenuItem();
      insertMOPCommand.Text = "V-Engrave";
      insertMOPCommand.Image = Properties.VCarveResources.VCarveButton;
      insertMOPCommand.Click += InsertMOP;

      for (int i = 0; i < _ui.Menus.mnuMachining.DropDownItems.Count; ++i) {
        var item = _ui.Menus.mnuMachining.DropDownItems[i];
        if (item is ToolStripSeparator
            || i == _ui.Menus.mnuMachining.DropDownItems.Count - 1) {
          _ui.Menus.mnuMachining.DropDownItems.Insert(i, insertMOPCommand);
          break;
        }
      }

      insertMOPCommand = insertMOPCommand = new ToolStripMenuItem();
      insertMOPCommand.Text = "V-Engrave";
      insertMOPCommand.Image = Properties.VCarveResources.VCarveButton;
      insertMOPCommand.Click += InsertMOP;

      foreach (ToolStripItem item
               in _ui.ViewContextMenus.ViewContextMenu.Items) {
        if (item is ToolStripMenuItem
            && item.Name == "machineToolStripMenuItem") {
          ToolStripMenuItem menu = (ToolStripMenuItem) item;
          for (int i = 0; i < menu.DropDownItems.Count; ++i) {
            if (menu.DropDownItems[i] is ToolStripSeparator
                || i == menu.DropDownItems.Count - 1) {
              menu.DropDownItems.Insert(i, insertMOPCommand);
              break;
            }
          }
          break;
        }
      }

      // Hook into toolbar?

      // Hook into XML serialization
      if (CADFile.ExtraTypes == null) {
        CADFile.ExtraTypes = new List<Type>();
      }
      CADFile.ExtraTypes.Add(typeof(MOPVEngrave));

      // HACK: Per 10bulls, this fixes clipboard errors associated with
      // copying external assembly objects that have not been serialized.
      new XmlSerializer(typeof(MOPVEngrave))
        .Serialize(new MemoryStream(), new MOPVEngrave());
    }

    public static void InitPlugin(CamBamUI ui) {
      Plugin plugin = new Plugin(ui);
    }

    private void About(object sender, EventArgs e) {
      ThisApplication.MsgBox("VEngrave CamBam plug-in v0.001");
    }

    //private void _TestXMLCommand(object sender, EventArgs unused) {
    //  try {
    //    ClassHelper.SerializeToXmlFile(
    //      CamBamUI.MainUI.ActiveView.CADFile,
    //      @"C:\Users\Paul J. Ste. Marie\Documents\CADCAM\TestXMLSerialization.xml");
    //    //CBXmlSerializer serializer = new CBXmlSerializer(typeof(CADFile));
    //    //TextWriter writer = new StreamWriter(
    //    //        @"C:\Users\Paul J. Ste. Marie\Documents\CADCAM\TestXMLSerialization.xml");
    //    //serializer.WriteXml(new System.Xml.XmlTextWriter(writer),
    //    //    CamBamUI.MainUI.ActiveView.CADFile);
    //  } catch (InvalidOperationException e) {
    //    ThisApplication.AddLogMessage(
    //      "Exception {0} when serializing document, inner exception: {1}",
    //      e, e.InnerException);
    //  }
    //}

    private void InsertMOP(object sender, EventArgs e) {
      ICADView view = CamBamUI.MainUI.ActiveView;
      CADFile file = view.CADFile;
      object[] objects = view.SelectedEntities;
      MOPVEngrave mop = new MOPVEngrave(file, objects);
      CAMPart part = view.CADFile.EnsureActivePart(true);
      mop.Part = part;
      _ui.InsertMOP(mop);
      // part.MachineOps.Add(mop);
      file.Modified = true;

      // verify these

      CamBamUI.MainUI.CADFileTree.Machining.Expand();
      foreach (TreeNode partNode in view.DrawingTree.Machining.Nodes) {
        if (partNode.Tag == view.CADFile.ActivePart) {
          partNode.Expand();
          foreach (TreeNode opNode in partNode.Nodes) {
            if (opNode.Tag == mop) {
              mop.Name = opNode.Text;
              opNode.EnsureVisible();
              break;
            }
          }
          break;
        }
      }

      // _ui.InsertMOP ?
      foreach (var smop in CamBamUI.MainUI.CADFileTree.SelectedMOPs) {
        ThisApplication.AddLogMessage("SelectedMOPs: {0}", smop.Name);
      }
    }
  }

  // public interface IPathIterator {
  //   bool hasNext();
  //   Point2F next();
  // }

  // public class LineSegmentIterator : IPathIterator {
  //   private Point2F current;
  //   private int nLeft;
  //   private Vector2F dse;
  //   public LineSegmentIterator(Point2F start, Point2F end, double dl) {
  //     current = new Point2F(start.X, start.Y);
  //     dse = new Vector2F(start, end);
  //     double length = dse.Length;
  //     nLeft = (int) (length/dl) + 1;
  //     if (nLeft < 2) {
  //       nLeft = 2;
  //     }
  //     dse.X /= (nLeft - 1);
  //     dse.Y /= (nLeft - 1);
  //   }
  //   public bool hasNext() {
  //     return nLeft > 0;
  //   }
  //   public Point2F next() {
  //     Point2F rv = current;
  //     current = new Point2F(rv.X + dse.X, rv.Y + dse.Y);
  //     --nLeft;
  //     return rv;
  //   }
  // }

  // public class ArcIterator : IPathIterator {
  //   private Point2F center;
  //   private int nLeft;
  //   private double radius, theta, dTheta;
  //   public ArcIterator(Point2F start, Point2F end, double bulge, double dl) {
  //     radius = Geometry.ConvertBulgeToArc(start, end, bulge, out center);
  //     theta = Math.Atan2(start.Y - center.Y, start.X - center.X);
  //     double endTheta = Math.Atan2(end.Y - center.Y, end.X - center.X);
  //     if (bulge > 0) { // CCW
  //       if (endTheta < theta) {
  //         endTheta += 2*Math.PI;
  //       }
  //     } else { // CW
  //       if (endTheta > theta) {
  //         endTheta -= 2*Math.PI;
  //       }
  //     }
  //     dTheta = Math.Sign(endTheta - theta)*dl/radius;
  //     nLeft = (int) ((endTheta - theta)/dTheta + 1);
  //   }
  //   public bool hasNext() {
  //     return nLeft > 0;
  //   }
  //   public Point2F next() {
  //     Point2F rv = new Point2F(center.X + radius*Math.Cos(theta),
  //                              center.Y + radius*Math.Sin(theta));
  //     --nLeft;
  //     theta += dTheta;
  //     return rv;
  //   }
  // }

  [XmlType("MOPVEngrave")]
  public class MOPVEngrave : MOPFromGeometry, IIcon {
    // Smallest number such that 1.0 + DBL_EPSILON != 1.0
    const double DBL_EPSILON = 2.2204460492503131e-016;
    // Radians per degree
    const double DEGREES = Math.PI/180.0;
    // Log levels
    const int ERROR = 0, WARNING = 1, INFO = 2, DEBUG = 3, TRACE = 4,
      FINE = 5, FINER = 6, FINEST = 7;
    // logger
    private Logger _log = new CamBamLogger();
    internal void SetLogger(Logger newValue) { _log = newValue; }

    #region Properties
    #region UIProperties
    // UI properties
    [XmlIgnore]
    System.Drawing.Image IIcon.ActiveIconImage {
      get { return Properties.VCarveResources.VCarveButton; }
    }

    [XmlIgnore]
    string IIcon.ActiveIconKey {
      get { return "VCarveButton"; }
    }

    [XmlIgnore]
    System.Drawing.Image IIcon.InactiveIconImage {
      get { return Properties.VCarveResources.VCarveButton; }
    }

    [XmlIgnore]
    string IIcon.InactiveIconKey {
      get { return "VCarveButton"; }
    }
    #endregion

    #region OperationProperties
    // Operation properties
    #region PathIncrementProperty
    const double DEFAULT_PATH_INCREMENT = 0.001;
    [DisplayName("Path Step Size"),
     Description("Length of step in path"),
     DefaultValue(DEFAULT_PATH_INCREMENT),
     Category("Options"), CBAdvancedValue, XmlIgnore]
    public CBValue<double> PathIncrement {
      set {
        if (value.IsValue && value.Value <= 0) {
          _pathIncrement.SetState(CBValueStates.Error);
          _log.Log(ERROR, "PathIncrement must be greater than 0.");
          return;
        }
        _pathIncrement = value;
        if (_pathIncrement.IsDefault) {
          _pathIncrement.SetCache(DEFAULT_PATH_INCREMENT);
          _pathIncrement.Value = _pathIncrement.Cached;
        }
      }
      get { return _pathIncrement; }
    }
    public bool ShouldSerializePathIncrement() {
      _log.Log(FINEST, "ShouldSerializePathIncrement(): {0}",
               !_pathIncrement.IsDefault);
      return !_pathIncrement.IsDefault;
    }
    [XmlElement("PathIncrement")]
    private CBValue<double> _pathIncrement;
    #endregion
    #region MaxCornerAngleProperty
    const double DEFAULT_MAX_CORNER_ANGLE = 135.0;    // degrees
    [DisplayName("Max Corner Angle"),
     Description("Maximum corner angle for sharp corners. "
                 + "If the corner angle is less than or equal to this value, "
                 + "the tool tip will be raised up into the corner to form a "
                 + "sharp point"),
     Category("Options"),
     DefaultValue(DEFAULT_MAX_CORNER_ANGLE),
     CBAdvancedValue, XmlIgnore]
    public CBValue<double> MaxCornerAngle {
      set {
        if (value.IsValue && (value.Value < 0 || 180 < value.Value)) {
          _maxCornerAngle.SetState(CBValueStates.Error);
          _log.Log(ERROR, "MaxCornerAngle must be between 0 and 180 degrees.");
          return;
        }
        _maxCornerAngle = value;
        if (_maxCornerAngle.IsDefault) {
          _maxCornerAngle.SetCache(DEFAULT_MAX_CORNER_ANGLE);
          _maxCornerAngle.Value = _maxCornerAngle.Cached;
        }
      }
      get { return _maxCornerAngle; }
    }
    public bool ShouldSerializeMaxCornerAngle() {
      _log.Log(FINEST, "ShouldSerializeMaxCornerAngle(): {0}",
               !_maxCornerAngle.IsDefault);
      return !_maxCornerAngle.IsDefault;
    }
    [XmlElement("MaxCornerAngle")]
    private CBValue<double> _maxCornerAngle;
    #endregion
    #region MaxDepthProperty
    [DisplayName("Max Depth"),
     Description("Maximum depth for cuts"), Category("Cutting Depth"),
     XmlIgnore]
    public CBValue<double> MaxDepth {
      set {
        if (value.IsValue && value.Value <= 0) {
          _maxDepth.SetState(CBValueStates.Error);
          _log.Log(ERROR, "MaxDepth must be greater than zero.");
          return;
        }
        _maxDepth = value;
        if (_maxDepth.IsDefault) {
          _maxDepth.SetState(CBValueStates.Auto);
          _maxDepth.ClearCache();
        }
      }
      get { return _maxDepth; }
    }
    public bool ShouldSerializeMaxDepth() {
      _log.Log(FINEST, "ShouldSerializeMaxDepth(): {0}", !_maxDepth.IsDefault);
      return !_maxDepth.IsDefault;
    }
    [XmlElement("MaxDepth")]
    private CBValue<double> _maxDepth;
    private double ComputeAutoMaxDepth() {
      return 0.5*(ToolDiameter.Cached - ToolTipDiameter.Cached)
        /Math.Tan(0.5*ToolVAngle.Cached*DEGREES);
    }
    #endregion
    #region ToolTipDiameterProperty
    const double DEFAULT_TOOL_TIP_DIAMETER = 0.0;
    [DisplayName("Tool Tip Diameter"),
     Description("Tip diameter of engraving tool"),
     DefaultValue(DEFAULT_TOOL_TIP_DIAMETER), Category("Tool"), XmlIgnore]
    public CBValue<double> ToolTipDiameter {
      set {
        if (value.IsValue && value.Value < 0) {
          _maxCornerAngle.SetState(CBValueStates.Error);
          _log.Log(ERROR, "Tip diameter must be greater than or equal to 0.");
          return;
        }
        _toolTipDiameter = value;
        if (_toolTipDiameter.IsDefault) {
          _toolTipDiameter.SetCache(DEFAULT_TOOL_TIP_DIAMETER);
          _toolTipDiameter.Value = _toolTipDiameter.Cached;
        }
      }
      get { return _toolTipDiameter; }
    }
    public bool ShouldSerializeToolTipDiameter() {
      _log.Log(FINEST, "ShouldSerializeToolTipDiameter(): {0}",
               !_toolTipDiameter.IsDefault);
      return !_toolTipDiameter.IsDefault;
    }
    [XmlElement("ToolTipDiameter")]
    private CBValue<double> _toolTipDiameter;
    #endregion
    #region ToolVAngleProperty
    const double DEFAULT_TOOL_V_ANGLE = 90.0;
    [DisplayName("Tool V-Angle"),
     Description("V-angle of engraving tool"),
     DefaultValue(DEFAULT_TOOL_V_ANGLE), Category("Tool"), XmlIgnore]
    public CBValue<double> ToolVAngle {
      set {
        if (value.IsValue && (value.Value <= 0 || 180 <= value.Value)) {
          _toolVAngle.SetState(CBValueStates.Error);
          _log.Log(ERROR, "ToolVAngle must be between 0 and 180 degrees.");
          return;
        }
        _toolVAngle = value;
        if (_toolVAngle.IsDefault) {
          _toolVAngle.SetCache(DEFAULT_TOOL_V_ANGLE);
          _toolVAngle.Value = _toolVAngle.Cached;
        }
        _cotHalfVAngle = 1.0/Math.Tan(0.5*_toolVAngle.Value*DEGREES);
      }
      get { return _toolVAngle; }
    }
    public bool ShouldSerializeToolVAngle() {
      _log.Log(FINEST, "ShouldSerializeToolVAngle(): {0}",
               !_toolVAngle.IsDefault);
      return !_toolVAngle.IsDefault;
    }
    [XmlElement("ToolVAngle")]
    private CBValue<double> _toolVAngle;
    [XmlIgnore]
    private double _cotHalfVAngle;
    #endregion
    #endregion

    #region PropertyCallbacks
    public override bool EvaluateAutoCBValue(ICBValue toset, string name) {
      _log.Log(FINEST, "EvaluateAutoValue {0}", name);
      if (name.Equals("MaxDepth")) {
        toset.SetValue(ComputeAutoMaxDepth());
        toset.SetState(CBValueStates.Auto);
        return true;
      } else {
        return base.EvaluateAutoCBValue(toset, name);
      }
    }
    #endregion
    #endregion

    public MOPVEngrave() {
      PathIncrement = CBValue<double>.Default;
      MaxDepth = new CBValue<double>();
      MaxDepth.SetState(CBValueStates.Auto);
      ToolTipDiameter = CBValue<double>.Default;
      ToolVAngle = CBValue<double>.Default;
      MaxCornerAngle = CBValue<double>.Default;
    }

    public MOPVEngrave(MOPVEngrave src) : base(src) {
      PathIncrement = src.PathIncrement;
      MaxDepth = src.MaxDepth;
      ToolTipDiameter = src.ToolTipDiameter;
      ToolVAngle = src.ToolVAngle;
      MaxCornerAngle = src.MaxCornerAngle;
    }

    public MOPVEngrave(CADFile file, object[] objects)
      : base(file, objects) { }

    [XmlIgnore]
    public override string MOPTypeName {
      get { return "VEngrave"; }
    }

    public override CamBam.CAM.MachineOp Clone() {
      return new MOPVEngrave(this);
    }

    protected override void _GenerateToolpathsWorker() {
      try {
        ClearToolpaths();

        ShapeList shapes = new ShapeList();
        shapes.CloneEntities = true;
        shapes.ApplyTransformations = true;
        shapes.AddEntities(_CADFile, PrimitiveIds);

        if (shapes.Count == 0) {
          return;
        }

        ToolpathSequence tseq = new ToolpathSequence(this);
        foreach (ShapeListItem item in shapes) {
          if (item.Shape is Polyline || item.Shape is CamBam.CAD.Region) {
            ComputeVEngraveToolpath(tseq, item);
          } else {
            _log.Log(ERROR, "I don't know what to do with a {0}",
                    item.Shape.GetType());
          }
        }

        // Replace the existing toolpath sequence...
        SetToolpathSequence(tseq);

        Toolpaths2.UseSplitPoint3D = true;
        Toolpaths2.ToolDiameter = ToolDiameter.Cached;

        // Determine the cutting order
        if (OptimisationMode.Cached == OptimisationModes.Standard) {
          Toolpaths2.BuildCutOrder(
            CutOrderingOption.LevelFirst,
            MillingDirectionOptions.Mixed,
            GetDistanceThreshold(), StartPoint);
        } else if (OptimisationMode.Cached
                   == OptimisationModes.Experimental) {
          Toolpaths2.BuildCutOrderEx_Inside(
            CutOrderingOption.LevelFirst,
            MillingDirectionOptions.Mixed,
            SpindleDirection.Cached,
            GetDistanceThreshold(), StartPoint,
            InsideOutsideOptions.Inside);
        } else {
          Toolpaths2.BuildSimpleCutOrder();
        }
        // Used to display the cut width area (optional)
        //if (ToolDiameter.Cached > 0) {
        //    Toolpaths2.CalculateCutWidths(ToolDiameter.Cached);
        //}
        //Toolpaths2.CalculateCutWidths(0.001 /* dummy value*/);
        ComputeCutSurfaces();
        //_log.log(TRACE, "CutSurfaces:");
        //foreach (Surface s in Toolpaths2.CutWidths) {
        //  _log.log(TRACE,
        //    "  Surface: {0}, nfaces: {1}, npoints: {2}",
        //    s, s.Faces.Length, s.Points.Count);
        //  //foreach (TriangleFace face in s.Faces) {
        //  //  Point3F a = s.Points[face.A];
        //  //  Point3F b = s.Points[face.B];
        //  //  Point3F c = s.Points[face.C];
        //  //  log.log(TRACE,
        //  //    "  Face: ({0},{1},{2}), ({3},{4},{5}), ({6},{7},{8})",
        //  //    a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z);
        //  //}
        //}

        // Detect Rapid moves (optional...for display only)
        Toolpaths2.DetectRapids(this, GetDistanceThreshold());

        // Must set MachineOpStatus == OK, otherwise g-code is
        // not allowed
        if (MachineOpStatus == MachineOpStatus.Unknown) {
          MachineOpStatus = MachineOpStatus.OK;
        }
      } catch (Exception ex) {
        MachineOpStatus = MachineOpStatus.Errors;
        ThisApplication.HandleException(ex);
      } finally {
        // ends and cleans up the worker thread
        _GenerateToolpathsFinal();
      }
    }

    internal void ComputeCutSurfaces() {
      List<Surface> cutsurfaces = new List<Surface>();
      double tanHalfVAngle = Math.Tan(0.5*ToolVAngle.Cached*DEGREES);
      double toolTipRadius = 0.5*ToolTipDiameter.Cached;
      foreach (ToolpathItem tpi in Toolpaths2.Toolpaths)  {
        Polyline toolpath = (Polyline) tpi.Toolpath.Clone();
        toolpath.CheckForClosed(PathIncrement.Cached);
        _log.Log(DEBUG, "Calculating cut surfaces for toolpath "
                 + "item {0}, point count: {1}, closed: {2}",
                 tpi, toolpath.Points.Count, toolpath.Closed);
        var exMin = GeomExtremaMin;
        var exMax = GeomExtremaMax;
        toolpath.RemoveDuplicatePoints(0.1*PathIncrement.Cached);
        SurfaceBuilder bob = new SurfaceBuilder(
            new Rect2F(exMin.To2D(), exMax.To2D()));
        bob.setLogger(_log);
        // faces should have the front side pointing up
        bob.Front = new Vector3F(0, 0, 1);

        double startCutRadius = toolTipRadius
          + (StockSurface.Cached
             - toolpath.Points[0].Point.Z)*tanHalfVAngle;
        Point3F current = toolpath.Points[0].Point;

        // Set up variables that carry over from one line segment
        // to the next
        Point3F next = toolpath.Points[toolpath.NextSegment(0)].Point;
        Vector2F currstep = new Vector2F(current.To2D(), next.To2D());
        int beyondi = toolpath.NextSegment(toolpath.NextSegment(0));
        Vector2F invector, currmiter;
        Point3F startBottomOutsideStart, startTopOutsideStart;
        if (!toolpath.Closed) {
          _log.Log(TRACE, "Adding starting endcap to toolpath item {0}", tpi);
          ComputeCutEndcap(current, currstep, bob);
          invector = currstep;
          currmiter = currstep.Normal().Unit();
          startBottomOutsideStart = startTopOutsideStart = Point3F.Undefined;
        } else {
          invector = new Vector2F(
            toolpath.Points[toolpath.PrevSegment(0)].Point.To2D(),
            current.To2D());
          currmiter = Geometry.Bisect(
            Geometry.Multiply(-1, invector), currstep);
          startBottomOutsideStart = toolTipRadius < DBL_EPSILON
            ? current : Geometry.Add(current,
                                     Geometry.Multiply(-toolTipRadius,
                                                       currmiter.To3D()));
          startTopOutsideStart = Geometry.Add(
            current.To2D(), Geometry.Multiply(-startCutRadius,
                                              currmiter))
            .To3D(StockSurface.Cached);
        }

        Point3F startBottomInside = toolTipRadius < DBL_EPSILON
            ? current : Geometry.Add(current,
                                     Geometry.Multiply(toolTipRadius,
                                                       currmiter.To3D()));
        Point3F startTopInside = Geometry.Add(
          current.To2D(),
          Geometry.Multiply(startCutRadius,
                            currmiter)).To3D(StockSurface.Cached);

        for (int i = 0; i < toolpath.NumSegments; ++i) {
          _log.Log(TRACE, "  Generating cut surfaces for toolpath item {0}"
                   + " segment {1}", tpi, i);
          // If outside normal is on the same side as as the
          // miter vector flip it
          Vector2F outsideNormal = currstep.Normal().Unit();
          if (Math.Sign(Vector2F.Determinant(currstep, outsideNormal))
              == Math.Sign(Vector2F.Determinant(currstep, currmiter))) {
            outsideNormal.Invert();
          }

          // Compute initial bend for this segment
          Point3F startBottomOutsideEnd = Geometry.Add(
            current,
            Geometry.Multiply(toolTipRadius, outsideNormal.To3D()));
          Point3F startTopOutsideEnd
            = Geometry.Add(current.To2D(),
                           Geometry.Multiply(startCutRadius,
                                             outsideNormal)).To3D(
                                               StockSurface.Cached);

          // Initial "pie wedge".
          if (!startTopOutsideStart.IsUndefined
              && !startBottomOutsideStart.IsUndefined
             // Check for an actual bend that we need to join together
              && Point3F.Distance(startTopOutsideStart,
                                  startTopOutsideEnd) >= DBL_EPSILON)  {
            ComputeCutPartialCone(current.To2D(), startBottomInside,
                startTopOutsideStart, startBottomOutsideStart,
                startTopOutsideEnd, startBottomOutsideEnd,
                Vector2F.Determinant(currstep, currmiter),
                bob);
          }

          // Compute main body of this segment
          double endCutRadius = toolTipRadius
            + (StockSurface.Cached - next.Z)*tanHalfVAngle;
          Vector2F nextmiter, outvector;
          Point3F beyond;
          if (beyondi < 0) {
            beyond = Point3F.Undefined;
            outvector = currstep;
            nextmiter = currstep.Normal().Unit();
          } else {
            // This assumes no arcs in the toolpath, but
            // currently I don't attempt to optimize the
            // toolpath to use arcs or combine co-linear
            // segments.
            beyond = toolpath.Points[beyondi].Point;
            outvector = new Vector2F(next.To2D(), beyond.To2D());
            if (outvector.Length < DBL_EPSILON) {
              _log.Log(WARNING, "zero-length segment in toolpath at {0}, aborting",
                       next);
              return;
            }
            nextmiter = Geometry.Bisect(
              Geometry.Multiply(-1, currstep), outvector);
          }

          // If outside normal is on the same side as as the
          // miter vector flip it and exchange the sides of
          // the points we're joining with. This handles the
          // zig-zag case.
          if (Math.Sign(Vector2F.Determinant(outvector, outsideNormal))
              == Math.Sign(Vector2F.Determinant(outvector, nextmiter))) {
            outsideNormal.Invert();
            Point3F temp = startTopOutsideEnd;
            startTopOutsideEnd = startTopInside;
            startTopInside = temp;
            temp = startBottomOutsideEnd;
            startBottomOutsideEnd = startBottomInside;
            startBottomInside = temp;
          }

          Point3F endBottomInside = Geometry.Add(
            next, Geometry.Multiply(toolTipRadius, nextmiter.To3D()));
          Point3F endTopInside = Geometry.Add(
            next.To2D(), Geometry.Multiply(endCutRadius, nextmiter))
            .To3D(StockSurface.Cached);
          Point3F endBottomOutsideStart = Geometry.Add(
            next, Geometry.Multiply(toolTipRadius, outsideNormal.To3D()));
          Point3F endTopOutsideStart = Geometry.Add(
            current.To2D(), Geometry.Multiply(startCutRadius, outsideNormal))
            .To3D(StockSurface.Cached);

          // Note: Let the surface builder suppress degenerate facets
          if (toolTipRadius < DBL_EPSILON) {  // sharp v-groove
            bob.AddFacets(startTopOutsideEnd, current, next,
                          endTopOutsideStart);
            bob.AddFacets(startTopInside, current, next,
                          endTopInside);
          } else {                        // flat-bottomed v-groove
            bob.AddFacets(startTopOutsideEnd, startBottomOutsideEnd,
                        endBottomOutsideStart, endTopOutsideStart);
            bob.AddFacets(startBottomInside, startBottomOutsideEnd,
                        endBottomOutsideStart, endBottomInside);
            bob.AddFacets(startTopInside, startBottomInside,
                        endBottomInside, endTopInside);
          }

          // Compute ending pie wedge
          Point3F endBottomOutsideEnd, endTopOutsideEnd;
          var outDet = Vector2F.Determinant(currstep, outvector);
          if (beyond.IsUndefined || Math.Abs(outDet) < DBL_EPSILON) {
            endBottomOutsideEnd = endTopOutsideEnd
              = Point3F.Undefined;
          } else {
            endBottomOutsideEnd = Geometry.Add(
                next, Geometry.Multiply(-toolTipRadius,
                                        nextmiter.To3D()));
            endTopOutsideEnd = Geometry.Add(
                next.To2D(), Geometry.Multiply(-endCutRadius,
                                                nextmiter))
                .To3D(StockSurface.Cached);
            // Check for an actual bend that we need to join together
            if (Point3F.Distance(endTopOutsideStart,
                                 endTopOutsideEnd) > DBL_EPSILON) {
              ComputeCutPartialCone(next.To2D(), endBottomInside,
                                    endTopOutsideStart, endBottomOutsideStart,
                                    endTopOutsideEnd, endBottomOutsideEnd,
                                    outDet, bob);
            }
          }

          // advance
          startCutRadius = endCutRadius;
          invector = currstep;
          current = next;
          currstep = outvector;
          currmiter = nextmiter;
          next = beyond;
          beyondi = toolpath.NextSegment(beyondi);

          startBottomInside = endBottomInside;
          startTopInside = endTopInside;
          startBottomOutsideStart = endBottomOutsideEnd;
          startTopOutsideStart = endTopOutsideEnd;
        }
        if (!toolpath.Closed) {
          ComputeCutEndcap(current,
                            Geometry.Multiply(-1, currstep),
                            bob);
        }
        cutsurfaces.Add(bob.Build());
      }
      Toolpaths2.CutWidths = cutsurfaces;
    }

    internal void ComputeCutPartialCone(Point2F center, Point3F facetJoin,
                                         Point3F startTop, Point3F startBottom,
                                         Point3F endTop, Point3F endBottom,
                                         double bendCrossProduct,
                                         SurfaceBuilder bob) {
      Point3F prevTop = startTop;
      Point3F prevBottom = startBottom;
      var startRadius = new Vector2F(center, startTop.To2D());
      double startTheta = startRadius.Angle;
      double cutRadius = startRadius.Length;
      double endTheta = new Vector2F(center, endTop.To2D()).Angle;
      double toolTipRadius = ToolTipDiameter.Cached/2;
      // +1 theta increasing, -1 decreasing
      int dirTheta = Math.Sign(bendCrossProduct);
      if (dirTheta == 0) {  // punt
        if (toolTipRadius < DBL_EPSILON) {
          bob.AddFacet(facetJoin, startTop, endTop);
        } else {
          bob.AddFacet(facetJoin, startBottom, endBottom);
          bob.AddFacets(startTop, startBottom, endBottom, endTop);
        }
        return;
      }
      while (dirTheta*endTheta < dirTheta*startTheta) {
        endTheta += dirTheta*360*DEGREES;
      }
      // divide interval into increments of no more than 5 degrees
      double dTheta = (endTheta - startTheta)
            / Math.Max(1, Math.Ceiling((endTheta - startTheta)/5*DEGREES));
      _log.Log(FINE, "  pivoting {0} degrees at {1}",
               (endTheta - startTheta)/DEGREES, center);
      for (double theta = startTheta + dTheta;
            dirTheta*theta < dirTheta*endTheta; theta += dTheta) {
        Vector2F currRadius = new Vector2F(Math.Cos(theta), Math.Sin(theta));
        Point3F currTop = Geometry.Add(center,
            Geometry.Multiply(cutRadius, currRadius))
                .To3D(StockSurface.Cached);
        Point3F currBottom = Geometry.Add(center,
            Geometry.Multiply(toolTipRadius, currRadius))
                .To3D(startBottom.Z);
        if (toolTipRadius < DBL_EPSILON) {
          bob.AddFacet(prevTop, currTop, facetJoin);
        } else {
          bob.AddFacet(prevBottom, currBottom, facetJoin);
          bob.AddFacets(prevBottom, prevTop, currTop, currBottom);
        }
        prevTop = currTop; prevBottom = currBottom;
      }
      if (toolTipRadius < DBL_EPSILON) {
        bob.AddFacet(prevTop, endTop, facetJoin);
      } else {
        bob.AddFacet(prevBottom, endBottom, facetJoin);
        bob.AddFacets(prevBottom, prevTop, endTop, endBottom);
      }
    }

    internal void ComputeCutEndcap(
        Point3F endpoint, Vector2F openDirection, SurfaceBuilder bob) {
      double bottomRadius = ToolTipDiameter.Cached/2;
      double topRadius = bottomRadius
        + (StockSurface.Cached - endpoint.Z)*Math.Tan(
          0.5*ToolVAngle.Cached*DEGREES);
      double endAngle = openDirection.Normal().Angle;
      double startAngle = endAngle - Math.PI;
      int nPoints = Math.Max(8, (int) (Math.PI*topRadius
                                       /PathIncrement.Cached));

      Point3F prevTop = new Point3F(
        endpoint.X + topRadius*Math.Cos(startAngle),
        endpoint.Y + topRadius*Math.Sin(startAngle),
        StockSurface.Cached);
      Point3F prevBottom = new Point3F(
        endpoint.X + bottomRadius*Math.Cos(startAngle),
        endpoint.Y + bottomRadius*Math.Sin(startAngle),
        endpoint.Z);
      for (int i = 1; i <= nPoints; ++i) {
        double angle = startAngle + i*(endAngle - startAngle)/nPoints;
        Point3F currTop = new Point3F(
          endpoint.X + topRadius*Math.Cos(angle),
          endpoint.Y + topRadius*Math.Sin(angle),
          StockSurface.Cached);
        Point3F currBottom = new Point3F(
          endpoint.X + bottomRadius*Math.Cos(angle),
          endpoint.Y + bottomRadius*Math.Sin(angle),
          endpoint.Z);
        bob.AddFacets(prevTop, prevBottom, currBottom, currTop);
        prevTop = currTop;
        prevBottom = currBottom;
      }
    }

    // Override to emit macros, etc
     public override bool PreProcess(CamBam.CAM.MachineOpToGCode gcg) {
         return base.PreProcess(gcg);
     }

    public override void PostProcess(MachineOpToGCode gcg) {
      // lets the stock model know where the current top of the
      // uncut stock is...
      gcg.DefaultStockHeight = StockSurface.Cached;

      if (Toolpaths2 != null) {
        foreach (int i in Toolpaths2.CutOrder) {
          ToolpathItem item = Toolpaths2.Toolpaths[i];
          Polyline tpath = item.Toolpath;

          // this bit is used when you have multiple,
          // repeated toolpaths at depth increments...
          if (item.ZOffset != 0) {
            tpath = (Polyline) tpath.Clone();
            Matrix4x4F trans = new Matrix4x4F();
            trans.Translate(0, 0, item.ZOffset);
            tpath.ApplyTransformation(trans);
          }

          // double.NaN will use the Z value from the
          // polyline (otherwise you can specify a Z depth)
          gcg.AppendPolyLine(tpath, double.NaN);
        }
      }
      base.PostProcess(gcg);
    }

    internal void ComputeVEngraveToolpath(ToolpathSequence tseq,
                                           ShapeListItem item) {
      _cotHalfVAngle = 1.0/Math.Tan(0.5*ToolVAngle.Cached*DEGREES);
      int depthIndex = 0;
      int offsetIndex = 0;
      if (item.Shape is Polyline) {
        List<Polyline> outlines = new List<Polyline>(1);
        outlines.Add((Polyline) item.Shape);
        FollowOutline(tseq, (Polyline) item.Shape, outlines,
                       item.EntityID, parentID: -1,
                       offsetIndex: offsetIndex++, depthIndex: depthIndex,
                       traceInside: true);
      } else if (item.Shape is Region) {
        ComputeRegionToolpaths(tseq, (Region) item.Shape, item.EntityID,
                                ref depthIndex, ref offsetIndex);
      } else if (item.Shape is MText) {
        foreach (Region region in ((MText) item.Shape).ToRegions()) {
          // somehow parent id ought to be involved here
          ComputeRegionToolpaths(tseq, region, item.EntityID,
                                  ref depthIndex, ref offsetIndex);
        }
      } else {
        _log.Log(ERROR, "Don't know what to do with {0} entity {1}",
                item.Shape.GetType(), item.EntityID);
      }
    }

    internal void ComputeRegionToolpaths(ToolpathSequence tseq,
                                          Region region,
                                          EntityIdentifier entityID,
                                          ref int depthIndex,
                                          ref int offsetIndex) {
      List<Polyline> outlines = new List<Polyline>(region.HoleCurves);
      outlines.Add(region.OuterCurve);
      FollowOutline(tseq, region.OuterCurve, outlines, entityID, parentID: -1,
                     offsetIndex: offsetIndex++, depthIndex: depthIndex,
                     traceInside: true);
      int holenum = 0;
      foreach (Polyline phole in region.HoleCurves) {
        EntityIdentifier holeid = new EntityIdentifier(entityID.EntityID,
                                                       entityID.SubItem1,
                                                       holenum++);
        FollowOutline(tseq, phole, outlines,
                       offsetIndex: offsetIndex++, depthIndex: depthIndex,
                       outlineID: holeid, parentID: entityID.EntityID,
                       traceInside: false);
      }
    }

    internal void FollowOutline(
        ToolpathSequence tseq, Polyline outline, List<Polyline> outlines,
        EntityIdentifier outlineID, int parentID, int offsetIndex, 
        int depthIndex, bool traceInside) {
      double dl = PathIncrement.Cached;
      _log.Log(DEBUG, "Following polyline {0}, direction: {1}",
               outline.ID, outline.Direction);
      if (!outline.Closed) {
        _log.Log(ERROR, "I don't know what to do with open polyline {0}",
                outlineID);
      } else if (outline.Direction == RotationDirection.Unknown) {
        _log.Log(ERROR, "I don't know what to do with unknown rotation"
                + " polyline {0}", outlineID);
      } else {
        bool leftIsInside = (outline.Direction
                             == RotationDirection.CCW) == traceInside;
        double thresholdAngle = MaxCornerAngle.Cached*DEGREES;
        PolylineItemList point = outline.Points;
        Polyline toolpath = new Polyline();
        Vector2F nextNormal;
        Geometry.CornerType startCornerType
          = Geometry.GetCornerType(outline, 0,
                                   thresholdAngle, leftIsInside,
                                   out nextNormal);
        Point2F start = point[0].Point.To2D();
        Point2F adjustedStart = AdjustCornerPoint(0, startCornerType, outline, outlines);
        double lastRadius = -1;
        for (int i = 0; i < outline.Points.Count; ++i) {
          _log.Log(TRACE, "Processing segment[{0}], starting point: {1}",
                   i, point[i]);
          double bulge = point[i].Bulge;
          int previ = outline.PrevSegment(i);
          int nexti = outline.NextSegment(i);
          Point2F end = point[nexti].Point.To2D();
          Geometry.CornerType endCornerType
            = Geometry.GetCornerType(outline, nexti,
                                     thresholdAngle, leftIsInside,
                                     out nextNormal);
          Point2F adjustedEnd = AdjustCornerPoint(nexti, endCornerType, outline, outlines);
          Vector2F dse = new Vector2F(start, end);
          double len = dse.Length;
          if (len < DBL_EPSILON) {
            _log.Log(WARNING, "Primitive {0}[{1}] is too short", outline.ID, i);
          }
          Vector2F curNormal = new Vector2F();
          if (Math.Abs(bulge) < DBL_EPSILON) {
            // Normal points to the side we trace out,
            // i.e. left for an outside CCW curve.
            curNormal = dse.Unit().Normal();
            if (leftIsInside) {
              curNormal.Invert();
            }
            // The avoids a partial step at the end and skipping
            // the corner.
            int stepsLeft = (int) Math.Max(1, Math.Ceiling(len/dl));
            double adj_dl= len/stepsLeft;
            Vector2F dp = Geometry.Multiply(dse.Unit(), adj_dl);
            Point2F current = start;
            while (stepsLeft-- >= 0) { // include both first and last points
              lastRadius = AnalyzePoint(outline, i, current, curNormal,
                                         Double.MaxValue, outlines,
                                         startCornerType, endCornerType,
                                         toolpath, leftIsInside);
              // Be certain to hit the end exactly.  Overshoot causes issues.
              current = stepsLeft == 0 ? end : Geometry.Add(current, dp);
            }
          } else {
            Point2F center;
            double arcRadius = Geometry.ConvertBulgeToArc(
              start, end, bulge, out center);
            int direction = Math.Sign(bulge);
            double startTheta = Math.Atan2(start.Y - center.Y,
                                           start.X - center.X);
            double endTheta = Geometry.NormalizeAngularInterval(
                direction, startTheta, Math.Atan2(end.Y - center.Y,
                                                  end.X - center.X));
            int stepsLeft = (int) Math.Max(
              Math.Abs((endTheta - startTheta)
                       /(dl/arcRadius)), 1);
            double dtheta = (endTheta - startTheta)/stepsLeft;
            double theta = startTheta;
            curNormal = new Vector2F(center, start).Unit();
            Point2F current = start;
            while (stepsLeft-- >= 0) {
              double maxRadius = Double.MaxValue;
              // These correspond to the arc convex to
              // the outside of the profile
              if ((direction > 0) == leftIsInside) {
                curNormal.Invert();
                maxRadius = arcRadius;
              }
              lastRadius = AnalyzePoint(outline, i, current, curNormal,
                                         maxRadius, outlines,
                                         startCornerType, endCornerType,
                                         toolpath, leftIsInside);
              theta += dtheta;
              // Be certain to hit the end exactly.  Overshoot causes issues.
              if (stepsLeft > 0) {
                curNormal = new Vector2F(Math.Cos(theta),
                                         Math.Sin(theta));
                current = Geometry.Add(
                    center, Geometry.Multiply(curNormal, arcRadius));
              } else if (stepsLeft == 0) {
                curNormal = new Vector2F(center, end).Unit();
                current = end;
              } else {
                // leave curNomal and current intact
              }
            }
          }
          if (endCornerType == Geometry.CornerType.Outside) {
            // Swivel around outside corners, varying the normal from
            // curNormal to the normal at the start of the next segment.
            double startTheta = curNormal.Angle;
            int direction = leftIsInside ? -1 : +1;
            double dTheta = direction*(lastRadius < DBL_EPSILON
                                       ? 5*DEGREES
                                       : dl/lastRadius);
            double endTheta = Geometry.NormalizeAngularInterval(
                direction, startTheta, nextNormal.Angle);
            _log.Log(TRACE, "Swiveling at {0} from {1} degrees to {2}",
                     end, startTheta/DEGREES, endTheta/DEGREES);
            for (double theta = startTheta + dTheta;
                 direction*theta < direction*(endTheta - dTheta/2);
                 theta += dTheta) {
              AnalyzePoint(outline, i, end,
                            new Vector2F(Math.Cos(theta), Math.Sin(theta)),
                            Double.MaxValue, outlines,
                            startCornerType, endCornerType,
                            toolpath, leftIsInside);
            }
          }
          startCornerType = endCornerType;
          start = end;
          adjustedStart = adjustedEnd;
        }
        toolpath.CheckForClosed(PathIncrement.Cached);
        _log.Log(DEBUG, "Adding {0} item toolpath for entity {1}",
                 toolpath.Points.Count, outlineID);
        tseq.Add(/* depthIndex */ 0,
                 offsetIndex,
                 outlineID,
                 parentID,
                 toolpath,
                 outline.Direction,     // ???
                 /* sourcepoint */ Point3F.Undefined,
                 /* zoffset */ StockSurface.Cached);
      }
    }

    internal Point2F AdjustCornerPoint(int cp, Geometry.CornerType cornerType,
                                       Polyline outline, List<Polyline> outlines) {
      if (cornerType != Geometry.CornerType.SmoothInside) {
        return outline.Points[cp].Point.To2D();
      }
      // TODO: Compute center of circle tangent to corner sides and one other.
      return outline.Points[cp].Point.To2D();
    }

    internal double AnalyzePoint(
      Polyline currentOutline, int currentItem,
      Point2F current, Vector2F normal, double maxRadius,
      List<Polyline> outlines,
      Geometry.CornerType startCornerType,
      Geometry.CornerType endCornerType,
      Polyline toolpath, bool leftIsInside) {
      double radius = maxRadius;
      Polyline selectedPolyline = null;
      int selectedItem = -1;
      foreach (Polyline outline in outlines) {
        PolylineItem[] point = outline.Points.ToArray();
        for (int i = 0; i < outline.Points.Count; ++i) {
          if (outline == currentOutline
              && (startCornerType != Geometry.CornerType.SharpInside
                      && i == currentOutline.PrevSegment(currentItem)
                  || i == currentItem
                  || endCornerType != Geometry.CornerType.SharpInside
                      && i == currentOutline.NextSegment(currentItem))) {
            continue;
          }

          Point2F start = point[i].Point.To2D();
          Point2F end = point[outline.NextSegment(i)].Point.To2D();
          double bulge = point[i].Bulge;
          // width of cut if this is the closest line
          double tangentRadius;
          // compute radius to straight line
          if (Math.Abs(bulge) < DBL_EPSILON) {
            tangentRadius = Geometry.RadiusToLineSegment(
                current, normal, start, end);
          } else {  // compute radius to arc
            tangentRadius = Geometry.RadiusToArc(
                current, normal, start, end, bulge);
          }
          if (tangentRadius < radius) {
            radius = tangentRadius;
            selectedPolyline = outline;
            selectedItem = i;
          }
        }
      }
      Point3F toolpathPoint = new Point3F(
          current.X + radius*normal.X,
          current.Y + radius*normal.Y,
          -(radius - ToolTipDiameter.Cached/2)*_cotHalfVAngle);
      //// Verify we didn't overlap adjacent segments when we skipped them.
      //if (startCornerType != Geometry.CornerType.SharpInside
      //    || endCornerType != Geometry.CornerType.SharpInside) {
      //  Vector2F nearestNormal = new Vector2F();
      //  int nearestSegment = currentItem;
      //  Point3F nearestPoint = currentOutline.GetNearestPoint(
      //      toolpathPoint.To2D(), ref nearestNormal, ref nearestSegment);
      //  double nearestDistance = Point2F.Distance(toolpathPoint.To2D(),
      //                                            nearestPoint.To2D());
      //  if (nearestDistance < radius) {
      //    //if (startCornerType == Geometry.CornerType.SmoothInside
      //    //    && nearestSegment == currentOutline.PrevSegment(currentItem)) {
      //    //  // TODO: What?
      //    //} else if (endCornerType == Geometry.CornerType.SmoothInside
      //    //    && nearestSegment == currentOutline.NextSegment(currentItem)) {
      //    //  // TODO: What?
      //    //} else {
      //    _log.log(Math.Abs(nearestDistance - radius) > 1e-6 ? WARNING : TRACE,
      //             "Bad point at {0}, distance was {1} but should have been "
      //             + "{2}, adjusting.", current, radius, nearestDistance);
      //      radius = nearestDistance;
      //      toolpathPoint = new Point3F(
      //          current.X + radius*normal.X,
      //          current.Y + radius*normal.Y,
      //          -(radius - ToolTipDiameter.Cached/2)*_cotHalfVAngle);
      //    //}
      //  }
      //}
      // TODO: Refine this to use arc/beziers?
      // TODO: Adjust for max depth
      if (toolpath.Points.Count >= 1
          && Point3F.Distance(toolpath.LastPoint,
                              toolpathPoint) < DBL_EPSILON) {
        // duplicate point, ignore it
        _log.Log(TRACE, "  Duplicate point at {0}, ignoring", toolpathPoint);
      } else {
        if (toolpath.Points.Count >= 2
            && Geometry.PointBetween(
                toolpath.Points[toolpath.Points.Count - 2].Point,
                toolpath.Points[toolpath.Points.Count - 1].Point,
                toolpathPoint,
                1e-3*PathIncrement.Cached)) {
          // Consolidate co-linear points
          _log.Log(TRACE, "  Removing point at {0} between {1} and {2}",
                    toolpath.Points[toolpath.Points.Count - 1].Point,
                    toolpath.Points[toolpath.Points.Count - 2].Point,
                    toolpathPoint);
          toolpath.Points.RemoveAt(toolpath.Points.Count - 1);
        }
        toolpath.Add(toolpathPoint);
      }
      return radius;
    }
  }
}
