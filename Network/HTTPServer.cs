using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace BeatSaberPlus_HTTPHook.Network
{
    internal class HTTPServer
    {
        private HttpListener m_Listener;
        private Thread m_Thread;
        private volatile bool m_Running;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal event Action<string> OnHookReceived;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal void Start(int p_Port)
        {
            if (m_Running)
                return;

            try
            {
                m_Listener = new HttpListener();
                m_Listener.Prefixes.Add($"http://+:{p_Port}/");
                m_Listener.Start();

                m_Running = true;

                m_Thread = new Thread(ListenLoop)
                {
                    IsBackground = true,
                    Name = "HTTPHook_Server"
                };
                m_Thread.Start();

                Logger.Instance.Info($"[HTTPServer] Started on port {p_Port}");
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error($"[HTTPServer] Failed to start on port {p_Port}");
                Logger.Instance.Error(l_Exception);
                m_Running = false;
            }
        }

        internal void Stop()
        {
            if (!m_Running)
                return;

            m_Running = false;

            try
            {
                m_Listener?.Stop();
                m_Listener?.Close();
            }
            catch { }

            try
            {
                m_Thread?.Join(2000);
            }
            catch { }

            m_Listener = null;
            m_Thread = null;

            Logger.Instance.Info("[HTTPServer] Stopped");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void ListenLoop()
        {
            while (m_Running)
            {
                try
                {
                    var l_Context = m_Listener.GetContext();
                    ProcessRequest(l_Context);
                }
                catch (HttpListenerException)
                {
                    if (!m_Running)
                        break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception l_Exception)
                {
                    Logger.Instance.Error("[HTTPServer] Error processing request");
                    Logger.Instance.Error(l_Exception);
                }
            }
        }

        private void ProcessRequest(HttpListenerContext p_Context)
        {
            var l_Request  = p_Context.Request;
            var l_Response = p_Context.Response;

            try
            {
                var l_Path = l_Request.Url.AbsolutePath.TrimEnd('/');

                if (!l_Path.StartsWith("/hook/", StringComparison.OrdinalIgnoreCase))
                {
                    SendResponse(l_Response, 404, "{\"error\":\"Not found. Use POST or GET /hook/{hookName}\"}");
                    return;
                }

                var l_HookName = l_Path.Substring("/hook/".Length);

                if (string.IsNullOrEmpty(l_HookName))
                {
                    SendResponse(l_Response, 400, "{\"error\":\"Hook name is required.\"}");
                    return;
                }

                Logger.Instance.Info($"[HTTPServer] Hook received: {l_HookName}");

                OnHookReceived?.Invoke(l_HookName);

                SendResponse(l_Response, 200, "{\"ok\":true}");
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error("[HTTPServer] Error in ProcessRequest");
                Logger.Instance.Error(l_Exception);

                try { SendResponse(l_Response, 500, "{\"error\":\"Internal server error\"}"); }
                catch { }
            }
        }

        private void SendResponse(HttpListenerResponse p_Response, int p_StatusCode, string p_Body)
        {
            p_Response.StatusCode = p_StatusCode;
            p_Response.ContentType = "application/json";
            p_Response.Headers.Add("Access-Control-Allow-Origin", "*");

            var l_Buffer = Encoding.UTF8.GetBytes(p_Body);
            p_Response.ContentLength64 = l_Buffer.Length;
            p_Response.OutputStream.Write(l_Buffer, 0, l_Buffer.Length);
            p_Response.OutputStream.Close();
        }
    }
}
