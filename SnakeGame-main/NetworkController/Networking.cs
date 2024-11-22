using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil;

public static class Networking
{
    /////////////////////////////////////////////////////////////////////////////////////////
    // Server-Side Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
    /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
    /// AcceptNewClient will continue the event-loop.
    /// </summary>
    /// <param name="toCall">The method to call when a new connection is made</param>
    /// <param name="port">The the port to listen on</param>
    public static TcpListener StartServer(Action<SocketState> toCall, int port)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        try
        {
            var primes = Tuple.Create(listener, toCall);
            listener.BeginAcceptSocket(AcceptNewClient, primes);
        }
        catch (Exception)
        {
        }
        return listener;
    }

    /// <summary>
    /// To be used as the callback for accepting a new client that was initiated by StartServer, and
    /// continues an event-loop to accept additional clients.
    ///
    /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
    /// OnNetworkAction should be set to the delegate that was passed to StartServer.
    /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action.
    ///
    /// If anything goes wrong during the connection process (such as the server being stopped externally),
    /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true
    /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
    /// an error occurs.
    ///
    /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept
    /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
    /// </summary>
    /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with
    /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
    private static void AcceptNewClient(IAsyncResult ar)
    {
        Tuple<TcpListener, Action<SocketState>> tempTuple = (Tuple<TcpListener, Action<SocketState>>)ar.AsyncState!;

        try
        {
            Socket temp = tempTuple.Item1.EndAcceptSocket(ar);
            SocketState tempSocket = new SocketState(tempTuple.Item2, temp);
            tempSocket.OnNetworkAction = tempTuple.Item2;
            tempSocket.OnNetworkAction(tempSocket);
        }
        catch (Exception ex)
        {
            SocketState socketState = new SocketState(tempTuple.Item2, ex.Message);
            socketState.ErrorOccurred = true;
            socketState.ErrorMessage = "Error occurred during connection process";
            socketState.OnNetworkAction(socketState);
        }
        try
        {
            tempTuple.Item1.BeginAcceptSocket(AcceptNewClient, tempTuple);
        }
        catch
        {
            SocketState socketState = new SocketState(tempTuple.Item2, "Error occurred during connection process");
            socketState.OnNetworkAction(socketState);
        }


    }

    /// <summary>
    /// Stops the given TcpListener.
    /// </summary>
    public static void StopServer(TcpListener listener)
    {
        try
        {
            listener.Stop();
        }
        catch
        {

        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    // Client-Side Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Begins the asynchronous process of connecting to a server via BeginConnect,
    /// and using ConnectedCallback as the method to finalize the connection once it's made.
    ///
    /// If anything goes wrong during the connection process, toCall should be invoked
    /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message
    /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
    /// in this method or in ConnectedCallback.
    ///
    /// This connection process should timeout and produce an error (as discussed above)
    /// if a connection can't be established within 3 seconds of starting BeginConnect.
    ///
    /// </summary>
    /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
    /// <param name="hostName">The server to connect to</param>
    /// <param name="port">The port on which the server is listening</param>
    public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
    {
        // TODO: This method is incomplete, but contains a starting point
        //       for decoding a host address

        // Establish the remote endpoint for the socket.
        IPHostEntry ipHostInfo;
        IPAddress ipAddress = IPAddress.None;

        // Determine if the server address is a URL or an IP
        try
        {
            ipHostInfo = Dns.GetHostEntry(hostName);
            bool foundIPV4 = false;
            foreach (IPAddress addr in ipHostInfo.AddressList)
                if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    foundIPV4 = true;
                    ipAddress = addr;
                    break;
                }
            // Didn't find any IPV4 addresses
            if (!foundIPV4)
            {
                // TODO: Indicate an error to the user, as specified in the documentation
                SocketState errorSocket = new SocketState(toCall, "");
                errorSocket.ErrorOccurred = true;
                errorSocket.ErrorMessage = "Didn't find any IPV4 addresses";
                toCall(errorSocket);
            }
        }
        catch (Exception)
        {
            // see if host name is a valid ipaddress
            try
            {
                ipAddress = IPAddress.Parse(hostName);
            }
            catch (Exception e)
            {
                // TODO: Indicate an error to the user, as specified in the documentation
                SocketState errorSocket = new SocketState(toCall, e.Message);
                errorSocket.ErrorOccurred = true;
                errorSocket.ErrorMessage = "Host name is not a valid ipaddress";
                toCall(errorSocket);
            }
        }

        // Create a TCP/IP socket.
        Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // This disables Nagle's algorithm (google if curious!)
        // Nagle's algorithm can cause problems for a latency-sensitive
        // game like ours will be
        socket.NoDelay = true;

        // TODO: Finish the remainder of the connection process as specified.
        IAsyncResult result = socket.BeginConnect(ipAddress, port, ConnectedCallback, new SocketState(toCall, socket));

        //return from begin connect asyncresult timer

        bool timeToConnect = result.AsyncWaitHandle.WaitOne(3000);

        if (timeToConnect == false)
        {
            SocketState errorSocket = new SocketState(toCall, "timeout error");
            toCall(errorSocket);
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
    ///
    /// Uses EndConnect to finalize the connection.
    ///
    /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
    /// either this method or ConnectToServer should indicate the error appropriately.
    ///
    /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
    /// with a new SocketState representing the new connection.
    ///
    /// </summary>
    /// <param name="ar">The object asynchronously passed via BeginConnect</param>
    private static void ConnectedCallback(IAsyncResult ar)
    {
        //new socketstate from the AsyncState of ar
        SocketState socket = (SocketState)ar.AsyncState!;

        try
        {
            //inside the try, call EndConnect, and following call the OnNetworkAction with a new socket
            socket.TheSocket.EndConnect(ar);


            socket.OnNetworkAction(new SocketState(socket.OnNetworkAction, socket.TheSocket));
        }
        catch
        {
            //if couldnt finalize the connection process, create an errorsocket and call networkAction with that new socketstate
            SocketState errorSocket = new SocketState(socket.OnNetworkAction, "Host name is not a valid ipaddress");
            errorSocket.ErrorOccurred = true;
            socket.OnNetworkAction(errorSocket);
        }
    }


    /////////////////////////////////////////////////////////////////////////////////////////
    // Server and Client Common Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback
    /// as the callback to finalize the receive and store data once it has arrived.
    /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
    ///
    /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should
    /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
    /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
    /// in this method or in ReceiveCallback.
    /// </summary>
    /// <param name="state">The SocketState to begin receiving</param>
    public static void GetData(SocketState state)
    {
        try
        {
            //try calling beginReceive on the socket of the socketstate
            state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
        }
        catch
        {
            //if error occurr, set the state flags to the errors and call OnNetworkAction with the state
            state.ErrorOccurred = true;
            state.ErrorMessage = "Error during the receive process";
            state.OnNetworkAction(state);
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
    ///
    /// Uses EndReceive to finalize the receive.
    ///
    /// As stated in the GetData documentation, if an error occurs during the receive process,
    /// either this method or GetData should indicate the error appropriately.
    ///
    /// If data is successfully received:
    ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
    ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its
    ///      string builder.
    ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
    /// </summary>
    /// <param name="ar">
    /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
    /// </param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        //new socketstate from the AsyncState of ar
        SocketState socket = (SocketState)ar.AsyncState!;
        try
        {
            //get the numb from the endReceive method
            int numb = socket.TheSocket.EndReceive(ar);
            //use a lock to make sure the socket append will not have race condition
            lock (socket)
            {
                String messgage = Encoding.UTF8.GetString(socket.buffer, 0, numb);
                socket.data.Append(messgage);
            }
            //after data appendend, call OnNetworkAction with the socket
            socket.OnNetworkAction(socket);
        }
        catch
        {
            //change the flags calling onNetworkAction with socket
            socket.ErrorOccurred = true;
            socket.ErrorMessage = "Error during the receive process";
            socket.OnNetworkAction(socket);
        }
    }

    /// <summary>
    /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
    ///
    /// If the socket is closed, does not attempt to send.
    ///
    /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
    /// </summary>
    /// <param name="socket">The socket on which to send the data</param>
    /// <param name="data">The string to send</param>
    /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
    public static bool Send(Socket socket, string data)
    {
        //get the bytes from data to a messageBuffer
        byte[] messagebuffer = Encoding.UTF8.GetBytes(data);

        //check if socket is connected
        if (!socket.Connected)
        {
            return false;
        }
        //callBeginSend on the socket and return true afterwards
        try
        {
            socket.BeginSend(messagebuffer, 0, messagebuffer.Length, SocketFlags.None, SendCallback, socket);
            return true;
        }
        //if an error occur, shutdown and call close on the socket
        catch
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            return false;
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a send operation that was initiated by Send.
    ///
    /// Uses EndSend to finalize the send.
    ///
    /// This method must not throw, even if an error occurred during the Send operation.
    /// </summary>
    /// <param name="ar">
    /// This is the Socket (not SocketState) that is stored with the callback when
    /// the initial BeginSend is called.
    /// </param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState!;
        try
        {
            //just call endSend on the socket
            socket.EndSend(ar);
        }
        catch
        {

        }
    }


    /// <summary>
    /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
    /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
    ///
    /// If the socket is closed, does not attempt to send.
    ///
    /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
    /// </summary>
    /// <param name="socket">The socket on which to send the data</param>
    /// <param name="data">The string to send</param>
    /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
    public static bool SendAndClose(Socket socket, string data)
    {

        byte[] messagebuffer = Encoding.UTF8.GetBytes(data);
        if (!socket.Connected)
        {
            //if socket is not connected, shutdown, close socket and return false
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            return false;
        }
        try
        {
            //if socket is connected, beginSend the data and return true
            socket.BeginSend(messagebuffer, 0, messagebuffer.Length, SocketFlags.None, SendAndCloseCallback, socket);
            return true;
        }
        catch
        {
            //if error occured, shutdown and close is called
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            return false;
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
    ///
    /// Uses EndSend to finalize the send, then closes the socket.
    ///
    /// This method must not throw, even if an error occurred during the Send operation.
    ///
    /// This method ensures that the socket is closed before returning.
    /// </summary>
    /// <param name="ar">
    /// This is the Socket (not SocketState) that is stored with the callback when
    /// the initial BeginSend is called.
    /// </param>
    private static void SendAndCloseCallback(IAsyncResult ar)
    {
        SocketState socket = (SocketState)ar.AsyncState!;
        try
        {
            //call endSend on the socketstate socket to finalize the operation and close it afterwards
            socket.TheSocket.EndSend(ar);
            socket.TheSocket.Close();
        }
        catch
        {
            //if an error is found during the endSend operation, the socket will not perform it and just close 
            socket.TheSocket.Close();

        }

    }
}

