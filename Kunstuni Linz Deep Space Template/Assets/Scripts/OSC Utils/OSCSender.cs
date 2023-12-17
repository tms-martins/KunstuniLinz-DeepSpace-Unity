/*
 * Tiago Martins 2023
 * Heavily based on UnityOSC by Thomas Fredericks
 * https://thomasfredericks.github.io/UnityOSC/
 * https://github.com/thomasfredericks/UnityOSC
 */

using System.Collections;
using System.Net.Sockets;
using UnityEngine;

namespace OSCUtils
{
    public class OSCSender : MonoBehaviour
    {
        [Tooltip("The destination port for OSC messages")]
        [SerializeField] protected int oscPort = 3333;

        [Tooltip("The destination IP address for OSC messages (use 127.0.0.1 for local machine)")]
        [SerializeField] protected string oscIP = "127.0.0.1";

        [Tooltip("When true, the OSC channel will be opened automatically on Start() or OnEnable()")]
        [SerializeField] protected bool openOnStart = true;

        [Tooltip("The maximum size in bytes for the UDP packet. If your OSC messages have a lot of values, you may need to increase this number.")]
        [SerializeField] protected int maxUdpPacketSize = 512;

        // The UdpClient used for communication, will be initialized when opening the UDP port.
        protected UdpClient udpClient = null;

        void Start()
        {
            if (openOnStart) Open();
        }

        public void OnEnable()
        {
            if (openOnStart) Open();
        }

        public void OnDisable()
        {
            Close();
        }

        void OnDestroy()
        {
            Close();
        }

        public void SetOscIP(string newIP)
        {
            Debug.Log($"{GetType().Name}.SetOscIP(): changing IP from {oscIP} to {newIP}");
            oscIP = newIP;
        }

        public void SetOSCPort(int newPort)
        {
            Debug.Log($"{GetType().Name}.SetOSCPort(): changing port from {oscPort} to {newPort}");
            oscPort = newPort;
        }

        public void Open()
        {
            if (udpClient == null)
            {
                Debug.Log($"{GetType().Name}.Open(): creating new UDP client");
                udpClient = new UdpClient();
            }
        }

        public void Close()
        {
            if (udpClient != null)
            {
                Debug.Log($"{GetType().Name}.Close(): closing UDP client");
                udpClient.Close();
                udpClient = null;
            }
        }

        /// <summary>
        /// Send an individual OSC message.  Internally takes the OscMessage object and 
        /// serializes it into a byte[] suitable for sending to the PacketIO.
        /// </summary>
        /// <param name="oscMessage">The OSC Message to send.</param>   
        public void Send(OSCMessage oscMessage)
        {
            if (udpClient == null)
            {
                Debug.LogWarning($"{GetType().Name}.Send(): UDP client is not initialized, message won't be sent");
                return;
            }

            byte[] packet = new byte[maxUdpPacketSize];
            int length = OSCMessage.OscMessageToPacket(oscMessage, packet, maxUdpPacketSize);
            udpClient.Send(packet, length, oscIP, oscPort);
        }

        /// <summary>
        /// Sends a list of OSC Messages.  Internally takes the OscMessage objects and 
        /// serializes them into a byte[] suitable for sending to the PacketExchange.
        /// </summary>
        /// <param name="oscMessageList">The OSC Message to send.</param>   
        public void Send(ArrayList oscMessageList)
        {
            if (udpClient == null)
            {
                Debug.LogWarning($"{GetType().Name}.Send(): UDP client is not initialized, messages won't be sent");
                return;
            }

            byte[] packet = new byte[maxUdpPacketSize];
            int length = OSCMessage.OscMessagesToPacket(oscMessageList, packet, maxUdpPacketSize);
            udpClient.Send(packet, length, oscIP, oscPort);
        }
    }
}


