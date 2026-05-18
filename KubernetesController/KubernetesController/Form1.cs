using KubernetesController.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KubernetesController
{
    public partial class Form1 : Form
    {
        private KubernetesApiClient api;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /// MUDAR IP AQUI!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            api = new KubernetesApiClient("http://IP_DO_MASTER_K3S:8001");
        }
    }
}
