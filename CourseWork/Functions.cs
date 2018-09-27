using System;
using System.Drawing;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swcommands;
using SolidWorks.Interop.swconst;
using System.Runtime.InteropServices;

namespace CourseWork
{
    public partial class Form1 : Form
    {
        SldWorks sw;
        ModelDoc2 Model;
        AssemblyDoc swAssembly;

        string ApplicationPath = Application.StartupPath + "\\";
        static double mm = 1.0 / 1000.0;


        double pi = 3.14159265359;
        double PaperWidth = 0;
        double PaperHeight = 0;
        int ActiveViewID = -1;
        int InsideSketch = 0;
        int ImageID = 0;


        string SourcePoint = "source_point";
        string last = "last_segment";

        Feature[] Sketches = new Feature[100]; int iSketches = 0; 
        Dimension[] Dimensions = new Dimension[100]; int iDimensions = 0;
        SketchSegment[] Segments = new SketchSegment[100]; int iSegments = 0;

        public class Vector
        {
            public double x, y;
            public Vector() { x = 0; y = 0; }
            public Vector(double X, double Y) { x = X; y = Y; }
            public static bool operator == (Vector a, Vector b) { return a.x == b.x && a.y == b.y; }
            public static bool operator != (Vector a, Vector b) { return a.x != b.x && a.y != b.y; }
            public static Vector operator ~(Vector p) { return new Vector(p.x * mm, p.y * mm); }
            public static Vector operator +(Vector a, Vector b) { return new Vector(a.x + b.x, a.y + b.y); }
        };
        Vector P(double x, double y) { return new Vector(x, y); }


        Vector LastPoint = new Vector(-9999, -9999);


        int abs(int n)
        {
            return n >= 0 ? n : -n;
        }
        double abs(double n)
        {
            return n >= 0 ? n : -n;
        }



        void StartSolidWorks(bool visible = true)
        {
            sw = new SldWorks();
            sw.Visible = visible;
        }


        Feature Last(int back = 0) { return Model.FeatureByPositionReverse(abs(back)); }
        void LastView()
        {
            if (ActiveViewID == 5) Model.ShowNamedView2("*Сверху", 5);
            if (ActiveViewID == 1) Model.ShowNamedView2("*Спереди", 1);
            if (ActiveViewID == 4) Model.ShowNamedView2("*Справа", 4);
        }
        void name(string str) { Last().Select(true); Model.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, str); }
        void Clear() { Model.ClearSelection2(true); }
        void clear() { Model.ClearSelection2(true); }



        //                                                      PLANES                                                    


        void PlaneTop()
        {
            Last().DeSelect();
            if (ActiveViewID != 5) { Model.ShowNamedView2("*Сверху", 5); ActiveViewID = 5; }
            Model.Extension.SelectByID2("Сверху", "PLANE", 0, 0, 0, true, 0, null, 0);
        }
        void PlaneFront()
        {
            Last().DeSelect();
            if (ActiveViewID != 1) { Model.ShowNamedView2("*Спереди", 1); ActiveViewID = 1; }
            Model.Extension.SelectByID2("Спереди", "PLANE", 0, 0, 0, true, 0, null, 0);
        }
        void PlaneRight()
        {
            Last().DeSelect();
            if (ActiveViewID != 4) { Model.ShowNamedView2("*Справа", 4); ActiveViewID = 4; }
            Model.Extension.SelectByID2("Справа", "PLANE", 0, 0, 0, true, 0, null, 0);
        }
        Feature PlaneTop(double dist)
        {
            PlaneTop();
            Feature plane = Model.CreatePlaneAtOffset3(abs(dist) * mm, dist < 0 ? true : false, true); Clear(); plane.Select(true);
            return Last();
        }
        Feature PlaneFront(double dist)
        {
            PlaneFront();
            Feature plane = Model.CreatePlaneAtOffset3(abs(dist) * mm, dist < 0 ? true : false, true); Clear(); plane.Select(true);
            return Last();
        }
        Feature PlaneRight(double dist)
        {
            PlaneRight();
            Feature plane = Model.CreatePlaneAtOffset3(abs(dist) * mm, dist < 0 ? true : false, true); Clear(); plane.Select(true);
            return Last();
        }


        //                                                      SKETCH


        void ResetAll() { iSketches = 0; iSegments = 0; }
        void Sketch()
        {
            Model.SketchManager.InsertSketch(true);
            InsideSketch = 1 - InsideSketch;
            if (InsideSketch == 1)
            {
                Sketches[iSketches++] = Last(); //NameSketch = Last().Name;
                iSegments = 0;
            }
            if (InsideSketch == 0)
            {
                LastPoint = P(-9999, -9999);
            }
        }


        void Paper(double width, double height, Vector shift)
        {
            double f = 0 * mm;
            Vector min = ~P(-width / 2, -height / 2) + ~shift;
            Vector max = ~P(width / 2, height / 2) + ~shift;
            Model.ViewZoomTo2(min.x - f, min.y - f, 0, max.x + f, max.y + f, 0);
        }
        void Paper(double width, double height = 0)
        {
            PaperWidth = width; PaperHeight = height;
            Paper(width, height, P(0, 0));
        }
        void Paper(double width, Vector shift)
        {
            PaperWidth = width;
            Paper(width, PaperHeight, shift);
        }
        void Paper(Vector shift)
        {
            Paper(PaperWidth, PaperHeight, shift);
        }
        void Paper()
        {
            Paper(PaperWidth, PaperHeight);
        }


        SketchPoint Point3D(float x, float y, float z = 0)
        {
            PlaneFront(z);
            Sketch(); SketchPoint skPoint = Model.SketchManager.CreatePoint(x * mm, y * mm, 0); Sketch();
            return skPoint;
        }

        SketchSegment Line(SketchPoint a, SketchPoint b)
        {
            clear();
            SketchSegment segment = Segments[iSegments++] = Model.SketchManager.CreateLine(a.X, a.Y, 0, b.X, b.Y, 0);
            clear(); a.Select(true); select(iSegments - 1, "a"); merge();
            clear(); b.Select(true); select(iSegments - 1, "b"); merge();
            return segment;
        }
        SketchSegment Line(SketchPoint p, double x, double y)
        {
            clear();
            Vector b = ~P(x, y);
            SketchSegment segment = Segments[iSegments++] = Model.SketchManager.CreateLine(p.X, p.Y, 0, b.x, b.y, 0);
            clear(); p.Select(true); select(iSegments - 1, "a"); merge();
            LastPoint = P(x, y);
            return segment;
        }
        SketchSegment Line(double x, double y, double xx, double yy)
        {
            clear();
            Vector a = ~P(x, y), b = ~P(xx, yy);
            SketchSegment segment = Segments[iSegments++] = Model.SketchManager.CreateLine(a.x, a.y, 0, b.x, b.y, 0);
            if (P(x,y) == LastPoint) { clear(); select(iSegments - 2, "b"); select(iSegments - 1, "a"); merge(); }
            LastPoint = P(xx, yy);
            return segment;
        }
        SketchSegment axis(double x, double y, double xx, double yy)
        {
            clear();
            Vector a = ~P(x, y), b = ~P(xx, yy);
            SketchSegment segment = Segments[iSegments++] = Model.SketchManager.CreateCenterLine(a.x, a.y, 0, b.x, b.y, 0);
            if (P(x, y) == LastPoint) { clear(); select(iSegments - 2, "b"); select(iSegments - 1, "a"); merge(); }
            LastPoint = P(xx, yy);
            return segment;
        }
        SketchSegment point(double x, double y)
        {
            return Line(LastPoint.x, LastPoint.y, x, y);
        }
        void Fillet(SketchSegment line1, SketchSegment line2, double radius)
        {
            clear(); line1.Select(true); line2.Select(true);
            SketchSegment last_seg = Segments[iSegments-1];
            SketchSegment fillet = Model.SketchManager.CreateFillet(radius * mm, 1);
            Segments[iSegments - 1] = fillet; Segments[iSegments++] = last_seg;
        }
        void Arc(SketchPoint start, SketchPoint end)
        {
            Segments[iSegments++] = Model.SketchManager.CreateTangentArc(start.X, start.Y, 0, end.X, end.Y, 0, 1);
            clear();    get(iSegments - 1, "b").Select(true);   end.Select(true);  merge(); tangent();
        }

        void Box(Vector a, Vector b)
        {
            clear();
            a = ~a; b = ~b;
            Model.SketchManager.CreateCornerRectangle(a.x, a.y, 0, b.x, b.y, 0);
        }
        void Box(Vector c, double width, double height)
        {
            Box(c, P(width, height) + c);
        }
        void Box(double width, double height)
        {
            Box(P(0, 0), P(width, height));
        }
        //void Box(double width, double height, Vector c)
        //{
        //    Vector a = P(-width/2,-height/2) + c;
        //    Vector b = P( width/2, height/2) + c;
        //    Box(a, b);
        //}
        void Circle(double Xc, double Yc, double R)
        {
            clear(); Model.SketchManager.CreateCircle(Xc * mm, Yc * mm, 0, (Xc + R) * mm, Yc * mm, 0);
        }
        void Circle(double R)
        {
            clear(); Model.SketchManager.CreateCircle(0, 0, 0, R * mm, 0, 0);
        }
        //void Polygon(double Xc, double Yc, double Xp, double Yp, int sides = 5)
        //{
        //    Model.SketchManager.CreatePolygon(Xc * mm, Yc * mm, 0, Xp * mm, Yp * mm, 0, sides, true);
        //}
        void Polygon(double R, int sides = 6)
        {
            clear(); Model.SketchManager.CreatePolygon(0, 0, 0, 0, R * mm, 0, sides, true); Select(P(-R, -R), P(R, R));
        }


        //                                                  Select/Get

        void Select(Vector min, Vector max)
        {
            min = ~min; max = ~max; Model.Extension.SketchBoxSelect(min.x, min.y, 0, max.x, max.y, 0);
        }
        void Select(int index_sketch)
        {
            Sketches[index_sketch].Select(true);
        }
        void Select(bool type = false)
        {
            if (type == false) Clear(); else Select(P(-1000, -1000), P(1000, 1000));
        }
        void select(int index)
        {
            if (index < 0) index = (iSegments - 1) - abs(index);
            Segments[index].Select(true);
        }
        void select(int index, string point)
        {
            if (index < 0) index = (iSegments - 1) - abs(index);
            SketchLine line = (SketchLine)Segments[index];
            if (point == "a" || point == "A") ((SketchPoint)line.GetStartPoint2()).Select(true);
            if (point == "b" || point == "B") ((SketchPoint)line.GetEndPoint2()).Select(true);
        }
        void select(int start, int end, bool ByMark = false, int mark = 0)
        {
            if (!ByMark)
            {
                for (int i = start; i <= end; i++) Segments[i].Select(true);
            }
            else
            {
                for (int i = start; i <= end; i++) Segments[i].SelectByMark(true, mark);
            }
        }
        void select(bool ByMark = false, int mark = -1)
        {
            if (ByMark)
            {
                if (mark == -1) select(0, iSegments - 1);
                else
                    for (int i = 0; i < iSegments; i++) Segments[i].SelectByMark(true, mark);
            }
            else
            {
                clear();
            }
        }
        void select(string what, string point)
        {
            if (what == "last_segment") select(iSegments - 1, point);
        }
        void select(string what)
        {
            if (what == "source_point") Model.Extension.SelectByID2("Point1@Исходная точка", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            if (what == "last_segment") select(iSegments - 1);
        }

        //SketchPoint get_line(int index, string point)
        //{
        //    SketchLine element = (SketchLine)Segments[index];
        //    if (point == "a" || point == "A") return element.GetStartPoint2();
        //    if (point == "b" || point == "B") return element.GetEndPoint2();
        //    return null;
        //}
        //SketchPoint get_arc(int index, string point)
        //{
        //    SketchArc element = (SketchArc)Segments[index];
        //    if (point == "a" || point == "A") return element.GetStartPoint2();
        //    if (point == "b" || point == "B") return element.GetEndPoint2();
        //    if (point == "c" || point == "C") return element.GetCenterPoint2();
        //    return null;
        //}
        SketchPoint get(int index, string point)
        {
            if (index < 0) index = (iSegments - 1) - abs(index);

            SketchSegment segment = Segments[index];
            if (segment.GetType() == (int)swSketchSegments_e.swSketchARC)
            {
                SketchArc element = (SketchArc)segment;
                if (point == "a" || point == "A") return element.GetStartPoint2();
                if (point == "b" || point == "B") return element.GetEndPoint2();
                if (point == "c" || point == "C") return element.GetCenterPoint2();
            }
            if (segment.GetType() == (int)swSketchSegments_e.swSketchLINE)
            {
                SketchLine element = (SketchLine)segment;
                if (point == "a" || point == "A") return element.GetStartPoint2();
                if (point == "b" || point == "B") return element.GetEndPoint2();
            }
            return null;
        }
        SketchPoint get(string what, string point)
        {
            if (what == "last_segment") return get(iSegments - 1, point);
            return null;
        }
        //Vector getP(int index, string point)
        //{
        //    SketchPoint p = get(index, point);
        //    return P(p.X / mm, p.Y / mm);
        //}

        //                                                    Constrains


        void merge() { Model.SketchAddConstraints("sgMERGEPOINTS"); }
        void middle() { Model.SketchAddConstraints("sgATMIDDLE"); }
        void tangent() { Model.SketchAddConstraints("sgTANGENT"); }
        void vertical() { Model.SketchAddConstraints("sgVERTICAL2D"); }
        void symmetric() { Model.SketchAddConstraints("sgSYMMETRIC"); }
        void horizontal() { Model.SketchAddConstraints("sgHORIZONTAL2D"); }
        void coincident() { Model.SketchAddConstraints("sgCOINCIDENT"); }
        void perpendicular() { Model.SketchAddConstraints("sgPERPENDICULAR"); }
        

        //                                                    Operations              
      

        void Mirror(SketchSegment axis)
        {
            Sketch sketch = (Sketch)Model.GetActiveSketch2();
            
            for (int k = iSegments - 2; k >= 0; k--)
            {
                clear();
                Segments[k].SelectByMark(true, 1);
                axis.SelectByMark(true, 2);
                Model.SketchMirror();
                
                dynamic segments = sketch.GetSketchSegments();
                for (int i = 0; i < segments.Length; i++)
                {
                    bool OK = true;
                    SketchSegment pretender = segments[i];
                    for (int j = 0; j < iSegments; j++) if (pretender.GetName() == Segments[j].GetName()) { OK = false; break; }
                    if (OK) Segments[iSegments++] = pretender;
                }   
            }
        }
        void Rotate(double angle)
        {
            Model.Extension.RotateOrCopy(false, 1, false, 0, 0, 0, 0, 0, 1, angle * pi / 180);
        }
        void setDimension(int index, double value)
        {
            Dimensions[index].SystemValue = value * mm;
        }
        void dimension(double value, string text = "")
        {
            DisplayDimension dispDim = Model.AddDimension2(0, 10 * mm, 10 * mm);

            string name = dispDim.GetNameForSelection();
            dispDim.CenterText = true;

            Dimension Dim = Dimensions[iDimensions++] = Model.Parameter(name);
            if (text != "") Dim.Name = text;
            Dim.SystemValue = value * mm;
        }
        void dimensionV(double value, string text = "")
        {
            DisplayDimension dispDim = Model.AddVerticalDimension2(0, 10 * mm, 10 * mm);
            
            string name = dispDim.GetNameForSelection();
            dispDim.CenterText = true;

            Dimension Dim = Dimensions[iDimensions++] = Model.Parameter(name);
            if (text != "") Dim.Name = text;
            Dim.SystemValue = value * mm;
        }
        void dimensionH(double value, string text = "")
        {
            DisplayDimension dispDim = Model.AddHorizontalDimension2(0, 10 * mm, 10 * mm);

            string name = dispDim.GetNameForSelection();
            dispDim.CenterText = true;

            Dimension Dim = Dimensions[iDimensions++] = Model.Parameter(name);
            if (text != "") Dim.Name = text;
            Dim.SystemValue = value * mm;
        }
        void dimensionR(double angle, string text = "")
        {
            if (angle < 0) angle = 180 - abs(angle);
            DisplayDimension dispDim = Model.AddDimension2(0, 10 * mm, 10 * mm);

            string name = dispDim.GetNameForSelection();
            dispDim.CenterText = true;

            Dimension Dim = Dimensions[iDimensions++] = Model.Parameter(name);
            if (text != "") Dim.Name = text;
            Dim.SystemValue = angle * pi / 180;
        }

        //nsion.SelectByID2("Point4", "SKETCHPOINT", 0, 0.03, 0, true, 0, null, 0);

        //                                                     ELEMENTS                                                    


        Feature Axis(float x, float y, float z, float xx, float yy, float zz)
        {
            SketchPoint one = Point3D(x, y, z), two = Point3D(xx, yy, zz);
            one.Select(true); two.Select(true); Model.InsertAxis2(true); return Last();
        }
        Feature FeatureBoss(double lenght)
        {
            Model.FeatureManager.FeatureExtrusion(true, false, lenght < 0 ? true : false,
                                      0, 0, abs(lenght) * mm, 0, false, false, false, false,
                                      0, 0, false, false, false, false, false, true, true); LastView();
            return Last();
        }
        Feature FeatureBossTwo(int iSketch_1, int iSketch_2)
        {
            Select(); Select(iSketch_1); Select(iSketch_2);
            Model.FeatureManager.InsertProtrusionBlend(false, true, false, 1, 0, 0, 1, 1,
                                                       true, true, false, 0, 0, 0, false, false, true);
            return Last();
        }
        Feature FeatureFaces()
        {
            Model.FeatureManager.InsertProtrusionBlend(false, true, false, 1, 0, 0, 1, 1,
                                                       true, true, false, 0, 0, 0, false, false, true);
            return Last();
        }
        Feature FeatureRevolve(Feature axis, double angle)
        {
            axis.SelectByMark(true, 16);
            Model.FeatureManager.FeatureRevolve(angle, false, 0, 0, 0, true, true, true);
            return Last();
        }

        Feature FeatureCut(double lenght)
        {
            Model.FeatureManager.FeatureCut(true, false, lenght < 0 ? false : true,
                                    0, 0, abs(lenght) * mm, 0, false, false, false, false,
                                    0, 0, false, false, false, false, false, true, true);
            return Last();
        }      
        Feature FeatureCutTwo(int iSketch_1, int iSketch_2)
        {
            Select(); Select(iSketch_1); Select(iSketch_2);
            Model.FeatureManager.InsertCutBlend(false, true, false, 1, 0, 0, false, 0, 0, 0, true, true);
            return Last();
        }
        Feature FeatureCutTwo()
        {
            Clear(); Last(2).Select(false); Last().Select(true);
            Model.FeatureManager.InsertCutBlend(false, true, false, 1, 0, 0, false, 0, 0, 0, true, true);
            return Last();
        }
        Feature FeatureFacesCut()
        {
            Model.FeatureManager.InsertCutBlend(false, true, false, 1, 0, 0, false, 0, 0, 0, true, true);
            return Last();
        }
        Feature FeatureRevolveCut(Feature axis, double angle)
        {
            axis.SelectByMark(true, 16);
            Model.FeatureManager.FeatureRevolveCut(angle, false, 0, 0, 0, true, true);
            return Last();
        }
        Feature PathCut(Feature prof, Feature path)
        {
            Last().DeSelect(); prof.Select(true); path.Select(true);
            Model.FeatureManager.InsertCutSwept4(false, true, 0, false, false, 0, 0, false, 0, 0, 0, 0,
                                                 true, true, 0, true, true, true, false);
            return Last();
        }
        void CircularFeature(Feature obj, Feature axis, int count)
        {
            obj.Select2(false, 4); axis.Select2(true, 1);
            Model.FeatureManager.FeatureCircularPattern4(count, 2 * pi, false, "NULL", false, true, false
                /*,false, false, false, 1, pi / 12, "NULL", false*/);
        }
        void MirrorFeature(Feature obj, Feature plane)
        {
            Clear();
            obj.SelectByMark(false, 1); plane.SelectByMark(true, 2);
            Model.FeatureManager.InsertMirrorFeature(false, false, false, false);
        }


        //                                                      VIEW    


        void Fit()
        {
            Model.ShowNamedView2("*Изометрия", 7);
            Model.ViewZoomtofit2();
        }
        void Rebuild() { Model.EditRebuild3(); }
        //activeModelView.DisplayMode = ((int)(swViewDisplayMode_e.swViewDisplayMode_ShadedWithEdges))
        void ViewShaded()           { Model.ViewDisplayShaded(); }
        void ViewFaceted()          { Model.ViewDisplayFaceted(); }
        void ViewWireFrame()        { Model.ViewDisplayWireframe(); }
        void ViewCurvature()        { Model.ViewDisplayCurvature(); }
        void ViewHiddenGreyed()     { Model.ViewDisplayHiddengreyed(); }
        void ViewHiddenRemoved()    { Model.ViewDisplayHiddenremoved(); }
        

        //                                                     VISIBLE


        void DisplayPlanes(bool flag = true)
        { Model.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swDisplayPlanes)), flag); }
        void DisplayAxes(bool flag = true)
        { Model.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swDisplayAxes)), flag); }
        void DisplayOther(bool flag = true)
        {
            Model.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swDisplayOrigins)), flag);
            Model.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swDisplaySketches)), flag);
            Model.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swDisplayReferencePoints2)), flag);
        }


        //                                                     DOCUMENT


        void NewPart()
        {
            sw.NewDocument(@"C:\ProgramData\SOLIDWORKS\SOLIDWORKS 2016\templates\Деталь.prtdot", 0, 0, 0);
            sw.ActivateDoc("");
            Model = sw.ActiveDoc;
            Model.SaveAs3(ApplicationPath + "temp_part" + ".SLDPRT", 0, 0);
            PlaneTop(0); name("Y");
            PlaneRight(0); name("X");
            PlaneFront(0); name("Z");
        }
        void SavePart(string name)
        {
            Model.SaveAs3(ApplicationPath + name + ".SLDPRT", 0, 0);
            sw.CloseDoc(ApplicationPath + name + ".SLDPRT");
        }

        void NewAssembly()
        {
            sw.NewDocument(@"C:\ProgramData\SOLIDWORKS\SOLIDWORKS 2016\templates\Деталь.prtdot", 0, 0, 0);
            sw.ActivateDoc("");
            Model = sw.ActiveDoc;
            Model.SaveAs3(ApplicationPath + "temp_assembly" + ".SLDASM", 0, 0);
            swAssembly = (AssemblyDoc)Model;
        }
        void SaveAssembly(string name)
        {
            Model.SaveAs3(ApplicationPath + name + ".SLDASM", 0, 0);
            sw.CloseDoc(ApplicationPath + name + ".SLDASM");
        }


        //                                                     ASSEMBLY


        string Load(string name, double x = 0, double y = 0, double z = 0)
        {
            sw.OpenDoc6(ApplicationPath + name + ".SLDPRT", 1, 32, "", 0, 0);
            Model = sw.ActivateDoc(name);

            swAssembly.AddComponent(ApplicationPath + name + ".SLDPRT", x*mm, y*mm, z*mm);
            sw.CloseDoc(ApplicationPath + name + ".SLDPRT");
            Model = (ModelDoc2)swAssembly;

            return Last(-1).Name;
        }
        void Mate(string name1, string name2, string plane1, string plane2, double dist = 0)
        {
            Clear(); double d = abs(dist) * mm;
            Model.Extension.SelectByID2(plane1 + "@" + name1 + "@temp_assembly", "PLANE", 0, 0, 0, false, 1, null, 0);
            Model.Extension.SelectByID2(plane2 + "@" + name2 + "@temp_assembly", "PLANE", 0, 0, 0, true, 1, null, 0);
            swAssembly.AddMate5(5, 0, dist < 0 ? true : false, d, d, d, 0, 0, 0, 0, 0, false, false, 0, out int log);
        }
        void mate(string plane1, string plane2, double dist = 0)
        {
            Clear(); double d = abs(dist) * mm;
            Model.Extension.SelectByID2(plane1 + "@" + Last(-2).Name + "@temp_assembly", "PLANE", 0, 0, 0, false, 1, null, 0);
            Model.Extension.SelectByID2(plane2 + "@" + Last(-1).Name + "@temp_assembly", "PLANE", 0, 0, 0, true, 1, null, 0);
            swAssembly.AddMate5(5, 0, dist < 0 ? true : false, d, d, d, 0, 0, 0, 0, 0, false, false, 0, out int log);

        }
        void MateM(string name1, string name2, double dist = 0)
        {
            Mate(name1, name2, "mate", "mate", dist);
        }
        void MateX(string name1, string name2, double dist = 0)
        {
            Mate(name1, name2, "X", "X", dist);
        }
        void MateY(string name1, string name2, double dist = 0)
        {
            Mate(name1, name2, "Y", "Y", dist);
        }
        void MateZ(string name1, string name2, double dist = 0)
        {
            Mate(name1, name2, "Z", "Z", dist);
        }
        void MateX(double dist = 0)
        {
            mate("X", "X", dist);
        }
        void MateY(double dist = 0)
        {
            mate("Y", "Y", dist);
        }
        void MateZ(double dist = 0)
        {
            mate("Z", "Z", dist);
        }


        //                                                       SAVE


        void SaveImage(string path)
        { Model.SaveAs3(path, 0, 0); }


        //                                                       FORM


        void Picture(PictureBox picture)
        {
            if (picture.Image != null)
            {
                picture.Image.Dispose();
                picture.Image = null;
            }
            string id = ImageID.ToString(); ImageID++;
            SaveImage(ApplicationPath + "model_" + id + ".JPG");
            picture.SizeMode = PictureBoxSizeMode.StretchImage;
            picture.Image = Image.FromFile(ApplicationPath + "model_" + id + ".JPG");
        }


        //                                                     FUNCTIONS


        Vector Min(Vector[] p)
        {
            double Xmin = 99999;
            double Ymin = 99999;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i].x < Xmin) Xmin = p[i].x;
                if (p[i].y < Ymin) Ymin = p[i].y;
            }
            return P(Xmin, Ymin);
        }
        Vector Max(Vector[] p)
        {
            double Xmax = -99999;
            double Ymax = -99999;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i].x > Xmax) Xmax = p[i].x;
                if (p[i].y > Ymax) Ymax = p[i].y;
            }
            return P(Xmax, Ymax);
        }

    }
}