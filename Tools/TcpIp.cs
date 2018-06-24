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
        public static void SendGeneric<T>(Socket clientSock, T obj)
        {
            byte[] dataByte = SerializationUtil.SerializeToByte(obj);
            send(clientSock, dataByte);
        }

        public static T ReceiveGeneric<T>(Socket clientSock)
        {
            object obj = Receive(clientSock);
            if (obj != null)
            {
                byte[] dataByte = Receive(clientSock);
                T data = (T)SerializationUtil.DeserializeToObject(dataByte);
                return data;
            }
            else
            { return default(T); }
        }

        public static void send(Socket clientSock, byte[] data)
        {
            // 객체의 바이트수 계산, null이거나 바이트가 0이면 실데이터는 전송하지 않음
            int dl = 0;
            if (data != null || data.Length == 0) dl = data.Length;

            // 객체의 바이트수 전송
            byte[] dlb = BitConverter.GetBytes(dl);
            clientSock.Send(dlb);

            // 객체의 바이트수 답변 받음
            byte[] lb1 = GetBytesFromStream(clientSock, 4);

            // 객체의 바이트수가 잘 전달되었는지 체크
            bool isRightLength = true;
            for (int i = 0; i < 4; i++)
            {
                if (dlb[i] != lb1[i]) isRightLength = false;
            }

            // 잘 전달되었는지 아닌지 전송, 잘못 전송되었다면 예외처리
            if (isRightLength == true)
            {
                clientSock.Send(Encoding.UTF8.GetBytes(@"!#%&("));
            }
            else
            {
                clientSock.Send(Encoding.UTF8.GetBytes(@"@$^*)"));
                throw new Exception(@"incorrect message length sended");
            }

            // 바이트수가 0 이상이어야 실데이터가 있으므로 전송
            if (dl > 0)
            {
                //메모리전송
                clientSock.Send(data);
            }
        }

        public static byte[] Receive(Socket clientSock)
        {
            // 객체의 바이트수 수신
            byte[] dlb = GetBytesFromStream(clientSock, 4);

            // 객체 바이트수 발신(echo)
            clientSock.Send(dlb);

            // 올바른 바이트수였는지 여부 수신
            // : "!#%&(" 이면 OK, "@$^*)"이면 에러
            byte[] respond = GetBytesFromStream(clientSock, 5);

            // 데이터 길이가 맞는지 다시 확인받음
            string respondStr = Encoding.UTF8.GetString(respond);
            if (respondStr == @"@$^*)") throw new Exception(@"incorrect message length received");

            int lth = BitConverter.ToInt16(dlb, 0);
            if (lth > 0)
            {
                byte[] dataReceived = GetBytesFromStream(clientSock, lth);
                return dataReceived;
            }
            else return null;
        }

        private static byte[] GetBytesFromStream(Socket sock, int lth)
        {
            byte[] dataBytes = new byte[lth];
            for (int i = 0; i < lth; i++)
            {
                byte[] tick = new byte[1];
                sock.Receive(tick);
                dataBytes[i] = tick[0];
            }
            return dataBytes;
        }
    }

    public class ServerSocket
    {
        public Socket sock;
        public Socket clientSock;
        
        public ServerSocket(int port)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // (2) 포트에 바인드
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            sock.Bind(ep);

            // (3) 포트 Listening 시작
            sock.Listen(10);
            
            // (4) 연결을 받아들여 새 소켓 생성 (하나의 연결만 받아들임)
            clientSock = sock.Accept();
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
        public Socket sock;
        
        public ClientSocket(string serverIP, int port)
        {
            // (1) 소켓 객체 생성 (TCP 소켓)
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // (2) 서버에 연결
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(serverIP), port);
            sock.Connect(ep);
        }
        
        public void Close()
        {
            // (5) 소켓 닫기
            sock.Close();
        }
    }    
}