using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace PROG11
{
    internal class Server
    {
        TcpListener listener;

        public Server(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);

            Console.WriteLine("Запускаем прослушку");

            listener.Start();

            Console.WriteLine("Запустили прослушку");

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                Thread thread = new Thread(clientThread);
                thread.Start(tcpClient);
                Console.WriteLine("Клиента поймали");
            }
        }

        static void clientThread(object client) 
        {
            TcpClient tcpClient = (TcpClient)client;
            Client newclient = new Client(tcpClient);

        }

        static void Main(string[] args)
        {
            Server server = new Server(80);
        }
    }

    class Client
    {
        public Client(TcpClient client)
        {
            string request = "";

            byte[] buffer = new byte[1024];

            int count;

            count = client.GetStream().Read(buffer, 0, buffer.Length);

            request = Encoding.UTF8.GetString(buffer, 0, count);
            Console.WriteLine(request);

            string url = "";
            if (request.Length > 2)
            {
                url = GetResourse(request);
            }

            if (url == "/")
            {
                url += "index.html";
            }

            string filePath = "www" + url;

            if (!File.Exists(filePath))
            {
                SendError(client, 404);
                return;
            }

            string contentType = "text/html";
            if (url.Contains(".css"))
            {
                contentType = "text/css";
            }

            FileStream fileStream = new FileStream(filePath, FileMode.Open);

            string initLine = "HTTP/1.1 200 OK";

            string headersLine = "Content-Type: text/html\n" + "Content-Length: " + fileStream.Length;

            string response_first = initLine + "\n" + headersLine + "\n\n";
            byte[] firstBytes = Encoding.UTF8.GetBytes(response_first);
            NetworkStream networkStream = client.GetStream();
            networkStream.Write(firstBytes, 0, firstBytes.Length);
            count = fileStream.Read(buffer, 0, buffer.Length);

            networkStream.Write(buffer, 0, count);

            fileStream.Close();
            client.Close();

        }

        public string GetResourse(string request)
        {
            string[] parts = request.Split();
            return parts[1];
        }

        public void SendError(TcpClient client, int code)
        {
            string initLine = "HTTP/1.1 " + code.ToString() + " " + (HttpStatusCode) code;

            string bodyLine = "<html><body><h1>"+code.ToString() + "</h1></body></html>";

            string headersLine = "Content-Type: text/html\n" + "Content-Length: " + bodyLine.Length.ToString();

            string response = initLine + "\n" + headersLine + "\n\n" + bodyLine;

            byte[] bytes = Encoding.UTF8.GetBytes(response);

            NetworkStream networkStream = client.GetStream();

            networkStream.Write(bytes, 0, bytes.Length);

            client.Close();
        }

        public void Test(TcpClient client)
        {
            string html = "<html><b><font color = red>Hello</font></b></html>";
            string response = "HTTP/1.1 200 OK\n" +
                "Content-Type: text/html\n" +
                "Content-Length: " +
                html.Length.ToString() + "\n\n" +
                html;

            byte[] bytes = Encoding.UTF8.GetBytes(response);

            NetworkStream networkStream = client.GetStream();

            networkStream.Write(bytes, 0, bytes.Length);
            //djdjdjdjd

            client.Close();
        }
    }
}
