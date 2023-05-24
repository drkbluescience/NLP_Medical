using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Processes
{

    public partial class Form1 : Form
    {
        public class Datas
        {
            public string Name { get; set; }

            public int Count { get; set; }

        }
        public class Difference
        {
            public string pNo { get; set; }

            public int difference { get; set; }

        }

        public class Oykus
        {
            public string PNo { get; set; }

            public string Tip_Adi { get; set; }

            public string Oyku { get; set; }

        }

        Processes processes = new Processes();
        string[] features = new string[13] { "sigara", "alkol", "Kilo kaybı", "Analjezik", "Anne", "Baba", "Kardeş", "Çocuk", "Eş", "Stresin yakınmalara etkisi", "Kanser Korkusu", "Ailede Reflü Öyküsü", "Ailede Kanser Öyküsü" };
        
        List<Datas> allData = new List<Datas>();
        List<Datas> allData_dset = new List<Datas>();
        List<Datas> allData_oyku = new List<Datas>();

        List<int> counts_data = new List<int>();
        List<int> cluster_1 = new List<int>();
        List<int> cluster_2 = new List<int>();

        public List<Oykus> founded_oyku = new List<Oykus>();
        List<string> oykuNew = new List<string>();

        string path = "Data Source = DESKTOP-QIO9SE6; Initial Catalog = Gastro; Integrated Security = SSPI";
        string[] items_ = new string[2] { "Var", "Yok" };

        public List<Label> labels = new List<Label>();
        public List<TextBox> textboxes = new List<TextBox>();
        public Form1()
        {
            InitializeComponent();
            panel4.Visible = false;
            labels.Add(label18);
            labels.Add(label17);
            labels.Add(label16);
            labels.Add(label19);
            labels.Add(label20);
            labels.Add(label21);

            textboxes.Add(textBox9);
            textboxes.Add(textBox8);
            textboxes.Add(textBox7);

            DateTime dt = DateTime.Now;
            int dakika = dt.Minute;
            int saat = dt.Hour;
            int gun = dt.Day;
            int ay = dt.Month;
            int yil = dt.Year;
            textBox2.Text = "Tarih: " + gun + ".0" + ay + "." + yil + "                  Saat: " + saat + "." + dakika;

            for (int i = 0; i < items_.Length; i++)
            {
                comboBox4.Items.Add(items_[i]);
                comboBox5.Items.Add(items_[i]);
                comboBox1.Items.Add(items_[i]);
            }

            comboBox2.Items.Add("Eski içici");
            comboBox2.Items.Add("İçmiyor");
            comboBox3.Items.Add("Kullanmıyor");
            comboBox3.Items.Add("Sosyal içici");
            comboBox1.Items.Add("Değişmiyor");
            
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //ReadFile();
            //NumberValuesAdd();
            stopwatch.Stop();
            string executionTime = $"Execution Time: {stopwatch.ElapsedMilliseconds} ms";
            MessageBox.Show(executionTime);        
        }

        private void ReadFile()
        {
            string[] file_ = System.IO.File.ReadAllLines(@"AllData.txt"); //Encoding.GetEncoding("iso-8859-9")
            foreach (var item in file_)
            {
                if (item.Split().Length == 2 && Regex.IsMatch(item.Split(' ')[1], @"^\d+$"))
                {
                    allData.Add(new Datas { Count = Int16.Parse(item.Split(' ')[1]), Name = item.Split(' ')[0].ToLower() });
                    counts_data.Add(Int16.Parse(item.Split(' ')[1]));
                }
            }
        }

        private int Distance(int x, int y)
        {
            int dist = Math.Abs(x - y);
            //int rest = (int)Math.Sqrt(dist * dist);
            return dist;
        }
       
        private void Kmeans()
        {
            List<int> cluster_1_local = new List<int>();
            List<int> cluster_2_local = new List<int>();

            var rand = new Random();
            int first = 0;
            int second = 0;

            while (first == second)
            {
                first = rand.Next(counts_data.Count - 1);
                second = rand.Next(counts_data.Count - 1);
            }
            

            int centroid1 = counts_data[first];
            int centroid2 = counts_data[second];

            cluster_1_local.Add(centroid1);
            cluster_2_local.Add(centroid2);

            List<int> indexes = new List<int>();
            for (int i = 0; i < counts_data.Count; i++)
            {
                if (!indexes.Contains(i) && i != first && i != second)
                {
                    int dist1 = Distance(centroid1, counts_data[i]);
                    int dist2 = Distance(centroid2, counts_data[i]);
                    if (dist1 > dist2)
                    {
                        cluster_2_local.Add(counts_data[i]);
                    }
                    else if (dist2 >= dist1)
                    {
                        cluster_1_local.Add(counts_data[i]);
                    }
                    indexes.Add(i);
                }
            }

            Recursive_Kmeans(cluster_1_local, cluster_2_local, 0);
            MessageBox.Show("K-MEANS DONE");
        }

        private bool Recursive_Kmeans(List<int> cluster_1_local, List<int> cluster_2_local, int count)
        {

            bool flag = false;

            if (count == 30)
            {

                cluster_1 = cluster_1_local.ToList();
                cluster_2 = cluster_2_local.ToList();
                return flag;
            }
            else
            {
                List<int> cluster_1_local_ = new List<int>();
                List<int> cluster_2_local_ = new List<int>();

                int sum_1 = 0;
                for (int i = 0; i < cluster_1_local.Count; i++)
                {
                    sum_1 += cluster_1_local[i];
                }

                int sum_2 = 0;
                for (int i = 0; i < cluster_2_local.Count; i++)
                {
                    sum_2 += cluster_2_local[i];
                }

                int centroid_1 = sum_1 / cluster_1_local.Count;
                int centroid_2 = sum_2 / cluster_2_local.Count;

                List<int> indexes = new List<int>();
                for (int i = 0; i < counts_data.Count; i++)
                {
                    if (!indexes.Contains(i))
                    {
                        int dist1 = Distance(centroid_1, counts_data[i]);
                        int dist2 = Distance(centroid_2, counts_data[i]);
                        if (dist1 > dist2)
                        {
                            if (!cluster_2_local_.Contains(counts_data[i]))
                            {
                                cluster_2_local_.Add(counts_data[i]);
                            }
                        }
                        else if (dist2 >= dist1)
                        {
                            if (!cluster_1_local_.Contains(counts_data[i]))
                            {
                                cluster_1_local_.Add(counts_data[i]);
                            }
                        }
                        indexes.Add(i);
                    }
                }
                count++;
                return Recursive_Kmeans(cluster_1_local_, cluster_2_local_, count);
            }

        }
        private void NumberValuesAdd()
        {
            Kmeans();
            if (cluster_1.Count > 0 && cluster_2.Count > 0)
            {
                int sum_1 = 0;
                int sum_2 = 0;
                for (int i = 0; i < cluster_1.Count; i++)
                {
                    sum_1 += cluster_1[i];
                }

                for (int i = 0; i < cluster_2.Count; i++)
                {
                    sum_2 += cluster_2[i];
                }

                if (sum_1 > sum_2)
                {
                    for (int i = 0; i < allData.Count; i++)
                    {
                        if (cluster_2.Contains(allData[i].Count))
                        {
                            allData_dset.Add(new Datas { Name = allData[i].Name, Count = allData[i].Count });
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < allData.Count; i++)
                    {
                        if (cluster_1.Contains(allData[i].Count))
                        {
                            allData_dset.Add(new Datas { Name = allData[i].Name, Count = allData[i].Count });
                        }
                    }
                }
                List<string> dset_words = new List<string>();
                for (int i = 0; i < allData_dset.Count; i++)
                {
                    dset_words.Add(allData_dset[i].Name);
                }
                List<string> number_oyku = processes.ExcelRead(dset_words);
                for (int i = 0; i < number_oyku.Count; i++)
                {
                    allData_dset.Add(new Datas { Name = number_oyku[i].Split(' ')[0], Count = Convert.ToInt32(number_oyku[i].Split(' ')[1]) });
                }
                DataSetAdd(allData_dset);
            }
        }
        public void DataSetAdd(List<Datas> dataset_)
        {
           using (StreamWriter file =
           new StreamWriter(@"dataset.txt"))
            {
                for (int i = 0; i < dataset_.Count; i++)
                {
                    file.WriteLine(dataset_[i].Name);
                }
            }
        }

        public List<string> ParseDictionary(string oyku)
        {
            List<string> dictionary = new List<string>();
            string[] splitDictionary = oyku.Split(' ');
            for (int i = 0; i < splitDictionary.Length; i++)
            {
                string value = splitDictionary[i];
                value = value.Trim();
                value = value.Replace("\n", "");
                if (!string.IsNullOrEmpty(value) && !value.Any(char.IsDigit))
                {
                    dictionary.Add(TrimPunctuation(value));
                }
            }
            return dictionary;
        }
        string TrimPunctuation(string value)
        {
            // Count start punctuation.
            int removeFromStart = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromStart++;
                }
                else
                {
                    break;
                }
            }

            // Count end punctuation.
            int removeFromEnd = 0;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromEnd++;
                }
                else
                {
                    break;
                }
            }
            // No characters were punctuation.
            if (removeFromStart == 0 &&
                removeFromEnd == 0)
            {
                return value;
            }
            // All characters were punctuation.
            if (removeFromStart == value.Length &&
                removeFromEnd == value.Length)
            {
                return "";
            }
            // Substring.
            return value.Substring(removeFromStart,
                value.Length - removeFromEnd - removeFromStart);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.panel3.ClientRectangle, Color.Orange, ButtonBorderStyle.Solid);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.panel3.ClientRectangle, Color.Orange, ButtonBorderStyle.Solid);
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.panel3.ClientRectangle, Color.Orange, ButtonBorderStyle.Solid);
        }
        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                oykuNew.Add(textBox1.Text);

                for (int i = 0; i < features.Length; i++)
                {
                    oykuNew.Add(features[i]);
                }
                if (!String.IsNullOrEmpty(textBox3.Text))
                {
                    oykuNew.Add(textBox3.Text);
                }
                if (!String.IsNullOrEmpty(textBox4.Text))
                {
                    oykuNew.Add(textBox4.Text);
                }
                if (!String.IsNullOrEmpty(textBox5.Text))
                {
                    oykuNew.Add(textBox5.Text);
                }
                if (!String.IsNullOrEmpty(textBox6.Text))
                {
                    oykuNew.Add(textBox6.Text);
                }

                processes.FunctionOykuNew(oykuNew);

                founded_oyku = processes.oykus_found;
                string out_message = String.Empty;

                for (int i = 0; i < founded_oyku.Count; i++)
                {
                    out_message += "-" + founded_oyku[i].PNo + " ";
                }

                string message = "Hasta " + out_message + " nolu hastalarla benzerlik göstermektedir. \nEşleşmeleri görmek ister misiniz ?";
                string title = "Eşleşme";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, title, buttons);
                if (result == DialogResult.No)
                {
                    this.Close();
                }
                else
                {
                    panel1.Visible = false;
                    panel2.Visible = false;
                    panel3.Visible = false;
                    panel4.Visible = true;

                    for (int i = 0; i < founded_oyku.Count; i++)
                    {
                        labels[i].Text = "P.No: " +  founded_oyku[i].PNo;
                        labels[i + 3].Text = "Tanı: " +  founded_oyku[i].Tip_Adi;
                        textboxes[i].Text = founded_oyku[i].Oyku;
                    }
                }
            }
            else
            {
                MessageBox.Show("Hasta öyküsü boş!");
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            panel2.Visible = true;
            panel3.Visible = true;
            panel4.Visible = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                oykuNew.Add("Var");
     
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked == true)
            {
                oykuNew.Add("Yok");
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                oykuNew.Add("Var");
            }

        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
            {
                oykuNew.Add("Yok");
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                oykuNew.Add("Var");
            }

        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
            {
                oykuNew.Add("Yok");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                oykuNew.Add("Var");
            }

        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked == true)
            {
                oykuNew.Add("Yok");
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)
            {
                oykuNew.Add("Var");
            }

        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked == true)
            {
                oykuNew.Add("Yok");
            }
        }

        private void textBox2_ReadOnlyChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                oykuNew.Add(comboBox1.SelectedItem.ToString());
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != -1)
            {
                oykuNew.Add(comboBox4.SelectedItem.ToString());
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex != -1)
            {
                oykuNew.Add(comboBox5.SelectedItem.ToString());
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex != -1)
            {
                oykuNew.Add(comboBox3.SelectedItem.ToString());
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1)
            {
                oykuNew.Add(comboBox2.SelectedItem.ToString());
            }
        }

        private void panel4_Paint_1(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.panel4.ClientRectangle, Color.Orange, ButtonBorderStyle.Solid);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                oykuNew.Add(textBox1.Text);

                for (int i = 0; i < features.Length; i++)
                {
                    oykuNew.Add(features[i]);
                }
                if (!String.IsNullOrEmpty(textBox3.Text))
                {
                    oykuNew.Add(textBox3.Text);
                }
                if (!String.IsNullOrEmpty(textBox4.Text))
                {
                    oykuNew.Add(textBox4.Text);
                }
                if (!String.IsNullOrEmpty(textBox5.Text))
                {
                    oykuNew.Add(textBox5.Text);
                }
                if (!String.IsNullOrEmpty(textBox6.Text))
                {
                    oykuNew.Add(textBox6.Text);
                }

                processes.FunctionOykuNew(oykuNew);

                founded_oyku = processes.oykus_found;
                string out_message = String.Empty;

                for (int i = 0; i < founded_oyku.Count; i++)
                {
                    out_message += "-" + founded_oyku[i].PNo + " ";
                }

                string message = "Hasta " + out_message + " nolu hastalarla benzerlik göstermektedir. \nEşleşmeleri görmek ister misiniz ?";
                string title = "Eşleşme";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, title, buttons);
                if (result == DialogResult.No)
                {
                    this.Close();
                }
                else
                {
                    panel1.Visible = false;
                    panel2.Visible = false;
                    panel3.Visible = false;
                    panel4.Visible = true;

                    for (int i = 0; i < founded_oyku.Count; i++)
                    {
                        labels[i].Text = "P.No: " + founded_oyku[i].PNo;
                        labels[i + 3].Text = "Tanı: " + founded_oyku[i].Tip_Adi;
                        textboxes[i].Text = founded_oyku[i].Oyku;
                    }
                }
            }
            else
            {
                MessageBox.Show("Hasta öyküsü boş!");
            }
        }
    }
}
