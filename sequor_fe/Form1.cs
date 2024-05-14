using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;


namespace sequor_fe
{
    public partial class Form1 : Form
    {
        public string currentOrder = "";
        public string currentMaterial = "";
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            toolStripStatusLabel.Text = "Procurando ordens...";
            cbMaterials.SelectedIndexChanged += new EventHandler(cbMaterial_SelectedIndexChanged);
            Program.PerformGetRequest(cbOrder, toolStripStatusLabel, pbProduct);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (currentOrder.Length > 0 && currentMaterial.Length > 0)
            {
                toolStripStatusLabel.Text = "Criando...";
                CreateJsonAndSend();
            }
            else
            {
                toolStripStatusLabel.Text = "Por favor, preencha todos os campos.";
            }
        }

        //Este método usa regex para validar o email.
        public bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        //This method packages the data to send to the remote api
        private void CreateJsonAndSend()
        {
            // Get the data from the controls
            string email = txtMail.Text;
            string order = cbOrder.SelectedItem as string;
            DateTime productionDate = dtDate.Value;
            decimal quantity = nQuantity.Value;
            Program.stopwatch.Stop();
            TimeSpan elapsedTime = Program.stopwatch.Elapsed;
            decimal cycleTime = (decimal)elapsedTime.TotalMilliseconds/1000;

            if (!IsValidEmail(email))
            {
                toolStripStatusLabel.Text = "Por favor, insira um e-mail válido.";
                return;
            }


            // Create a dictionary to store the data
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "order", order },
                { "productionDate", productionDate.ToString("yyyy-MM-dd") },
                { "productionTime", "10:30:00" },
                { "quantity", quantity },
                { "materialCode", currentMaterial},
                { "cycleTime", cycleTime }
            };

            // Serialize the dictionary into a JSON string
            string json = JsonConvert.SerializeObject(data);
            Program.PostJsonAsync("http://localhost:5270/api/orders/SetProduction", json, toolStripStatusLabel, this.Controls);

        }
        private void cbMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMaterial = cbMaterials.SelectedItem as string;
            if (selectedMaterial != null)
            {
                // Iterate over each order in the JSON array
                foreach (var order in Program.localData)
                {
                    // Check if the order number matches the selected order
                    if (order["order"].ToString() == currentOrder)
                    {
                        // Extract materials array from the selected order data
                        var materialsArray = order["materials"] as JArray;

                        // Populate cbMaterials with materialDescription values
                        foreach (var material in materialsArray)
                        {
                            string materialDescription = material["materialDescription"].ToString();
                            if (materialDescription == selectedMaterial)
                            {
                                currentMaterial = material["materialCode"].ToString();
                                break;
                            }
                        }

                    }
                }



            }
        }
    private void cbOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.stopwatch = Stopwatch.StartNew();

            // Cast the SelectedItem to a string and get the selected order
            string selectedOrder = cbOrder.SelectedItem as string;
            if(cbOrder.SelectedIndex == -1)
            {
                currentOrder = "";
                currentMaterial = "";
                return;
            }
            if (selectedOrder != null)
            {
                // Iterate over each order in the JSON array
                foreach (var order in Program.localData)
                {
                    // Check if the order number matches the selected order
                    if (order["order"].ToString() == selectedOrder)
                    {
                        currentOrder = selectedOrder;
                        // Extract materials array from the selected order data
                        var materialsArray = order["materials"] as JArray;

                        // Populate cbMaterials with materialDescription values
                        cbMaterials.Items.Clear(); // Clear existing items
                        foreach (var material in materialsArray)
                        {
                            string materialDescription = material["materialDescription"].ToString();
                            cbMaterials.Items.Add(materialDescription);
                        }

                        string base64Image = (string)order["image"];
                        byte[] imageData = Convert.FromBase64String(base64Image);
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            pbProduct.SizeMode = PictureBoxSizeMode.StretchImage; // Set the SizeMode
                            pbProduct.Image = System.Drawing.Image.FromStream(ms);
                        }


                        // Exit the loop since we found the selected order
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhuma ordem selecionada", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
