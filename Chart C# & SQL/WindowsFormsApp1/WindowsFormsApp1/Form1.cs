using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            



        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MySqlConnection con = new MySqlConnection("host=localhost;user=root;password=xxxxxx;port=3306;database=demo1");
            DataSet ds;
            MySqlDataAdapter da;
            ds = new DataSet();
            con.Open();
            da = new MySqlDataAdapter("select * from table1", con);
            da.Fill(ds);
            chart1.DataSource = ds;
            chart1.Series["Price"].XValueMember = "isim";
            chart1.Series["Price"].YValueMembers = "price";
            chart1.Titles.Add("DEMO GRAPH");
            con.Close();
        }
    }
}
