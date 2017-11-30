using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddr = Array.FindAll(ipHost.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork)[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 5354);


            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);


                while (true)
                {
                    Console.WriteLine("We are waiting for the connection through the port {0}", ipEndPoint + "\n");


                    Socket handler = sListener.Accept();
                    string data = null;

                    byte[] buffer = new byte[4]; //Буфер, куда получим информацию ниже
                    int l = handler.Receive(buffer); //Получаем длину запроса от клиента в байтах
                    if (l != 4) { //Должно обязательно приняться ровно 4 байта, в противном случае, это не то что нужно
                        Console.WriteLine("Не получено число байт в ответе");
                        handler.Close();
                        continue;
                    }
                    l = BitConverter.ToInt32(buffer, 0); //Переводим полученные байты в число int (длина сообщения)
                    byte[] bytes = new byte[l];
                    int index = 0;//index будет смещаться после приёма новых данных
                    while (index < bytes.Length) { //Пока не будет получено нужное кол-во байт, сокет будет пытаться принять ещё
                        index += handler.Receive(bytes, index, bytes.Length - index, SocketFlags.None); //Нельзя принять больше информации, чем <длина массива> - <текущий индекс> 
                    }

                    data += Encoding.UTF8.GetString(bytes);


                    Console.WriteLine(IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString()) + ":");
                    Console.Write("Received text: " + data + "\n\n");


                    string reply = "Long Message " + data.Length.ToString()
                            + " Characters";
                    byte[] msg = Encoding.UTF8.GetBytes(reply);
                    handler.Send(BitConverter.GetBytes(msg.Length));
                    handler.Send(msg);

                    if (data.IndexOf("exit") > -1)
                    {
                        Console.WriteLine("Server OFF.");
                        break;
                    }


                    handler.Close();
                }
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
    }
}
