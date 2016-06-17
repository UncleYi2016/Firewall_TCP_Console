using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace Firewall_Console_Test {
    class Program
	    {  
	        private static int myProt = 80;   //端口  
            static Socket serverSocket;
	        static void Main(string[] args)
	        {
            //服务器IP地址  
                IPAddress ip = IPAddress.Any;
	            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  
	            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
	            serverSocket.Listen(10);    //设定最多10个排队连接请求  
	            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());  
	            //通过Clientsoket发送数据  
	            Thread myThread = new Thread(ListenClientConnect);  
	            myThread.Start();
            while (true) {
                string denyIp = Console.ReadLine();
                StreamWriter dw = File.AppendText("./deny.txt");
                dw.WriteLine(denyIp);
                dw.Close();
            }  
	        }  
	  
	        /// <summary>  
	        /// 监听客户端连接  
	        /// </summary>  
	    private static void ListenClientConnect()
	    {
	        while (true)  
	        {
                StreamWriter sw = File.AppendText("./log.txt");
                Socket clientSocket = serverSocket.Accept();
                try {
                    bool deny = false;
                    ArrayList denyTable = new ArrayList();
                    StreamReader rd = new StreamReader("./deny.txt");
                    while(rd.Peek() >= 0) {
                        denyTable.Add(rd.ReadLine());
                    }
                    rd.Close();
                    // clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                    IPEndPoint point = clientSocket.RemoteEndPoint as IPEndPoint;
                    sw.WriteLine("********************************");
                    sw.WriteLine("Time: " + DateTime.Now.ToString());
                    sw.WriteLine("Connect IP: " + point.Address.ToString());
                    sw.WriteLine("Connect port: " + point.Port.ToString());
                    if (denyTable.Contains(point.Address.ToString())) {
                        Console.WriteLine("Reject");
                        string rejectStr = "Your IP has been rejected./r/n";
                        clientSocket.Send(Encoding.ASCII.GetBytes(rejectStr), rejectStr.Length, SocketFlags.None);
                        clientSocket.Close();
                        sw.WriteLine("Operation: Reject");
                        sw.WriteLine("********************************");
                        deny = true;
                        continue;
                    }
                    sw.WriteLine("Operation: Accept");
                    sw.WriteLine("********************************");

                    Thread receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start(clientSocket);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                } finally {
                    sw.Close();
                }
	            
	        }  
	    }  
	  
	        /// <summary>  
	        /// 接收消息  
	        /// </summary>  
	        /// <param name="clientSocket"></param>  
	    private static void ReceiveMessage(object clientSocket)
	    {
            int bufferSize = 131072;
            byte[] clientRecv = new byte[bufferSize];
            byte[] serverRecv = new byte[bufferSize];
            Socket myClientSocket = (Socket)clientSocket;
            //设定服务器IP地址  
            IPAddress ip = IPAddress.Parse(/*IP*/);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                serverSocket.Connect(new IPEndPoint(ip, /*port*/)); //配置服务器IP与端口  
                //Console.WriteLine("连接服务器成功");
            } catch {
                Console.WriteLine("连接服务器失败，请按回车键退出！");
                Thread.CurrentThread.Abort();
            }
	        try  
	        {
                //通过clientSocket接收数据  

                int receiveNumber = myClientSocket.Receive(clientRecv);
                //byte a = 0;
                //int len = Array.IndexOf(clientRecv, a);
                //len = (len == -1) ? bufferSize : len;
                if(receiveNumber == 0) {
                    Console.WriteLine("空包");
                    Thread.CurrentThread.Abort();
                }
                Console.WriteLine(Encoding.ASCII.GetString(clientRecv, 0, 20));
                int sendNumber = serverSocket.Send(clientRecv, receiveNumber, SocketFlags.None);
                //MemoryStream ms = new MemoryStream();

                while (0 != (receiveNumber = serverSocket.Receive(serverRecv))) {
                    //Console.WriteLine("Receive:" + receiveNumber);
                    //Console.WriteLine(Encoding.ASCII.GetString(serverRecv, 0, 20));
                    //len = Array.IndexOf(serverRecv, a);
                    //len = (len == -1) ? bufferSize : len;
                    //Console.WriteLine(Encoding.ASCII.GetString(result, 0, 7));
                    sendNumber = myClientSocket.Send(serverRecv, receiveNumber, SocketFlags.None);
                    //Console.WriteLine("Send:" + sendNumber);
                    //ms.Write(result, 0, receiveNumber);
                }
                //byteReceive = ms.ToArray();
                //ms.Close();
                    
            } 
	        catch(Exception ex)  
	        {  
	            Console.Write(ex.Message);  
	        } finally {
                Console.WriteLine("\tSocket closed!");
                myClientSocket.Shutdown(SocketShutdown.Both);
                myClientSocket.Close();
            }
	    }  
	    }  
	}

