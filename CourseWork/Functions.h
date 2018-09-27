using System;
using System.Drawing;
using System.Windows.Forms;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swcommands;
using SolidWorks.Interop.swconst;
using System.Runtime.InteropServices;



namespace Course
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SldWorks sw;
        PartDoc Part;
        ModelDoc2 Model;

        double mm = 1.0 / 1000.0;


        void StartSolidWorks()
        {
            sw = new SldWorks();
            sw.Visible = true;

            Part = sw.NewDocument("C:\\ProgramData\\SolidWorks\\SOLIDWORKS 2017\\templates\\gost-part.prtdot", 0, 0, 0);
            sw.ActivateDoc("My_detal");
            Model = sw.ActiveDoc;
        }

        void Rename(string name) { Model.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, name); }

        void PlaneTop() { Model.SelectByID("Сверху", "PLANE", 0, 0, 0); }
        void PlaneFront() { Model.SelectByID("Спереди", "PLANE", 0, 0, 0); }
        void PlaneRight() { Model.SelectByID("Справа", "PLANE", 0, 0, 0); }
        void PlaneTopOffset(double dist)
        {
            PlaneTop();
            Feature plane = Model.CreatePlaneAtOffset3(dist*mm, false, true); Model.ClearSelection();
            plane.Select(true);
        }
        void PlaneFrontOffset(double dist)
        {
            PlaneFront();
            Feature plane = Model.CreatePlaneAtOffset3(dist*mm, false, true); Model.ClearSelection();
            plane.Select(true);
        }
        void PlaneRightOffset(double dist)
        {
            PlaneRight();
            Feature plane = Model.CreatePlaneAtOffset3(dist*mm, false, true); Model.ClearSelection();
            plane.Select(true);
        }



        void InsertSketch() { Model.SketchManager.InsertSketch(true); }
        void StartSketch(string name = null)    { InsertSketch(); }
        void EndSketch(string name = null)      { InsertSketch(); if(name != null) Rename(name); }



        void CreatePolygon(double Xc, double Yc, double Xp, double Yp, int sides = 5)
        {
            Model.SketchManager.CreatePolygon(Xc*mm, Yc*mm, 0, Xp*mm, Yp*mm, 0, sides, true);
        }
        void CreateCircle(double Xc, double Yc, double R)
        {
            Model.SketchManager.CreateCircle(Xc*mm, Yc*mm, 0, (Xc+R)*mm, Yc*mm, 0);
        }

        void FeatureBoss(double lenght, bool inverse = false)
        {
            Model.FeatureBoss2(true, false, inverse,
                                 0, 0, lenght*mm, 0, true, false, true, false,
                                 0, 0, false, false, false, false);
        }
        void FeatureCut(double lenght, bool inverse = true)
        {
            Model.FeatureManager.FeatureCut(true, false, inverse,
                                 0, 0, lenght*mm, 0, false, false, false, false,
                                 0, 0, false, false, false, false, false, true, true);
        }
        void FeatureBossTwo(string sketch1, string sketch2)
        {
            Model.Extension.SelectByID2(sketch1, "SKETCH", 1, 1, 1, false, 1, null, 0);
            Model.Extension.SelectByID2(sketch2, "SKETCH", 1, 1, 1, true, 1, null, 0);
            Model.FeatureManager.InsertProtrusionBlend(false, true, false, 1, 0, 0, 1, 1, true, true, false, 0, 0, 0, false, false, true);
            //Model.FeatureManager.InsertProtrusionBlend(false, true, false, 1, 0, 0, 1, 1, true, true, false, 0, 0, 0, true, false, true);
        }
        void FeatureCutTwo(string sketch1, string sketch2)
        {
            Model.Extension.SelectByID2(sketch1, "SKETCH", 1, 1, 1, false, 1, null, 0);
            Model.Extension.SelectByID2(sketch2, "SKETCH", 1, 1, 1, true, 1, null, 0);
            Model.FeatureManager.InsertCutBlend(false, true, false, 1, 0, 0, false, 0, 0, 0, true, true);
        }


        void ViewIsometric()
        {
            Model.ShowNamedView2("*Изометрия", 7);
            Model.ViewZoomtofit2();
        }



        void DisplayPlanes(bool flag = true)
        { Model.SetUserPreferenceToggle(((int)(swUserPreferenceToggle_e.swDisplayPlanes)), flag); }



        void SaveImage(string path)
        { Model.SaveAs3(path, 0, 0); }



        void Picture()
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            SaveImage("C:\\Files\\Деталь666.JPG");
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = Image.FromFile("C:\\Files\\Деталь666.JPG");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float h = Convert.ToSingle(textBox1.Text);
            float d = Convert.ToSingle(textBox2.Text);
            float D = Convert.ToSingle(textBox3.Text);
            float d_otv = Convert.ToSingle(textBox4.Text);
            float h_otv = Convert.ToSingle(textBox5.Text);

            StartSolidWorks();

            //PlaneTop();
            //InsertSketch();
            //CreatePolygon(0, 0, 0, -D/2, 3);
            //InsertSketch();
            //FeatureBoss(h);

            //PlaneTop();
            //InsertSketch();
            //CreatePolygon(0, 0, 0, -d/2, 3);
            //InsertSketch();
            //FeatureCut(h);

            //PlaneFrontOffset(D/2);
            //InsertSketch();
            //CreateCircle(0, h_otv, d_otv/2, h_otv);
            //InsertSketch();
            //FeatureCut(D, false);


            //DisplayPlanes(false);

            //Picture();


            PlaneTopOffset(10);
            StartSketch();
            CreateCircle(0, 0, 80);
            EndSketch();
            FeatureBoss(200); Rename("sds");


            PlaneTop();
            StartSketch();
            CreatePolygon(0, 0, 0, 50, 3);
            EndSketch("one");
            

            PlaneTopOffset(100);
            StartSketch();
            CreatePolygon(0, 0, 0, 40, 3);
            EndSketch("two");

            PlaneTopOffset(150);
            StartSketch();
            CreatePolygon(0, 0, 0, 20, 3);
            EndSketch("three");

            FeatureCutTwo("one", "two");    Rename("Cut1");
            FeatureCutTwo("two", "three");     Rename("Boss2");
            ViewIsometric();


        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            sw.ExitApp();
            sw = null;
        }
    }
}


//StartSolidWorks();

//PlaneTop();
//InsertSketch();
//CreatePolygon(0, 0, 0, -50, 3);
//InsertSketch();
//FeatureBoss(120);

//PlaneTop();
//InsertSketch();
//CreatePolygon(0, 0, 0, -30, 3);
//InsertSketch();
//FeatureCut(120);

//PlaneFrontOffset(50);
//InsertSketch();
//CreateCircle(0, 60, 20, 60);
//InsertSketch();
//FeatureCut(100, false);


//DisplayPlanes(false);
//SaveImage("C:\\Files\\Деталь666.JPG");

//Image image = Image.FromFile("C:\\Files\\Деталь666.JPG");
//pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
//pictureBox1.Image = image;