using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace sequor_fe
{
    internal static class Program
    {
        public static Stopwatch stopwatch;
        public static JArray localData = null;
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
        public static async Task<string> PostJsonAsync(string url, string json, ToolStripStatusLabel statusStrip)
        {
            try
            {
                HttpClient client = new HttpClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JObject.Parse(responseContent);

                    string description = responseData["description"].ToString();
                    statusStrip.Text = description;
                    return await response.Content.ReadAsStringAsync();

                }
                else
                {
                    string msg = response.ReasonPhrase;
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public static async Task PerformGetRequest(ComboBox cbOrder, ToolStripStatusLabel toolStripStatusLabel)
        {
            try
            {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5270/api/orders/GetOrders");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // Parse the JSON response
                        JObject jsonResponse = JObject.Parse(responseBody);

                        // Get the array of orders
                        JArray ordersArray = (JArray)jsonResponse["orders"];

                        // Clear the ComboBox before adding new items
                        cbOrder.Items.Clear();
                        toolStripStatusLabel.Text = "Pronto";

                        // Loop through each order in the array and add the order number to the ComboBox
                        foreach (JObject order in ordersArray)
                        {
                            string orderNumber = (string)order["order"];
                            cbOrder.Items.Add(orderNumber);
                        }

                        localData = ordersArray;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to retrieve data. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
