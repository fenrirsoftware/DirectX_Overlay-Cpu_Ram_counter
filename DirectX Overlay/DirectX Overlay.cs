using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;
using D3D = Microsoft.DirectX.Direct3D;
using System.Management;
using System.Diagnostics;
using System.Linq;

namespace DirectX_Overlay
{
    
    // Yürü; hâlâ ne diye oyunda, oynaþtasýn?
   //Fâtih'in Ýstanbul'u fethettiði yaþtasýn!
    



    public partial class TransparentBorderlessForm : Form
    {

        private Margins marg;
        public PresentParameters presentParams;
        public Texture texture;

        private static D3D.Font font;
        public static D3D.Line drawBoxLine;
        public static D3D.Line drawLine;
        public static D3D.Line drawCircleLine;
        public static D3D.Line drawFilledBoxLine;
        public static D3D.Line drawTriLine;

        //transparent alanýn sýnýrlarýný belirleme
        internal struct Margins
        {
            public int Left, Right, Top, Bottom;
        }

        //gerekli dllleri ekleme
        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        Color red = Color.FromArgb(100, 255, 0, 0);
        Color green = Color.FromArgb(100, 0, 255, 0);
        Color blue = Color.FromArgb(100, 0, 0, 255);

        float CenterX = 0.0f;
        float CenterY = 0.0f;

        int CenterrX = 0;
        int CenterrY = 0;

        [DllImport("dwmapi.dll")]
        static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMargins);

        private Device device = null;
        public TransparentBorderlessForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            timer1.Start();
            //timer sayesinde anlýk olarak cpu ve ramin kullaným oranlarýný döndüren fonksiyonlarý tetikliyorum.

            SetWindowLong(this.Handle, GWL_EXSTYLE,
        (IntPtr)(GetWindowLong(this.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED ^ WS_EX_TRANSPARENT));

            //aktif pencerenin renk deðerleriyle oynuyorum 
            SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);

            //DirectX ekliyorum
            //directxi baþlatýyorum, bu baþlatma iþlemi yalnýzca 1 defa yapýlmalýdýr. aksi takdirde çalýþmaz
           
            PresentParameters presentParameters = new PresentParameters();
            presentParameters.Windowed = true;
            presentParameters.SwapEffect = SwapEffect.Discard;
            presentParameters.BackBufferFormat = Format.A8R8G8B8;


            this.device = new Device(0, DeviceType.Hardware, this.Handle,
            CreateFlags.HardwareVertexProcessing, presentParameters);


            drawLine = new D3D.Line(this.device);
            drawBoxLine = new D3D.Line(this.device);
            drawCircleLine = new D3D.Line(this.device);
            drawFilledBoxLine = new D3D.Line(this.device);
            drawTriLine = new D3D.Line(this.device);

            CenterX = (float)this.ClientSize.Width / 2;
            CenterY = (float)this.ClientSize.Height / 2;

            CenterrX = this.ClientSize.Width / 2;
            CenterrY = this.ClientSize.Height / 2;

            font = new D3D.Font(device, new System.Drawing.Font("Fixedsys Regular", 15, FontStyle.Bold));

            Thread dx = new Thread(new ThreadStart(this.dxThread));
            dx.IsBackground = true;
            dx.Start();








        }

        //kodun directx kýsýmlarý hakkýnda hiç ama hiç türkçe kaynak ve dokumantasyon olmadýðý için
        //burasý hakkýnda pek bilgi sahibi deðilim.
        private void dxThread()
        {
            while (true)
            {
                
                device.Clear(ClearFlags.Target, Color.FromArgb(0, 0, 0, 0), 1.0f, 0);
                device.RenderState.ZBufferEnable = false;
                device.RenderState.Lighting = false;
                device.RenderState.CullMode = Cull.None;
                device.Transform.Projection = Matrix.OrthoOffCenterLH(0, this.Width, this.Height, 0, 0, 1);

                // ekrana kare üçgen crosshair vs çizmek istiyorsanýz aþaðýdaki fonksiyonlarý burada tetikleyerek yapabilirsiniz.
                device.BeginScene();

               //ayný þekilde drawfont fonksiyonunu tetikleyerek yazý yazdýrabilirsiniz.




                device.EndScene();
                device.Present();
            }
        }


        static int Ramdeger;
        //deðerleri fonksiyon dýþýna çýkarýyorum bunlar sayesinde
        static int Cpudeger;
        public static void Rc()
            //ram ve cpu deðerlerini anlýk olarak çeken fonksiyon
        {


            //valla burda neden sql sorgusu attýðýmýza dair en ufak bir fikrim yok. 
            //bulduðum tüm yöntemler bu þekildeydi
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();



            var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            //hesap kitap

            percent = Math.Round(percent, 0); //çevirme iþlemleri round da virgülden sonrasýný silme
            Ramdeger = (int)percent;



            //sql sorgusu ama CPU için 
            ObjectQuery objQuery = new ObjectQuery("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
            ManagementObjectSearcher mngObjSearch = new ManagementObjectSearcher(objQuery);
            ManagementObjectCollection mngObjColl = mngObjSearch.Get();

            if (mngObjColl.Count > 0)
            {
                foreach (ManagementObject mngObject in mngObjColl)
                {

                    uint cpu_usage = 100 - Convert.ToUInt32(mngObject["PercentIdleTime"]);
                    Cpudeger = (int)cpu_usage;
                   
                    
                }
            }
        }




        protected override void OnPaint(PaintEventArgs e)
        {
            marg.Left = 0;
            marg.Top = 0;
            marg.Right = this.Width;
            marg.Bottom = this.Height;

            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
        }

        // ekrana yazý yazma fonksiyonu
        public static void DrawFont(string text, Point position, Color color)
        {
             
        }

        // bununla da ekrana çizik mizik atýyosun ya iþte 
        public static void DrawLine(float x1, float y1, float x2, float y2, float w, Color Color)
        {
            drawLine.Width = w;
            drawLine.Antialias = false;
            drawLine.GlLines = true;

            Vector2[] vertices = 
            {
                new Vector2(x1, y1),
                new Vector2(x2, y2)
            };

            drawLine.Begin();
            drawLine.Draw(vertices, Color.ToArgb());           
            drawLine.End();
        }

        // içi dolu kutu yapma fonk.
        public static void DrawFilledBox(float x, float y, float w, float h, Color Color)
        {
            Vector2[] vLine = new Vector2[2];

            drawFilledBoxLine.Width = w;
            drawFilledBoxLine.GlLines = true;
            drawFilledBoxLine.Antialias = false;

            vLine[0].X = x + w / 2;
            vLine[0].Y = y;
            vLine[1].X = x + w / 2;
            vLine[1].Y = y + h;

            drawFilledBoxLine.Begin();
            drawFilledBoxLine.Draw(vLine, Color.ToArgb());
            drawFilledBoxLine.End();

         
            /* DrawFilledBox(x , y, w, h, color); */
        }

        // nasýl da mesajlar kýsalýyo dimi :D
        //crosshair yapma
        public static void DrawCrosshairBox(float x, float y, float w, float h, float px, Color color)
        {
            DrawFilledBox(x, y + h, w, px, color);
            DrawFilledBox(x - px, y, px, h, color);
            DrawFilledBox(x, y - px, w, px, color);
            DrawFilledBox(x + w, y, px, h, color);

            /*              Example             */
            /* DrawCrosshairBox(x , y, w, h, px, color); */
        }

        // 2 boyutlu kare çizme
        public static void DrawBox(float x, float y, float w, float h, Color color)
        {
            Vector2[] vertices = 
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + w, y + h),
                new Vector2(x, y + h),
                new Vector2(x, y)
            };
            drawBoxLine.Begin();
            drawBoxLine.Draw(vertices, color);
            drawBoxLine.End();

            /* DrawBox(x , y, w, h, color); */
        }
      
        // yuvarlak çizme
        public static void DrawCircle(float x, float y, float radius, Color color)
        {
            float PI = 3.14159265f;  //pi 3 alýyoz hocam dimi 
            double t = 0; ;

            for (t = 0.0; t <= PI * 2; t += 0.1)
            {
                x = (float)(x - radius * Math.Cos(t));
                y = (float)(y - radius * Math.Sin(t));
                DrawPoint(x, y, color);
            }

            /* DrawCircle(x Axis, y Axis, radius, Color); */
        }

        // nokta atýyorsun bununla 
        public static void DrawPoint(float x, float y, Color color)
        {
            DrawFilledBox(x, y, 1, 1, color);

            
            /* DrawPoint(x Axis, y Axis, Color */
        }

        // kusursuz tam böyle harika çember çiziyo 
        private void DrawCircle(int X, int Y, int radius, int numSides, Color color)
        {
            Vector2[] Line = new Vector2[100];

            float Step = (float)(Math.PI * 2.0 / numSides);
            int Count = 0;
            for (float a = 0; a < Math.PI * 2.0; a += Step)
            {
                float X1 = (float)(radius * Math.Cos(a) + X);
                float Y1 = (float)(radius * Math.Sin(a) + Y);
                float X2 = (float)(radius * Math.Cos(a + Step) + X);
                float Y2 = (float)(radius * Math.Sin(a + Step) + Y);

                Line[Count].X = X1;
                Line[Count].Y = Y1;
                Line[Count + 1].X = X2;
                Line[Count + 1].Y = Y2;
                Count += 2;
            }
            drawCircleLine.Begin();
            drawCircleLine.Draw(Line, color);
            drawCircleLine.End();

            /* DrawCircle(x Axis, y Axis, radius, numOfSides, color); */
        }

        // üçgen çizme
        public static void DrawTriangle(int x, int y, int w, int h, Color c)
        {

            Vector2[] vertices =
            {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + (w / 2), y - h),
                new Vector2(x, y)
            };
            drawBoxLine.Begin();
            drawBoxLine.Draw(vertices, c);
            drawBoxLine.End();

        
            /* Triangle(x Axis, y Axis, base length, height, color); */
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
              Rc();
            label1.Text = Cpudeger.ToString();
            label2.Text = Ramdeger.ToString();

        }
    }
}

