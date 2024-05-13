using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace sequor_fe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            toolStripStatusLabel.Text = "Procurando ordens...";
            Program.PerformGetRequest(cbOrder, toolStripStatusLabel);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Criando...";
            Program.stopwatch.Stop();
            CreateJsonAndSend(); 
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
        private void CreateJsonAndSend()
        {
            // Get the data from the controls
            string email = txtMail.Text;
            string order = cbOrder.SelectedItem as string;
            DateTime productionDate = dtDate.Value;
            decimal quantity = nQuantity.Value;
            string materialCode = cbMaterials.SelectedItem as string;
            decimal cycleTime = Program.stopwatch.ElapsedMilliseconds / 1000;

            materialCode = "bbb";
            // Create a dictionary to store the data
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "order", order },
                { "productionDate", productionDate.ToString("yyyy-MM-dd") },
                { "productionTime", "10:30:00" },
                { "quantity", quantity },
                { "materialCode", materialCode },
                { "cycleTime", cycleTime }
            };

            // Serialize the dictionary into a JSON string
            string json = JsonConvert.SerializeObject(data);
            Program.PostJsonAsync("http://localhost:5270/api/orders/setProduction", json, toolStripStatusLabel);

        }
    private void cbOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.stopwatch = new Stopwatch();
            Stopwatch.StartNew();

            // Cast the SelectedItem to a string and get the selected order
            string selectedOrder = cbOrder.SelectedItem as string;
            if (selectedOrder != null)
            {
                // Iterate over each order in the JSON array
                foreach (var order in Program.localData)
                {
                    // Check if the order number matches the selected order
                    if (order["order"].ToString() == selectedOrder)
                    {
                        // Extract materials array from the selected order data
                        var materialsArray = order["materials"] as JArray;

                        // Populate cbMaterials with materialDescription values
                        cbMaterials.Items.Clear(); // Clear existing items
                        foreach (var material in materialsArray)
                        {
                            string materialDescription = material["materialDescription"].ToString();
                            cbMaterials.Items.Add(materialDescription);
                        }

                        // Exit the loop since we found the selected order
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("No order selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



    }
}
