using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;

namespace ExpLogs
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private const string GoogleScriptUrl = "";
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(descriptionEntry.Text) ||
                typePicker.SelectedItem == null ||
                string.IsNullOrWhiteSpace(amountEntry.Text) ||
                !decimal.TryParse(amountEntry.Text, out decimal amount))
            {
                await DisplayAlert("Error", "Please fill all fields correctly!", "OK");
                return;
            }

            submitButton.IsEnabled = false; // Disable the button to prevent multiple submissions
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;

            var expenditureData = new
            {
                Date = datePicker.Date.ToString("yyyy-MM-dd"),
                Description = descriptionEntry.Text,
                Type = typePicker.SelectedItem.ToString(),
                Amount = amount.ToString("0.00") // Ensure proper formatting
            };

            string json = JsonConvert.SerializeObject(expenditureData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(GoogleScriptUrl, content);

                loadingIndicator.IsVisible = false;
                loadingIndicator.IsRunning = false;
                submitButton.IsEnabled = true; // Re-enable button

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Data saved to Google Sheets!", "OK");
                    descriptionEntry.Text = string.Empty;
                    typePicker.SelectedIndex = -1;
                    amountEntry.Text = string.Empty;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to save data.", "OK");
                }
            }
        }

    }

    public partial class ShowSheetPage : ContentPage
    {
        private const string GoogleScriptUrl = "https:/kV-yHnUxanlCItEXk3n7ZhabNzqJ/exec";

        public ObservableCollection<Expenditure> Expenditures { get; set; } = new ObservableCollection<Expenditure>();

        public ShowSheetPage()
        {
            InitializeComponent();
            dataCollectionView.ItemsSource = Expenditures;
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                using HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(GoogleScriptUrl);
                var data = JsonConvert.DeserializeObject<List<Expenditure>>(json);

                Expenditures.Clear();
                foreach (var item in data)
                {
                    Expenditures.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load data: {ex.Message}", "OK");
            }
        }
    }



    public class Expenditure
    {
        [JsonProperty("Date")]
        public string Date { get; set; }  

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Amount")]
        public decimal Amount { get; set; }
    }

}
