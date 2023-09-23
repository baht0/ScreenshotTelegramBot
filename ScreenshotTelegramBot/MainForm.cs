using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenshotTelegramBot
{
    public partial class MainForm : Form
    {
        private decimal TimerSec = 0;
        private string ScreenFileName => AppDomain.CurrentDomain.BaseDirectory + "screen.png";

        public MainForm() => InitializeComponent();

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(captionСomboBox.Text))
            {
                groupBox1.Enabled = false;
                TimerSec = timerNumericUpDown.Value;
                timer.Enabled = true;
            }
        }
        private async void Timer_Tick(object sender, EventArgs e)
        {
            timerLabel.Text = $"Next check: {TimerSec} sec.";
            if (TimerSec <= 0)
            {
                TimerSec = timerNumericUpDown.Value;
                TakeScreen();
                await Send();
            }
            TimerSec--;
        }

        //Screenshot
        private void TakeScreen()
        {
            try
            {
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                var bitmap = new Bitmap(screenWidth, screenHeight);
                var g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                bitmap.Save(ScreenFileName, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Status: Exception (take screen)";
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "ex_take_screen.txt", ex.Message);
            }
        }
        //Telegram API
        private async Task Send()
        {
            try
            {
                string chat_id = chatTextBox.Text;
                string bot_token = tokenTextBox.Text;

                var fileStream = new FileStream(ScreenFileName, FileMode.Open, FileAccess.Read);
                var content = new MultipartFormDataContent
                {
                    { new StringContent(chat_id, Encoding.UTF8), "chat_id" },
                    { new StreamContent(fileStream), "photo", fileStream.Name },
                    { new StringContent(captionСomboBox.Text, Encoding.UTF8), "caption" }
                };

                var client = new HttpClient();
                var res = await client.PostAsync("https://api.telegram.org/bot" + bot_token + "/sendPhoto?", content);
                statusLabel.Text = "Status: " + res.StatusCode.ToString();
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Status: Exception (send)";
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "ex_send.txt", ex.Message);
            }
        }
    }
}
