using Google.GenAI;
using Google.GenAI.Types;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private const string API_KEY = "AIzaSyCoQzrSGIFca7OFVrdug8_k2dIHfoG2dX0";
        private const string MODEL_NAME = "gemini-2.5-flash";
        private readonly Client client;
        private readonly List<Content> chatHistory;

        public Form1()
        {
            InitializeComponent();
            client = new Client(apiKey: API_KEY);
            chatHistory = new List<Content>();
            AddWelcomeMessage();
        }

        private void AddWelcomeMessage()
        {
            AppendMessage("Bot", "Merhaba! Ben yagmurchatbot. Size nasıl yardımcı olabilirim?");
        }

        private async void sendButton_Click(object? sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void messageTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            string userMessage = messageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userMessage))
                return;

            // Kullanıcı mesajını göster
            AppendMessage("Sen", userMessage);
            chatHistory.Add(new Content
            {
                Role = "user",
                Parts = new List<Part> { new Part { Text = userMessage } }
            });
            messageTextBox.Clear();
            sendButton.Enabled = false;

            try
            {
                // Gemini API'ye istek gönder
                var response = await client.Models.GenerateContentAsync(
                    model: MODEL_NAME,
                    contents: chatHistory
                );

                if (response?.Candidates != null && response.Candidates.Count > 0)
                {
                    string botResponse = response.Candidates[0].Content.Parts[0].Text;
                    AppendMessage("Bot", botResponse);
                    chatHistory.Add(new Content
                    {
                        Role = "model",
                        Parts = new List<Part> { new Part { Text = botResponse } }
                    });
                }
                else
                {
                    AppendMessage("Bot", "Üzgünüm, yanıt alınamadı.");
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (errorMessage.Contains("429") || errorMessage.Contains("TooManyRequests"))
                {
                    AppendMessage("Bot", "Çok fazla istek gönderildi. Lütfen birkaç saniye bekleyip tekrar deneyin.");
                }
                else
                {
                    AppendMessage("Bot", $"Üzgünüm, bir hata oluştu: {errorMessage}");
                }
            }
            finally
            {
                sendButton.Enabled = true;
                messageTextBox.Focus();
            }
        }

        private void AppendMessage(string sender, string message)
        {
            if (chatTextBox.InvokeRequired)
            {
                chatTextBox.Invoke(new Action(() => AppendMessage(sender, message)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm");
            string formattedMessage = $"[{timestamp}] {sender}: {message}\n\n";
            
            chatTextBox.SelectionStart = chatTextBox.TextLength;
            chatTextBox.SelectionLength = 0;
            
            if (sender == "Sen")
            {
                chatTextBox.SelectionColor = System.Drawing.Color.FromArgb(66, 133, 244);
            }
            else
            {
                chatTextBox.SelectionColor = System.Drawing.Color.Black;
            }
            
            chatTextBox.AppendText(formattedMessage);
            chatTextBox.SelectionColor = chatTextBox.ForeColor;
            chatTextBox.ScrollToCaret();
        }

    }
}
