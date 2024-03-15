using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MulticastChat
{
    public partial class Form1 : Form
    {
        private const int port = 23000;
        private const string multicastAddress = "235.0.0.0";
        private UdpClient client;
        private IPAddress multicastIPAddress;
        private IPEndPoint remoteEndPoint;

        public Form1()
        {
            InitializeComponent();
            client = new UdpClient();
            multicastIPAddress = IPAddress.Parse(multicastAddress);
            remoteEndPoint = new IPEndPoint(multicastIPAddress, port);

            // Запускаем поток для приема сообщений от сервера
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        // Метод для приема сообщений от сервера
        private void ReceiveMessages()
        {
            try
            {
                client.JoinMulticastGroup(multicastIPAddress); // Присоединяемся к мультикаст группе
                while (true)
                {
                    byte[] data = client.Receive(ref remoteEndPoint); // Получаем сообщение
                    string message = Encoding.UTF8.GetString(data); // Декодируем сообщение
                    DisplayMessage(message); // Отображаем сообщение в UI
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при приеме сообщения: " + ex.Message);
            }
        }

        // Метод для отображения сообщения в UI
        private void DisplayMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => listBoxChat.Items.Add(message)));
            }
            else
            {
                listBoxChat.Items.Add(message);
            }
        }

        // Метод для отправки сообщения на сервер
        private async Task SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message); // Кодируем сообщение
                await client.SendAsync(data, data.Length, remoteEndPoint); // Асинхронно отправляем сообщение на сервер
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке сообщения: " + ex.Message);
            }
        }


        // Обработчик нажатия на кнопку "Отправить"
        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = textBoxMessage.Text;
            SendMessage(message);
            textBoxMessage.Clear();
        }

        private void listBoxChat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Получаем выбранный элемент в списке чата
            var selectedItem = listBoxChat.SelectedItem;

            // Проверяем, что выбранный элемент не является null и что он строкового типа
            if (selectedItem != null && selectedItem is string)
            {
                // Если условия выполнены, то преобразуем выбранный элемент в строку
                string selectedMessage = selectedItem.ToString();

                // Выводим выбранное сообщение в отдельном окне для подробного просмотра
                MessageBox.Show(selectedMessage, "Подробное сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
