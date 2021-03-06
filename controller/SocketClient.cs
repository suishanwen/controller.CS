﻿using controller.util;
using SuperSocket.ClientEngine;
using System;
using System.Threading;
using WebSocket4Net;
namespace controller
{
    public class SocketClient : IDisposable
    {

        #region 向外传递数据事件
        public event Action<string> MessageReceived;
        #endregion

        WebSocket4Net.WebSocket _webSocket;
        /// <summary>
        /// 检查重连线程
        /// </summary>
        Thread _thread;
        bool _isRunning = true;
        /// <summary>
        /// WebSocket连接地址
        /// </summary>
        public string ServerPath { get; set; }

        public SocketClient(string identity)
        {
            ServerPath = $"ws://bitcoinrobot.cn:8051/sw/api/websocket/{identity}";
            this._webSocket = new WebSocket(ServerPath);
            this._webSocket.Opened += WebSocket_Opened;
            this._webSocket.Error += WebSocket_Error;
            this._webSocket.Closed += WebSocket_Closed;
            this._webSocket.MessageReceived += WebSocket_MessageReceived;
        }

        #region "web socket "
        /// <summary>
        /// 连接方法
        /// <returns></returns>
        public bool Start()
        {
            bool result = true;
            try
            {
                Log.writeLogs("./socket.txt", "");
                this._webSocket.Open();

                this._isRunning = true;
                this._thread = new Thread(new ThreadStart(CheckConnection));
                this._thread.Start();
            }
            catch (Exception ex)
            {
                Log.writeLogs("./socket.txt", ex.ToString());
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 消息收到事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            String msg = e.Message;
            Log.writeLogs("./socket.txt", " Received:" + msg);
            MessageReceived?.Invoke(msg);
            if (!StringUtil.isEmpty(msg))
            {
                if (msg.Contains("TASK_SYS"))
                {
                    SocketAction.SYS(msg, true);
                }
                else if (msg.Contains("TASK_PC"))
                {
                    if (msg.Equals(SocketAction.TASK_PC_RAR))
                    {
                        SocketAction.PC_RAR();
                    }
                    else if (msg.Equals(SocketAction.TASK_PC_EPT))
                    {
                        SocketAction.PC_EPT();
                    }
                    else if (msg.Equals(SocketAction.TASK_PC_UPGRADE))
                    {
                        SocketAction.PC_UPGRADE();
                    }
                }
                else if (msg.Contains("FORM1"))
                {
                    if (msg.Contains("FORM1_VM"))
                    {
                        int type = int.Parse(msg.Substring(8, 1));
                        string[] arr = msg.Split(':');
                        string val = arr.Length == 2 ? arr[1] : "";
                        SocketAction.VM(type, val);
                    }
                    else if (msg.Contains("FORM1_WORKER"))
                    {
                        string[] arr = msg.Split(':');
                        string val = arr.Length == 2 ? arr[1] : "";
                        int type = 0;
                        if (SocketAction.FORM1_WORKER_SET.Equals(arr[0]))
                        {
                            type = 1;
                        }
                        else if (SocketAction.FORM1_WORKER_INPUT.Equals(arr[0]))
                        {
                            type = 2;
                        }
                        else if (SocketAction.FORM1_WORKER_TAIL.Equals(arr[0]))
                        {
                            type = 3;
                        }
                        if (type > 0)
                        {
                            SocketAction.WORKER_SET(type, val);
                        }
                    }
                    else if (msg.Contains("FORM1_TIMEOUT"))
                    {
                        string[] arr = msg.Split(':');
                        string val = arr.Length == 2 ? arr[1] : "";
                        if (!StringUtil.isEmpty(val))
                        {
                            SocketAction.TIMEOUT_SET(val);
                        }
                    }
                }
                else if (msg.Contains("AUTO_VOTE_SET"))
                {
                    int type = int.Parse(msg.Substring(13, 1));
                    string[] arr = msg.Split(':');
                    string val = arr.Length == 2 ? arr[1] : "";
                    SocketAction.AUTO_VOTE_SET(type, val);
                }
                else if (msg.Contains("TASK_EXEC_REPLUG"))
                {
                    string[] arr = msg.Split(':');
                    string val = arr.Length == 2 ? arr[1] : "";
                    SocketAction.EXEC_REPLUG(val);
                }
                else if (msg.Contains("AUTO_VOTE_INDEX"))
                {
                    string[] arr = msg.Split(':');
                    string method = arr[0];
                    string val = arr.Length == 2 ? arr[1] : "";
                    if (method.Equals(SocketAction.AUTO_VOTE_INDEX_SELECT))
                    {
                        SocketAction.AUTO_VOTE_SELECT_INDEX(int.Parse(val));
                    }
                    else if (method.Equals(SocketAction.AUTO_VOTE_INDEX_NAME_START))
                    {
                        SocketAction.AUTO_VOTE_START_NAME_INDEX(val);
                    }
                }
                else if (msg.Contains("DROP_PROJECT"))
                {
                    string val =  msg.Split(':')[1];
                    string[] valInfo = val.Split('|');
                    SocketAction.Drop_Project(valInfo[0], bool.Parse(valInfo[1]));
                }
                else if (msg.Contains("TOP_PROJECT"))
                {
                    string val = msg.Split(':')[1];
                    string[] valInfo = val.Split('|');
                    SocketAction.Top_Project(valInfo[0], bool.Parse(valInfo[1]));
                }
                else if (msg.Contains("REPORT"))
                {
                    int type = msg.Equals(SocketAction.REPORT_STATE) ? 1 : msg.Equals(SocketAction.REPORT_STATE_LESS) ? 2 : msg.Equals(SocketAction.REPORT_STATE_VOTE) ? 3 : 4;
                    SocketAction.REPORT(type);
                }else if (msg.Contains("TASK_VOTE_PROJECT"))
                {
                    SocketAction.Vote(msg.Split(':')[1]);
                }
            }
        }
        /// <summary>
        /// Socket关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Closed(object sender, EventArgs e)
        {
            Log.writeLogs("./socket.txt", "websocket_Closed");
        }
        /// <summary>
        /// Socket报错事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Error(object sender, ErrorEventArgs e)
        {
            Log.writeLogs("./socket.txt", "websocket_Error:" + e.Exception.ToString());
        }
        /// <summary>
        /// Socket打开事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WebSocket_Opened(object sender, EventArgs e)
        {
            Log.writeLogs("./socket.txt", " websocket_Opened");
        }
        /// <summary>
        /// 检查重连线程
        /// </summary>
        private void CheckConnection()
        {
            do
            {
                try
                {
                    if (this._webSocket.State != WebSocket4Net.WebSocketState.Open && this._webSocket.State != WebSocket4Net.WebSocketState.Connecting)
                    {
                        Log.writeLogs("./socket.txt", " Reconnect websocket WebSocketState:" + this._webSocket.State);
                        this._webSocket.Close();
                        this._webSocket.Open();
                        Console.WriteLine("正在重连");
                    }
                }
                catch (Exception ex)
                {
                    Log.writeLogs("./socket.txt", ex.ToString());
                }
                System.Threading.Thread.Sleep(5000);
            } while (this._isRunning);
        }
        #endregion

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Message"></param>
        //public void SendMessage(string Message)
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        if (_webSocket != null && _webSocket.State == WebSocket4Net.WebSocketState.Open)
        //        {
        //            this._webSocket.Send(Message);
        //        }
        //    });
        //}

        public void Dispose()
        {
            this._isRunning = false;
            try
            {
                _thread.Abort();
            }
            catch
            {

            }
            this._webSocket.Close();
            this._webSocket.Dispose();
            this._webSocket = null;
        }
    }
}
