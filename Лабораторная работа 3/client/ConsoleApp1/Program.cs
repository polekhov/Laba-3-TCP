using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SendMessageFromSocket(5354);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static void SendMessageFromSocket(int port)
        {

            




            IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddr = Array.FindAll(ipHost.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            sender.Connect(ipEndPoint);

            Console.Write("Send MEssage: ");
            string message = Console.ReadLine();

            Console.WriteLine("\nSocet connect with {0} ", sender.RemoteEndPoint.ToString());
            byte[] msg = Encoding.UTF8.GetBytes(message);

            sender.Send(BitConverter.GetBytes(msg.Length));
            int bytesSent = sender.Send(msg);

            byte[] buffer = new byte[4]; //Буфер, куда получим информацию ниже
            int l = sender.Receive(buffer); //Получаем длину ответа от сервера
            if (l != 4) { //Должно обязательно приняться ровно 4 байта, в противном случае, это не то что нужно
                Console.WriteLine("Не получено число байт в ответе");
                sender.Close();
                return;
            }
            l = BitConverter.ToInt32(buffer, 0); //Переводим полученные байты в число int (длина сообщения)

            byte[] bytes = new byte[l];
            int index = 0;//index будет смещаться после приёма новых данных
            while (index < bytes.Length) { //Пока не будет получено нужное кол-во байт, сокет будет пытаться принять ещё
                index += sender.Receive(bytes, index, bytes.Length - index, SocketFlags.None); //Нельзя принять больше информации, чем <длина массива> - <текущий индекс> 
            }
            Console.WriteLine("\nServer answer: {0}\n", Encoding.UTF8.GetString(bytes));


            if (message.IndexOf("exit") == -1)
                SendMessageFromSocket(port);


            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}