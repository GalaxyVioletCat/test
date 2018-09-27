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
           
        public Form1()
        {
            InitializeComponent();
        }

        private void Create(object sender, EventArgs e)
        {
            double Thick = Convert.ToSingle(textBox1.Text);
            double diametr = Convert.ToSingle(textBox2.Text);
            double H_model = Convert.ToSingle(textBox3.Text);
            double D_model = Convert.ToSingle(textBox4.Text);
            double Diametr = Convert.ToSingle(textBox5.Text);
            double Distance = Convert.ToSingle(textBox5.Text);
            double model_percent = Convert.ToSingle(textBox6.Text);
            double angle = 2 * pi * model_percent/100;

            StartSolidWorks();

            NewPart();
            {
                DisplayPlanes(false);

                Feature Ax = Axis(0, 0, 0, 0, 60, 0);

                PlaneFront(0); name("mate");
                Sketch();
                {
                    Paper(150);
                    Line(5, 4, 32, 4);
                    point(32, 0);
                    point(40, 0);
                    Fillet(point(40, Thick - 2), point(8, Thick - 2), 2);
                    point(8, Thick);
                    point(5, Thick);
                    point(5, 4);
                }
                Sketch();
                FeatureRevolve(Ax, angle);

                ResetAll();

                PlaneTop(Thick - 2);
                Sketch();
                {
                    Paper(200);
                    Circle(25, 0, diametr + 2);
                }
                Sketch();
                PlaneTop(4 + (Thick - 2 - 4) / 2);
                Sketch();
                {
                    Paper();
                    Circle(25, 0, diametr);
                }
                Sketch();
                PlaneTop(0);
                Sketch();
                {
                    Paper();
                    Circle(25, 0, diametr);
                }
                Sketch();
                Feature cut1 = FeatureCutTwo(0, 1);
                Feature cut2 = FeatureCutTwo(1, 2);
                CircularFeature(cut1, Ax, 3);
                CircularFeature(cut2, Ax, 3);

                Fit(); Clear(); Picture(pictureBox1);
            }
            SavePart("one");
            NewPart();
            {
                DisplayPlanes(false);

                Feature Ax = Axis(0, 0, 0, 0, 60, 0);

                PlaneFront(0); name("mate");
                Sketch();
                {
                    Paper(P(0, 20));
                    Line(8, 10, 10, 10);
                    point(10, 25);
                    point(8, 25);
                    point(8, 10);
                }
                Sketch();
                FeatureRevolve(Ax, angle);

                PlaneFront();
                Sketch();
                {
                    Paper(P(0, 30));
                    Line(0, 23, 8, 23);
                    point(8, 25);
                    point(14, 25);
                    point(24, 32);
                    Fillet(point(30, 50), point(0, 50), 2);
                    point(0, 23);

                    clear();
                    select(0); select(SourcePoint); dimension(23);
                    clear();
                    select(last); select(SourcePoint); coincident();
                    clear();
                    select(1); dimension(2);
                    clear();
                    select(2); select(6); dimension(H_model);
                    clear();
                    select(-1); dimension(D_model);
                }
                Sketch();
                FeatureRevolve(Ax, angle);

                PlaneTop(25 + H_model);
                Sketch();
                {
                    Paper();
                    Circle(0, 0, 10);
                }
                Sketch();
                PlaneTop(20 + H_model);
                Sketch();
                {
                    Paper();
                    Circle(0, 0, 5);
                }
                Sketch();
                FeatureCutTwo();

                PlaneTop((25 + H_model) - 10);
                Sketch();
                {
                    Paper();
                    Polygon(8); Rotate(pi / 6);
                }
                Sketch();
                FeatureBoss(-8);

                PlaneTop((25 + H_model) - 18);
                Sketch();
                {
                    Paper();
                    Circle(5);
                }
                Sketch();
                FeatureBoss(-(25 + H_model - 18));

                Fit(); Clear(); Picture(pictureBox2);
            }
            SavePart("two");
            NewAssembly();
            {
                DisplayPlanes(false);

                string m1 = Load("one");
                string m2 = Load("two");
                MateX(); MateZ(); MateY(Distance);

                Rebuild();

                Fit(); Clear(); Picture(pictureBox3);
            }
            SaveAssembly("one_ass");
        }

        private void ClosedApp(object sender, FormClosedEventArgs e)
        {
            if (sw != null)
            {
                sw.ExitApp();
                sw = null;
            }
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    double Thick = Convert.ToSingle(textBox1.Text);
        //    double diametr = Convert.ToSingle(textBox2.Text);
        //    double H_model = Convert.ToSingle(textBox3.Text);
        //    double model_percent = Convert.ToSingle(textBox7.Text);
        //    double angle = 2 * pi * model_percent / 100;

        //    setDimension(2, H_model);
        //    Rebuild();
        //}
    }
}



//NewPart();
//{
//    DisplayPlanes(false);

//    Feature Ax = Axis(0, 0, 0, 100, 0, 0);

//    PlaneRight();
//    Sketch();
//    {
//        Paper(500);
//        Circle(80);
//    }
//    Sketch();
//    FeatureBoss(50);

//    ResetAll();                                             //Сбрасывает массив скетчей

//    PlaneRight();
//    Sketch();
//    {
//        Paper(500);

//        Line(1, 1, 5, 5);                                   //0 - line, set random coordinats
//                                                                //Ни одна линия не должна совпадать с осью симметрии иначе ошибка
//        Mirror(axis(0, -10, 0, 10));                        //1 - axis  , 2 - mirrored line above

//        clear();
//        select(1); vertical();
//        clear();
//        select(1); select(SourcePoint); middle();
//        clear();
//        select(0, "a"); select(2, "a"); dimension(4);
//        clear();
//        select(0, "b"); select(2, "b"); dimension(16);
//        clear();
//        select(0); dimension(20);

//        Arc(get(0, "a"), get(2, "a"));

//        Line(get(0, "b"), 0, 20); point(0, 30);
//        Line(get(last, "b"), get(2, "b"));

//        clear();
//        select(-2); vertical();
//        clear();
//        select(-1); horizontal();
//        clear();
//        select(last); vertical();
//        clear();
//        select(-1); select(last, "b"); dimension(10);

//        clear();
//        select(0, "b"); select(SourcePoint); dimensionV(80);
//    }
//    Sketch();
//    Feature cut = FeatureCut(50);
//    CircularFeature(cut, Ax, 23);


//    Fit();
//}
////SavePart("my_part");