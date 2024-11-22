// Authors: Kopta, 2019; Martin, 2022
// SocketState for CS 3500 networking library (part of final project)
// University of Utah

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil;

/// <summary>
/// A SocketState is a class for representing one network connection
/// and all its related state
/// </summary>
public class SocketState
{
    public readonly Socket TheSocket;                   // The socket
    public const int BufferSize = 4096;                 // Size of receive buffer
    internal byte[] buffer = new byte[BufferSize];      // Receive buffer
    internal StringBuilder data = new StringBuilder();  // Unprocessed data

    /// <summary>
    /// A message indicating the nature of an error, if one occurred
    /// </summary>
    public string? ErrorMessage
    {
        get;
        internal set;
    }

    /// <summary>
    /// If an error occurs during a network operation, this flag must
    /// be set to true before invoking the user's OnNetworkAction delegate.
    /// The user of the SocketState should check this flag in their
    /// OnNetworkAction delegates.
    /// </summary>
    public bool ErrorOccurred
    {
        get;
        internal set;
    }

    /// <summary>
    /// An identifier associated with the connection
    /// </summary>
    public readonly long ID;
    private static long nextID = 0;
    private static object mutexForID = new object();

    /// <summary>
    /// Function to call when data is received or when a connection is made.
    /// The OnNetworkAction function allows the same network code to be utilized
    /// by a chain of handling methods. For example, there may be a main
    /// gameplay data handling routine, but you may want to have a different
    /// initial handshake routine. By swapping out this function, you
    /// don't have to change the network code
    /// </summary>
    public Action<SocketState> OnNetworkAction;

    /// <summary>
    /// Simple constructor
    /// </summary>
    /// <param name="toCall">The action to take when network activity occurs</param>
    /// <param name="s">The socket</param>
    public SocketState(Action<SocketState> toCall, Socket s)
    {
        OnNetworkAction = toCall;
        TheSocket = s;
        ErrorOccurred = false;
        lock (mutexForID)
        {
            ID = nextID++;
        }
    }

    /// <summary>
    /// Simple constructor for erroneous sockets. Sets flags and messages to
    /// indicate an error state.
    /// </summary>
    /// <param name="toCall">The action to take when network activity occurs</param>
    /// <param name="errorMsg">Error message</param>
    public SocketState(Action<SocketState> toCall, string errorMsg)
    {
        // The socket and action will never be used.
        OnNetworkAction = toCall;
        TheSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        ErrorOccurred = true;
        ErrorMessage = errorMsg;
        lock (mutexForID)
        {
            ID = nextID++;
        }
    }

    /// <summary>
    /// Returns the unprocessed data the SocketState has received so far,
    /// in a thread-safe manner.
    /// </summary>
    /// <returns></returns>
    public string GetData()
    {
        string retval;
        lock (data)
        {
            retval = data.ToString();
        }
        return retval;
    }

    /// <summary>
    /// Removes data from the SocketState's unprocessed data buffer in a thread-safe manner.
    /// Call this after processing data in your OnNetworkAction methods.
    /// </summary>
    /// <param name="start">The index of the first character to remove</param>
    /// <param name="length">The length of the string to remove, starting at "start"</param>
    public void RemoveData(int start, int length)
    {
        lock (data)
        {
            data.Remove(start, length);
        }
    }
}
