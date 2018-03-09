using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;

namespace Tools
{
    public class SendReceive
    {
        public static void send(Socket clientSock, byte[] data)
        {
            //글자수 전송
            int dl = data.Length;
            byte[] dlb = BitConverter.GetBytes(dl);
            clientSock.Send(dlb);

            //답변
            byte[] lb1 = new byte[8196];
            clientSock.Receive(lb1);
            //bool lbool1 = BitConverter.ToBoolean(lb1, 0);
            
            //메모리전송
            clientSock.Send(data);

            //답변
            byte[] lb2 = new byte[8196];
            clientSock.Receive(lb2);
            //bool lbool1 = BitConverter.ToBoolean(lb1, 0);
            //BitConverter.ToInt32(bytes, 0);
        }

        public static byte[] Receive(Socket clientSock)
        {
            //길이 받기
            byte[] dlb = new byte[8196];
            clientSock.Receive(dlb);
            int length = BitConverter.ToInt32(dlb, 0);

            //true 반환
            clientSock.Send(BitConverter.GetBytes(true));

            //데이터 받기
            byte[] data = new byte[length];
            clientSock.Receive(data);

            //결과 반환
            clientSock.Send(BitConverter.GetBytes(true));

            return data;
        }        
    }

    public class ServerSocket
    {
        Socket sock;
        Socket clientSock;
        
        public ServerSocket(int port)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // (2) 포트에 바인드
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            sock.Bind(ep);

            // (3) 포트 Listening 시작
            sock.Listen(10);

            // (4) 연결을 받아들여 새 소켓 생성 (하나의 연결만 받아들임)
            Socket clientSock = sock.Accept();

            //byte[] buff = new byte[8192];
            //while (!Console.KeyAvailable) // 키 누르면 종료
            //{
            //    // (5) 소켓 수신
            //    int n = clientSock.Receive(buff);

            //    string data = Encoding.UTF8.GetString(buff, 0, n);
            //    Console.WriteLine(data);

            //    // (6) 소켓 송신
            //    clientSock.Send(buff, 0, n, SocketFlags.None);  // echo
            //}
        }

        public void Close()
        {
            // (7) 소켓 닫기
            clientSock.Close();
            sock.Close();
        }
    }

    public class ClientSocket
    {
        Socket sock;
        
        public ClientSocket(string serverIP, int port)
        {
            // (1) 소켓 객체 생성 (TCP 소켓)
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // (2) 서버에 연결
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIP), port);
            sock.Connect(ep);

            string cmd = string.Empty;
            //byte[] receiverBuff = new byte[8192];

            //Console.WriteLine("Connected... Enter Q to exit");

            //// Q 를 누를 때까지 계속 Echo 실행
            //while ((cmd = Console.ReadLine()) != "Q")
            //{
            //    byte[] buff = Encoding.UTF8.GetBytes(cmd);

            //    // (3) 서버에 데이타 전송
            //    sock.Send(buff, SocketFlags.None);

            //    // (4) 서버에서 데이타 수신
            //    int n = sock.Receive(receiverBuff);

            //    string data = Encoding.UTF8.GetString(receiverBuff, 0, n);
            //    Console.WriteLine(data);
            //}

            // (5) 소켓 닫기

        }

        public void Close()
        {
            sock.Close();
        }
    }

    
}