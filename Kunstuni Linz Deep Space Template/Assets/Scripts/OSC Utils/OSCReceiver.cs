/*
 * Tiago Martins 2023
 * Heavily based on UnityOSC by Thomas Fredericks
 * https://thomasfredericks.github.io/UnityOSC/
 * https://github.com/thomasfredericks/UnityOSC
 */

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace OSCUtils
{
    public class OSCReceiver : MonoBehaviour
    {
        [Tooltip("The incoming port for OSC messages")]
        [SerializeField] protected int oscPort = 3333;

        [Tooltip("When true, the OSC channel will be opened automatically on Start() or OnEnable()")]
        [SerializeField] protected bool openOnStart = true;

        [Tooltip("The maximum size in bytes for the UDP packet. If your OSC messages have a lot of values, you may need to increase this number.")]
        [SerializeField] protected int maxUdpPacketSize = 512;

        // The UdpClient used for communication, will be initialized when opening the UDP port.
        protected UdpClient udpClient = null;

        // Determines how long the UDP reader thread sleeps between iterations
        static int READER_THREAD_SLEEP_MILLIS = 5;

        // Message handling
        protected OSCMessageHandler AllMessageHandler;
        protected Hashtable AddressTable;
        protected ArrayList messagesReceived;
        byte[] buffer;

        // Threading
        protected Thread readThread = null;
        protected bool readerRunning = false;        
        protected object readThreadLock = new object();
        protected bool paused = false;

        public delegate void OSCMessageHandler(OSCMessage oscMessage);

        void Awake()
        {
            AddressTable = new Hashtable();
            messagesReceived = new ArrayList();
            buffer = new byte[maxUdpPacketSize];

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;  //FIX FOR UNITY POST 2017
#endif
        }

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

        public void SetOSCPort(int newPort)
        {
            Debug.Log($"{GetType().Name}.SetOSCPort(): changing port from {oscPort} to {newPort}");
            oscPort = newPort;

            if (udpClient != null) 
            {
                StopReadThread();

                Debug.Log($"{GetType().Name}.SetOSCPort(): closing UDP client");
                udpClient.Close();
                udpClient = null;

                Debug.Log($"{GetType().Name}.SetOSCPort(): creating new UDP client");
                IPEndPoint listenerIp = new IPEndPoint(IPAddress.Any, oscPort);
                udpClient = new UdpClient(listenerIp);

                StartCoroutine(StartThreadAfterSeconds(1f));
            }
        }

        /// <summary>
        /// Set the method to call back on when a message with the specified
        /// address is received.  The method needs to have the OscMessageHandler signature - i.e. 
        /// void amh( OscMessage oscM )
        /// </summary>
        /// <param name="key">Address string to be matched</param>   
        /// <param name="messageHandler">The method to call back on.</param>   
        public void SetAddressHandler(string key, OSCMessageHandler messageHandler)
        {
            ArrayList handlersList = (ArrayList)Hashtable.Synchronized(AddressTable)[key];
            if (handlersList == null)
            {
                handlersList = new ArrayList();
                handlersList.Add(messageHandler);
                Hashtable.Synchronized(AddressTable).Add(key, handlersList);
            }
            else
            {
                handlersList.Add(messageHandler);
            }
        }

        public void Open()
        {
            if (udpClient == null)
            {
                Debug.Log($"{GetType().Name}.Open(): creating new UDP client");
                IPEndPoint listenerIp = new IPEndPoint(IPAddress.Any, oscPort);
                udpClient = new UdpClient(listenerIp);
            }

            if (!readerRunning) StartReadThread();
        }

        public void Close()
        {
            StopReadThread();

            if (udpClient != null)
            {
                Debug.Log($"{GetType().Name}.Close(): closing UDP client");
                udpClient.Close();
                udpClient = null;
            }
        }

        void StartReadThread()
        {
            if (readerRunning)
            {
                Debug.Log($"{GetType().Name}.StartReadThread(): reader thread is still running! Killing thread before creating a new one.");
                if (readThread != null)
                {
                    readThread.Abort();
                }
            }

            readThread = new Thread(Read);
            readThread.IsBackground = true;
            readerRunning = true;
            readThread.Start();
        }

        IEnumerator StartThreadAfterSeconds(float seconds)
        {
            Debug.Log($"{GetType().Name}.StartThreadAfterSeconds(): on port {oscPort} after {seconds:0.00} seconds");
            yield return new WaitForSeconds(seconds);
            StartReadThread();
        }

        void StopReadThread()
        {
            if (readerRunning)
            {
                Debug.Log($"{GetType().Name}.StopReadThread(): stopping thread...");
                readerRunning = false;
            }
        }

        private void Read()
        {
            try
            {
                while (readerRunning)
                {
                    int length = ReceivePacket(buffer);

                    if (length > 0)
                    {
                        lock (readThreadLock)
                        {
                            if (!paused)
                            {
                                ArrayList newMessages = OSCMessage.PacketToOscMessages(buffer, length);
                                messagesReceived.AddRange(newMessages);
                            }
                        }
                    }
                    else
                        Thread.Sleep(READER_THREAD_SLEEP_MILLIS);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"{GetType().Name}.Read(): ThreadException " + e);
            }
            finally
            {
            }
            Debug.Log($"{GetType().Name}.Read(): thread exiting.");
        }

        /// <summary>
        /// Receive a packet of bytes over UDP.
        /// </summary>
        /// <param name="buffer">The buffer to be read into.</param>
        /// <returns>The number of bytes read, or 0 on failure.</returns>
        public int ReceivePacket(byte[] buffer)
        {
            if (udpClient == null)
            {
                Debug.LogError($"{GetType().Name}.ReceivePacket(): UDP client is not initialized.");
                return 0;
            }

            IPEndPoint iep = new IPEndPoint(IPAddress.Any, oscPort);
            byte[] incoming = udpClient.Receive(ref iep);
            int count = Math.Min(buffer.Length, incoming.Length);
            Array.Copy(incoming, buffer, count);
            return count;
        }

        void Update()
        {
            if (messagesReceived.Count > 0)
            {
                lock (readThreadLock)
                {
                    foreach (OSCMessage oscMessage in messagesReceived)
                    {
                        if (AllMessageHandler != null) AllMessageHandler(oscMessage);

                        ArrayList handlersList = (ArrayList)Hashtable.Synchronized(AddressTable)[oscMessage.address];
                        if (handlersList != null)
                        {
                            foreach (OSCMessageHandler messageHandler in handlersList)
                            {
                                messageHandler(oscMessage);
                            }
                        }
                    }
                    messagesReceived.Clear();
                }
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
#if !UNITY_EDITOR
		paused = pauseStatus;
		Debug.Log($"{GetType().Name}.OnApplicationPause(): paused is {pauseStatus}");
#endif
        }

#if UNITY_EDITOR
        private void HandleOnPlayModeChanged(UnityEditor.PlayModeStateChange state) //FIX FOR UNITY POST 2017
        {
            // This method is run whenever the playmode state is changed.
            paused = UnityEditor.EditorApplication.isPaused;
        }
#endif
    }
}


