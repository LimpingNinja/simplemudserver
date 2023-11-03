using System.Net.Sockets;
using System.Text;
using zlib;
namespace SimpleMud;

public class Socket
{
    
    public enum TelnetCommand
    {
        EOR = 239,  // End of Record, sent to denote when the server is ready to receive (similar to GA)
        SE = 240,   // End of subnegotiation parameters.
        AYT = 246,  // Are you there. Sent by the NVT inquiring as to the status of the remote end.
        GA = 249,   // Go ahead. Used, under certain circumstances, to tell the remote end that it can transmit.
        SB = 250,   // Indicates that what follows is subnegotiation of the indicated option.
        WILL = 251, // Indicates the desire to begin performing, or confirmation that you are now performing, the indicated option.
        WONT = 252, // Indicates the refusal to perform, or continue performing, the indicated option.
        DO = 253,   // Indicates the request that the other party perform, or confirmation that you are expecting the other party to perform, the indicated option.
        DONT = 254, // Indicates the demand that the other party stop performing, or confirmation that you are no longer expecting the other party to perform, the indicated option.
        IAC = 255,  // Data Byte 255. IAC is always followed by a second byte.
    }
    
    public enum TelnetOption
    {
        TTYPE = 24, // Terminal Type
        MCCP2 = 86, // Mud Compression 
        EOR = 25,  // End of Record
        SGA = 3 // Suppress Go Ahead
        // ... add other options here
    }
    
    private enum TelnetState
    {
        Normal,
        IACReceived,
        WillReceived,
        DoReceived,
        WontReceived,
        DontReceived,
        SBReceived,
        TerminalTypeReceived,
    }
    
    public enum OptionState
    {
        NoInterest,
        Will,
        Wont,
        Do,
        Dont
    }

    public TcpClient? Client { get; }
    public NetworkStream Stream { get; }
    public StringBuilder InputBuffer { get; }
    public MemoryStream OutputBuffer { get; }
    private readonly Queue<string> lineBuffer = new Queue<string>();
    public bool IsCompressed { get; private set; }
    private ZOutputStream? compressionStream;
    
    public bool Connected => Client.Connected;
    public Dictionary<TelnetOption, OptionState> OptionStates = new Dictionary<TelnetOption, OptionState>();
    
    private TelnetState _telnetState = TelnetState.Normal;
    private string _lastTerminalType;

    public Socket(TcpClient client)
    {
        Client = client;
        Stream = client.GetStream();
        InputBuffer = new StringBuilder();
        OutputBuffer = new MemoryStream();
        // On connection, send a request for EOR negotiation
        byte[] response = new byte[] {(byte)TelnetCommand.IAC, (byte)TelnetCommand.WILL, (byte)TelnetOption.EOR};
        OutputBuffer.Write(response, 0, response.Length);
        OptionStates[TelnetOption.EOR] = OptionState.Will;
        // On connection, send a request for TTYPE negotiation
        response = new byte[] {(byte)TelnetCommand.IAC, (byte)TelnetCommand.DO, (byte)TelnetOption.TTYPE};
        OutputBuffer.Write(response, 0, response.Length);
        OptionStates[TelnetOption.TTYPE] = OptionState.Do;
        byte[] offerCompression = new byte[] {(byte)TelnetCommand.IAC, (byte)TelnetCommand.WILL, (byte)TelnetOption.MCCP2}; // IAC WILL MCCP2
        OutputBuffer.Write(offerCompression, 0, offerCompression.Length);
    }

    public void Poll(User user)
    {
        // Handle input
        try
        {
            if (Client is not {Client: not null} ||
                (Client.Client.Poll(0, SelectMode.SelectRead) && Client.Client.Available == 0))
            {
                throw new IOException();
            }

            if (Stream.DataAvailable)
            {
                var buffer = new byte[1024];
                int bytesRead = Stream.Read(buffer, 0, buffer.Length);
                // Process the buffer to handle telnet commands
                var processedBuffer = ProcessTelnetCommands(buffer, bytesRead);

                InputBuffer.Append(Encoding.UTF8.GetString(processedBuffer, 0, processedBuffer.Length));
                
                // Check if there are any complete lines in the InputBuffer
                string[] lines = InputBuffer.ToString().Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
                for (int i = 0; i < lines.Length - 1; i++)
                {
                    lineBuffer.Enqueue(lines[i]);
                }

                // Leave the last (potentially incomplete) line in the InputBuffer
                InputBuffer.Clear();
                InputBuffer.Append(lines[lines.Length - 1]);
            }

            // Execute state handler if there is a complete line of input
            if (lineBuffer.Count > 0 && user.StateHandlers.Count > 0)
            {
                var currentStateHandler = user.StateHandlers.Peek();
                currentStateHandler(user);
            }

            // Handle output
            if (OutputBuffer.Length <= 0) return;
            byte[] bytes = OutputBuffer.ToArray();
            Stream.Write(bytes, 0, bytes.Length);
            OutputBuffer.SetLength(0);
        }
        catch (IOException)
        {
            // Before closing the client, close the compression stream if it's active
            compressionStream?.Close();

            // Connection was closed by the client
            Client?.Close();
        }
        catch (ObjectDisposedException)
        {
            // Before closing the client, close the compression stream if it's active
            compressionStream?.Close();

            // Connection was closed by the client
            Client?.Close();
        }
    }

    public string? GetTermType()
    {
        return _lastTerminalType;
    }
    
    private byte[] ProcessTelnetCommands(IReadOnlyList<byte> buffer, int bytesRead)
    {
        List<byte> processedBuffer = new List<byte>();
        for (int i = 0; i < bytesRead; i++)
        {
            byte b = buffer[i];
            switch (_telnetState)
            {
                case TelnetState.Normal:
                    if (b == 255) // IAC
                    {
                        _telnetState = TelnetState.IACReceived;
                    }
                    else
                    {
                        processedBuffer.Add(b);
                    }

                    break;

                case TelnetState.IACReceived:
                    if (b == 251) // WILL
                    {
                        _telnetState = TelnetState.WillReceived;
                    }
                    else if (b == 253) // DO
                    {
                        _telnetState = TelnetState.DoReceived;
                    }
                    else if (b == 254) // DONT
                    {
                        _telnetState = TelnetState.DontReceived;
                    }
                    else if (b == 252) // WONT
                    {
                        _telnetState = TelnetState.WontReceived;
                    }
                    else if (b == 250) // SB
                    {
                        _telnetState = TelnetState.SBReceived;
                    }
                    else
                    {
                        _telnetState = TelnetState.Normal;
                    }

                    break;
                case TelnetState.WillReceived:
                    if (b == 24) // TERM-TYPE
                    {
                        // respond with IAC SB TERM-TYPE SEND IAC SE
                        byte[] response = new byte[] {255, 250, 24, 1, 255, 240};
                        OutputBuffer.Write(response, 0, response.Length);
                    }
                    _telnetState = TelnetState.Normal;
                    break;

                case TelnetState.DoReceived:
                    if (b == (byte)TelnetOption.MCCP2) 
                    {
                        // The client has agreed to use MCCP2. Respond with IAC SB MCCP2 IAC SE and enable compression.
                        byte[] startCompression = new byte[] {255, 250, (byte)TelnetOption.MCCP2, 255, 240}; 
                        OutputBuffer.Write(startCompression, 0, startCompression.Length);

                        // Initialize the zlib compression stream
                        compressionStream = new ZOutputStream(Stream);
                        IsCompressed = true;
                    }
                    else if (b == (byte)TelnetOption.EOR) // EOR
                    {
                        OptionStates[TelnetOption.EOR] = OptionState.Do;
                    }
                    else if (b == (byte)TelnetOption.SGA) // Suppress Go Ahead
                    {
                        byte[] startCompression = new byte[] {(byte)TelnetCommand.IAC, (byte)TelnetCommand.WILL, (byte)TelnetOption.SGA}; 
                        OutputBuffer.Write(startCompression, 0, startCompression.Length);
                        OptionStates[TelnetOption.SGA] = OptionState.Do;
                    }
                    _telnetState = TelnetState.Normal;
                    break;
                
                case TelnetState.DontReceived:
                    if (b == (byte)TelnetCommand.DONT && IsCompressed)
                    {
                        // The client doesn't want to use MCCP2 after all, or there was an error. Stop compression.
                        compressionStream?.finish();
                        IsCompressed = false;
                        compressionStream = null;
                    }
                    else if (b == (byte)TelnetOption.EOR) // EOR
                    {
                        OptionStates[TelnetOption.EOR] = OptionState.Dont;
                    }
                    _telnetState = TelnetState.Normal;
                    break;

                case TelnetState.WontReceived:
                    _telnetState = TelnetState.Normal;
                    break;

                case TelnetState.SBReceived:
                    if (b == 24) // TERM-TYPE
                    {
                        _telnetState = TelnetState.TerminalTypeReceived;
                    }
                    else
                    {
                        _telnetState = TelnetState.Normal;
                    }

                    break;

                case TelnetState.TerminalTypeReceived:
                    if (b == 1) // SEND
                    {
                        // send IAC SB TERM-TYPE SEND IAC SE
                        byte[] response = new byte[] {255, 250, 24, 1, 255, 240};
                        OutputBuffer.Write(response, 0, response.Length);
                    }
                    else if (b == 0) // IS
                    {
                        // read the terminal type
                        i++;
                        StringBuilder terminalType = new StringBuilder();
                        while (i < bytesRead && buffer[i] != 255) // IAC
                        {
                            terminalType.Append((char) buffer[i]);
                            i++;
                        }

                        i++; // skip the IAC
                        if (i < bytesRead && buffer[i] == 240) // SE
                        {
                            // handle the received terminal type
                            if (terminalType.ToString() == _lastTerminalType)
                            {
                                // negotiation is complete
                                // ...
                                OptionStates[TelnetOption.TTYPE] = OptionState.Will;
                            }
                            else
                            {
                                _lastTerminalType = terminalType.ToString();
                                // send IAC SB TERM-TYPE SEND IAC SE
                                byte[] response = new byte[] {255, 250, 24, 1, 255, 240};
                                OutputBuffer.Write(response, 0, response.Length);
                                OptionStates[TelnetOption.TTYPE] = OptionState.Will;
                            }
                        }
                    }

                    _telnetState = TelnetState.Normal;
                    break;
            }
        }

        return processedBuffer.ToArray();
    }

    public void Send(string message)
    {
        var bytes = Utilities.ConvertToAnsi(message);

        if (IsCompressed && compressionStream != null)
        {
            // If we're compressing, write to the compression stream
            compressionStream.Write(bytes, 0, bytes.Length);
            compressionStream.Flush(); // Important to ensure compressed data is sent
        }
        else
        {
            // Otherwise, send as usual
            OutputBuffer.Write(bytes, 0, bytes.Length);
        }
    }
    public void Send(byte[] bytes)
    {
        if (IsCompressed && compressionStream != null)
        {
            // If we're compressing, write to the compression stream
            compressionStream.Write(bytes, 0, bytes.Length);
            compressionStream.Flush(); // Important to ensure compressed data is sent
        }
        else
        {
            // Otherwise, send as usual
            OutputBuffer.Write(bytes, 0, bytes.Length);
        }
    }
    
    public void SendPrompt(string message)
    {
        byte[] bytes = Utilities.ConvertToAnsi($"{message}#n");
        Send(bytes);

        if (OptionStates.TryGetValue(TelnetOption.EOR, out OptionState eorState) && eorState == OptionState.Do)
        {
            Send(new byte[] {(byte)TelnetCommand.IAC, (byte)TelnetCommand.EOR});
        }
        else
        {
            if (!OptionStates.TryGetValue(TelnetOption.SGA, out OptionState sgaState) || sgaState != OptionState.Do)
            {
                Send(new byte[] {(byte)TelnetCommand.IAC, (byte)TelnetCommand.GA});
            }
        }
    }
    
    public string ReadLine()
    {
        if (lineBuffer.Count > 0)
        {
            return lineBuffer.Dequeue();
        }
        return null;
    }
}