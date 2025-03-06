using System.Net.Sockets;
using System.Net;
using System.Text;

public class StunClient
{
    private const string StunServer = "stun.l.google.com";
    private const int StunPort = 19302;

    public static (string PublicIP, int PublicPort) GetPublicEndpoint()
    {
        using (UdpClient udpClient = new UdpClient(StunServer, StunPort))
        {
            byte[] stunRequest = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x21, 0x12, 0xA4, 0x42 };
            udpClient.Send(stunRequest, stunRequest.Length);

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] stunResponse = udpClient.Receive(ref remoteEndPoint);

            // Parse the STUN response to get the public IP and port
            string publicIP = ParseStunResponse(stunResponse);
            int publicPort = remoteEndPoint.Port;

            return (publicIP, publicPort);
        }
    }

    private static string ParseStunResponse(byte[] response)
    {
        // Parse the STUN response to extract the public IP
        // This is a simplified example; a full STUN implementation would require proper parsing
        return $"{response[12]}.{response[13]}.{response[14]}.{response[15]}";
    }
}