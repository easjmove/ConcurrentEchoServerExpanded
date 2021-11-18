using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConcurrentEchoServerExpanded
{
    //to keep the code more readable all previous comments are removed
    //open the ConcurrentEchoServer to read them
    class Program
    {
        //Only IPAddress is changed in main method
        static void Main(string[] args)
        {
            Console.WriteLine("Server:");

            //changed the IPAddress.Loopback to IPAddress.Any, in order for the listener to listen on all network adapters
            TcpListener listener = new TcpListener(IPAddress.Any, 7);

            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Task.Run(() => HandleClient(socket));
            }
            listener.Stop();
        }

        public static void HandleClient(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);

            //added a bool to keep track of when to end the loop
            bool keepReading = true;

            //added a loop to keep reading messages from the client, until a command is send to close the connection
            while (keepReading)
            {
                string message = reader.ReadLine();
                Console.WriteLine(message);

                //a string to store the result, this is being changed in the if statement below
                string result = "";

                //splits the read message if it contains a space
                string[] splittedMessage = message.Split(" ");
                
                //checks if the message contains one or spaces
                if (splittedMessage.Length > 1)
                {
                    //here it assumes that the first part of the message is a function
                    string messageFunction = splittedMessage[0].ToLower();

                    //concats the rest of the message to a single string
                    //if the text the clients sends contains other spaces
                    //+1 to not include the space
                    string messageText = message.Substring(message.IndexOf(" ") + 1);
                    
                    //a switch to see what funtion the client wanted done
                    switch (messageFunction)
                    {
                        case "toupper":
                            result = messageText.ToUpper();
                            break;
                        case "tolower":
                            result = messageText.ToLower();
                            break;
                        case "reverse":
                            char[] charArray = messageText.ToCharArray();
                            Array.Reverse(charArray);
                            result = new string(charArray);
                            break;
                        //if no function is understood, return the entire message (simple echo)
                        default:
                            result = message;
                            break;
                    }
                }
                //if the client simply send a close and nothing else, the server sets the bool false
                else if (message.ToLower() == "close")
                {
                    keepReading = false;
                }

                //writes either the entire message or the manipulated message back to the client
                writer.WriteLine(result);
                writer.Flush();
            }

            socket.Close();
        }
    }
}
