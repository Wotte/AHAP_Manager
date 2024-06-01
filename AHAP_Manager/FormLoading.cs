using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AHAP_Manager
{
    public partial class FormLoading : Form
    {
        public FormLoading()
        {
            InitializeComponent();
        }

        private void FormLoading_Load(object sender, EventArgs e)
        {

        }
        private delegate void CloseDelegate();
        private delegate void IncreaseDelegate();

        private static FormLoading? splashForm;
        private static ProgressBar? progressBar;
        private static Thread? thread;
        static public void ShowSplashScreen(Point parentPos, Size parentSize)
        {
            // Make sure it is only launched once.    
            if (splashForm != null) return;
            splashForm = new FormLoading();
            splashForm.Location = new Point(parentPos.X+ parentSize.Width/2-splashForm.Size.Width/2,
                                            parentPos.Y + parentSize.Height / 2 - splashForm.Size.Height/2);
            /*progressBar = new ProgressBar();
            progressBar.Value = 0;
            progressBar.Visible = true;
            progressBar.SetBounds(0, splashForm.Height - 30,splashForm.Width, 30);
            splashForm.Controls.Add(progressBar);*/
            thread = new Thread(new ThreadStart(FormLoading.ShowForm));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            
        }

        static private void ShowForm()
        {
            if (splashForm != null) Application.Run(splashForm);
        }
        //public bool isClosing;
        static public void CloseForm()
        {
            //if(splashForm!=null) splashForm.isClosing = true;
            splashForm?.Invoke(new CloseDelegate(FormLoading.CloseFormInternal));
        }

        static private void CloseFormInternal()
        {
            if (splashForm != null)
            {

                splashForm.Close();
                splashForm = null;
            };
        }
    }
}
