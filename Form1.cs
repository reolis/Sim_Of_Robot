﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Krasnyanskaya221327_Lab01_Sem5_Ver2
{
    public partial class Form1 : Form
    {
        static int count = 0;
        static int oldCount = 0;
        private int savedCount = 0;
        private int N = 0;

        public static UDPServer server;

        private DateTime moveBackStartTime;
        private bool isMovingBack = false;
        private bool isMovingForward = false;
        private TimeSpan moveBackDuration = TimeSpan.FromSeconds(1);
        private int bumpCount = 0;

        public class UDPServer
        {
            public IPAddress IpAddress { get; set; }
            public int LocalPort { get; set; }
            public int RemotePort { get; set; }
            public UdpClient UdpClient { get; set; }
            public IPEndPoint IpEndPoint { get; set; }
            public byte[] Data { get; set; }
            public static Dictionary<string, int> DecodeText;
           
            public static string DecodeData { get; set; }
            public static int n, s, c, le, re, az, b, d0, d1, d2, d3, d4, d5, d6, d7, l0, l1, l2, l3, l4;

            public UDPServer(IPAddress ip, int localPort, int remotePort)
            {
                IpAddress = ip;
                LocalPort = localPort;
                RemotePort = remotePort;
                UdpClient = new UdpClient(LocalPort);
                IpEndPoint = new IPEndPoint(IpAddress, LocalPort);
            }

            public async Task ReceiveDataAsync()
            {
                while (true)
                {
                    var receivedResult = await UdpClient.ReceiveAsync();
                    Data = receivedResult.Buffer;
                    DecodingData(Data);
                }
            }

            public async Task SendDataAsync(byte[] data)
            {
                if (data != null)
                {
                    IPEndPoint pointServer = new IPEndPoint(IpAddress, RemotePort);
                    await UdpClient.SendAsync(data, data.Length, pointServer);
                }
            }

            public async Task SendRobotDataAsync()
            {
                string robotData = Robot.GetCommandsAsJson();
                byte[] dataToSend = Encoding.ASCII.GetBytes(robotData + "\n");
                await SendDataAsync(dataToSend);
            }

            private void DecodingData(byte[] data)
            {
                var message = Encoding.ASCII.GetString(data);
                DecodeText = JsonConvert.DeserializeObject<Dictionary<string, int>>(message);
                var lines = DecodeText.Select(kv => kv.Key + ": " + kv.Value.ToString());
                DecodeData = "IoT: " + string.Join(Environment.NewLine, lines);

                AnalyzeData(DecodeText);
            }

            private void AnalyzeData(Dictionary<string, int> pairs)
            {
                if (pairs.ContainsKey("n"))
                {
                    n = pairs["n"];
                    s = pairs["s"];
                    c = pairs["c"];
                    le = pairs["le"];
                    re = pairs["re"];
                    az = pairs["az"];
                    b = pairs["b"];
                    d0 = pairs["d0"];
                    d1 = pairs["d1"];
                    d2 = pairs["d2"];
                    d3 = pairs["d3"];
                    d4 = pairs["d4"];
                    d5 = pairs["d5"];
                    d6 = pairs["d6"];
                    d7 = pairs["d7"];
                    l0 = pairs["l0"];
                    l1 = pairs["l1"];
                    l2 = pairs["l2"];
                    l3 = pairs["l3"];
                    l4 = pairs["l4"];
                }
                else
                {
                    MessageBox.Show("No data");
                }
            }

        }

        public static class Robot
        {
            public static Dictionary<string, int> Commands = new Dictionary<string, int>
            {
                { "N", 0 },
                { "M", 0 },
                { "F", 0 },
                { "B", 0 },
                { "T", 0 },
            };

            public static bool isInStartZone = false;
            public static bool isInWaitingZone = false;
            public static bool isPalletGet = false;
            public static bool isReadyToPick = false;
            public static bool PickedOrder = false;
            public static bool ReturnedToFinal = false;
            public static bool FinalStateGot = false;
            public static int countOfOrders = 0;
            public static int n, s, c, le, re, az, b, d0, d1, d2, d3, d4, d5, d6, d7, l0, l1, l2, l3, l4;
            public static bool isFirstRotateDone = false, isFirstWayDone = false;

            public static void UpdateData(Dictionary<string, int> pairs)
            {
                n = pairs["n"];
                s = pairs["s"];
                c = pairs["c"];
                le = pairs["le"];
                re = pairs["re"];
                az = pairs["az"];
                b = pairs["b"];
                d0 = pairs["d0"];
                d1 = pairs["d1"];
                d2 = pairs["d2"];
                d3 = pairs["d3"];
                d4 = pairs["d4"];
                d5 = pairs["d5"];
                d6 = pairs["d6"];
                d7 = pairs["d7"];
                l0 = pairs["l0"];
                l1 = pairs["l1"];
                l2 = pairs["l2"];
                l3 = pairs["l3"];
                l4 = pairs["l4"];
            }

            

            public static string GetCommandsAsJson()
            {
                return JsonConvert.SerializeObject(Commands);
            }

            public static void LoadCommandsFromJson(string json)
            {
                var newCommands = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                if (newCommands != null)
                {
                    Commands = newCommands;
                }
            }

            public static void SetCommand(string key, int value)
            {
                if (Commands.ContainsKey(key))
                {
                    Commands[key] = value;
                }
                else
                {
                    throw new ArgumentException("Команда с таким ключом не существует.");
                }
            }

            public static void UpdateDecodeText()
            {
                UDPServer.DecodeText["n"] = Commands["N"];
            }

            public static void SendOldCommands()
            {
                string oldCommands = JsonConvert.SerializeObject(UDPServer.DecodeText, Formatting.None);

                byte[] data = Encoding.ASCII.GetBytes(oldCommands + "\n");

                UdpClient udpCommands = new UdpClient();
                IPEndPoint pointServer = new IPEndPoint(server.IpAddress, server.RemotePort);
                udpCommands.Send(data, data.Length, pointServer);

                string jsonString = JsonConvert.SerializeObject(Commands, Formatting.None);
                byte[] dataForRobot = Encoding.ASCII.GetBytes(jsonString + "\n");

                udpCommands.Send(dataForRobot, dataForRobot.Length, pointServer);
            }

            public static void RotateRight()
            {
                SetCommand("B", 25);
            }

            public static void RotateLeft()
            {
                SetCommand("B", -25);
            }

            public static void MoveStraight()
            {
                SetCommand("F", 100);
            }

            public static void MoveBack()
            {
                SetCommand("F", -100);
            }

            public static void Stop()
            {
                SetCommand("B", 0);
                SetCommand("F", 0);
            }

            public static void MoveBackWhenBump()
            {
                SetCommand("F", -70);
                SetCommand("B", -25);
            }

            public static void FirstRotate(int count)
            {
                if (count <= 10)
                {
                    RotateLeft();
                    isFirstRotateDone = true;

                    SetCommand("N", count);
                    UpdateDecodeText();
                }
                else
                {
                    Stop();

                    SetCommand("N", count);
                    UpdateDecodeText();
                }
            }

            public static void FirstWay(int count)
            {
                if (count > 10 && count <= 100)
                {
                    MoveStraight();
                    isFirstWayDone = true;

                    SetCommand("N", count);
                    UpdateDecodeText();
                }
                else
                {
                    Stop();

                    SetCommand("N", count);
                    UpdateDecodeText();
                }
            }

        }

        public Form1()
        {
            InitializeComponent();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            server = new UDPServer(IPAddress.Parse(textBox3.Text), Int32.Parse(textBox2.Text), Int32.Parse(textBox1.Text));
            await server.ReceiveDataAsync();
           
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (UDPServer.DecodeData != null)
            {
                SplitDataToTextBoxs();

                richTextBox1.Text += "\r\n" + "Here is data";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();

                // Проверяем, произошло ли столкновение
                if (UDPServer.b == 1)
                {
                    bumpCount++; // Увеличиваем счетчик столкновений

                    if (bumpCount % 2 == 1)
                    {
                        // Нечетное столкновение (например, первое, третье) — начинаем двигаться назад
                        if (!isMovingBack)
                        {
                            savedCount = count;
                            Robot.MoveBackWhenBump();
                            moveBackStartTime = DateTime.Now;
                            isMovingBack = true;
                        }
                    }
                    else
                    {
                        // Четное столкновение (второе, четвертое) — начинаем двигаться вперед
                        if (isMovingBack)
                        {
                            Robot.SetCommand("B", 0);
                            Robot.MoveStraight();
                            isMovingBack = false;
                            isMovingForward = true;
                        }
                    }
                }

                // Логика движения назад после нечетного столкновения
                if (isMovingBack)
                {
                    if (DateTime.Now - moveBackStartTime >= moveBackDuration)
                    {
                        Robot.Stop();
                        isMovingBack = false;
                        count = savedCount; // Возвращаем сохранённое значение count после движения назад
                    }
                    else
                    {
                        N++; // Увеличиваем N независимо от count
                        Robot.SetCommand("N", N);

                        Robot.UpdateDecodeText();
                        Robot.SendOldCommands();
                        await server.SendRobotDataAsync();

                        textBox19.Text = count.ToString();
                        return; // Выходим, чтобы не выполнять дальнейший код
                    }
                }

                // Логика движения вперед после четного столкновения
                if (isMovingForward)
                {
                    Robot.MoveStraight(); // Движение вперёд после повторного столкновения
                    N++; // Увеличиваем N
                    Robot.SetCommand("N", N);
                }
                else
                {
                    // Движение до столкновения
                    if (count <= 10)
                    {
                        Robot.RotateLeft();
                        count++;
                    }
                    else if (count > 10 && count <= 30)
                    {
                        Robot.SetCommand("B", 0);
                        Robot.MoveStraight();
                        count++;
                    }
                    else if (count > 30 && count <= 35)
                    {
                        Robot.SetCommand("F", 0);
                        Robot.RotateRight();
                        count++;
                    }
                    else if (count > 35 && count < 138)
                    {
                        Robot.SetCommand("B", 0);
                        Robot.MoveStraight();
                        count++;
                    }
                    else if (count >= 138 && count <= 150)
                    {
                        Robot.RotateLeft();
                        count++;
                    }
                    else if (count > 150 && count <= 175)
                    {
                        Robot.SetCommand("B", 0);
                        Robot.MoveStraight();
                        count++;
                    }
                    else if (count > 175 && count <= 200)
                    {
                        Robot.RotateLeft();
                        count++;
                    }
                    else if (count > 200 && count <= 253)
                    {
                        Robot.SetCommand("B", 0);
                        Robot.MoveStraight();
                        count++;
                    }
                    else if (count > 253 && count <= 275)
                    {
                        Robot.RotateLeft();
                        count++;
                    }
                    else if (count > 275)
                    {
                        Robot.SetCommand("B", 0);
                        Robot.MoveStraight();
                        count++;
                    }
                    else if (count > 400)
                    {
                        Robot.Stop();
                        count++;
                    }
                    else
                    {
                        Robot.Stop();
                        count++;
                    }

                    // Увеличиваем N независимо от count
                    N++;
                    Robot.SetCommand("N", N);
                }

                // Обновляем данные робота
                Robot.UpdateDecodeText();
                Robot.SendOldCommands();
                await server.SendRobotDataAsync();

                textBox19.Text = count.ToString();
            }
            else
            {
                richTextBox1.Text += "\r\n" + "No data";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        public void MoveRobot()
        {
            if (!Robot.isFirstRotateDone)
            {
                Robot.FirstRotate(count);
                Robot.UpdateDecodeText();
                if (Robot.isFirstRotateDone)
                {
                    Robot.FirstWay(count);
                    Robot.UpdateDecodeText();
                }
            }
            else
            {
                if (count > 10 && count <= 100)
                {
                    Robot.MoveStraight();
                    Robot.UpdateDecodeText();
                }
                else if (count > 100)
                {
                    Robot.RotateRight();
                    Robot.UpdateDecodeText();
                }
            }

            count++;

            Robot.SetCommand("N", count);
        }


        public void SplitDataToTextBoxs()
        {
            var message = Encoding.ASCII.GetString(server.Data);
            var text = JsonConvert.DeserializeObject<Dictionary<string, int>>(message);

            foreach (var chr in text)
            {
                if (chr.Key == "d0")
                {
                    textBox4.Text = chr.Value.ToString();
                }
                if (chr.Key == "d1")
                {
                    textBox5.Text = chr.Value.ToString();
                }
                if (chr.Key == "d2")
                {
                    textBox6.Text = chr.Value.ToString();
                }
                if (chr.Key == "d3")
                {
                    textBox7.Text = chr.Value.ToString();
                }
                if (chr.Key == "d4")
                {
                    textBox8.Text = chr.Value.ToString();
                }
                if (chr.Key == "d5")
                {
                    textBox9.Text = chr.Value.ToString();
                }
                if (chr.Key == "d6")
                {
                    textBox10.Text = chr.Value.ToString();
                }
                if (chr.Key == "d7")
                {
                    textBox11.Text = chr.Value.ToString();
                }

                if(chr.Key == "n")
                {
                    textBox12.Text = chr.Value.ToString();
                }
                if (chr.Key == "s")
                {
                    textBox13.Text = chr.Value.ToString();
                }
                if (chr.Key == "c")
                {
                    textBox14.Text = chr.Value.ToString();
                }
                if (chr.Key == "re")
                {
                    textBox15.Text = chr.Value.ToString();
                }
                if (chr.Key == "le")
                {
                    textBox16.Text = chr.Value.ToString();
                }
                if (chr.Key == "az")
                {
                    textBox17.Text = chr.Value.ToString();
                }
                if (chr.Key == "b")
                {
                    textBox18.Text = chr.Value.ToString();
                }
                if (chr.Key == "l0")
                {
                    textBox24.Text = chr.Value.ToString();
                }
                if (chr.Key == "l1")
                {
                    textBox23.Text = chr.Value.ToString();
                }
                if (chr.Key == "l2")
                {
                    textBox22.Text = chr.Value.ToString();
                }
                if (chr.Key == "l3")
                {
                    textBox21.Text = chr.Value.ToString();
                }
                if (chr.Key == "l4")
                {
                    textBox20.Text = chr.Value.ToString();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(solutionDirectory, "textbox_data.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<dynamic>(json);

                textBox1.Text = data.TextBox1;
                textBox2.Text = data.TextBox2;
                textBox3.Text = data.TextBox3;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var data = new
            {
                TextBox1 = textBox1.Text,
                TextBox2 = textBox2.Text,
                TextBox3 = textBox3.Text
            };

            string solutionDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(solutionDirectory, "textbox_data.json");

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
