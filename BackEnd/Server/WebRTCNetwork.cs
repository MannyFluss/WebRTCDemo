using Godot;
using System;

public partial class WebRTCNetwork : Node
{
    private WebRtcPeerConnection peerConnection;
    private WebRtcDataChannel dataChannel; 

    public override void _Ready()
    {
        // Initialize WebRTC peer connection
        peerConnection = new WebRtcPeerConnection();

        // Set up event handlers
        peerConnection.SessionDescriptionCreated += OnSessionDescriptionCreated;
        peerConnection.IceCandidateCreated += OnIceCandidateCreated;
        

        // Create a data channel
        dataChannel = peerConnection.CreateDataChannel("data", new Godot.Collections.Dictionary());
        //dataChannel.Reciev += OnDataReceived;
    }

    private void OnIceCandidateCreated(string media, long index, string name)
    {
        throw new NotImplementedException();
    }


    private void OnSessionDescriptionCreated(string type, string sdp)
    {
        throw new NotImplementedException();
    }
}
