
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;


//这个脚本是hololens端的SocketUDP脚本，提供发送方法，初始化并开启接收方法
public class Community : MonoBehaviour
{
    public GameObject quad;
    public GameObject photobtn;
    public Sprite jujiao;
    public Sprite daiji;
    private Text lMessage;
    string openpicname = null;

    //修改时间用的变量
    public Text[] D=new Text[3];
    public int flag = 0;
    DateTime da = DateTime.Now;  

    private VideoPlayer videoplayer;
    PhotoCaptureFinal PCF = new PhotoCaptureFinal();//直接调用PhotoCaptureFinal的

    Socket socket; //目标socket
    //发送端口
    EndPoint serverEnd;
    IPEndPoint ipEnd;
    //接收端口
    IPEndPoint IPLocalPoint;
    //发送用的socket异步参数
    SocketAsyncEventArgs socketAsyceArgs;
    //接收用的socket异步参数
    SocketAsyncEventArgs reciveArgs;
    //接收SAEA用来接收的缓冲区
    byte[] reciveArgsBuffer;
    //初始化
    void InitSocket()
    {
        //定义连接的服务器ip和端口，可以是本机ip，局域网，互联网，和服务端的要一致
        ipEnd = new IPEndPoint(IPAddress.Parse("192.168.1.12"), 8781);
        //初始化要接收的IP，IPAddress.Any表示接收所有IP地址发来的字节流
        IPLocalPoint = new IPEndPoint(IPAddress.Any, 8002);
        //初始化socket
        socket = new Socket(IPLocalPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        //定义服务端
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        //初始化发送用的SAEA
        socketAsyceArgs = new SocketAsyncEventArgs();
        //设置发送用的SAEA的IP
        socketAsyceArgs.RemoteEndPoint = ipEnd;
        //初始化接收用的SAEA的缓冲区，此处我设为10K
        reciveArgsBuffer = new byte[1024 * 10];
        //初始化接收SAEA
        reciveArgs = new SocketAsyncEventArgs();
        //设置接收SAEA的接收IP地址
        reciveArgs.RemoteEndPoint = IPLocalPoint;
        //因为SAEA系列API 是异步方法，所以设置好完成方法后的回调
        reciveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedRecive);
        //设置接收缓冲区
        reciveArgs.SetBuffer(reciveArgsBuffer, 0, reciveArgsBuffer.Length);
    }

    //异步方法完成后的complete时间
    private void CompletedRecive(object sender, SocketAsyncEventArgs e)
    {
        //通过SAEA.LastOperation这个枚举来判断完成的是什么方法，对应不同的操作
        switch (reciveArgs.LastOperation)
        {
            //因为reciveArgs是我专门用来接收的SAEA,所以这里只设置一个完成接收后用的方法
            case SocketAsyncOperation.ReceiveFrom:
                PocessReceiveFrom(e);
                break;
        }
    }



    //中转缓冲区，将数据拷贝出来给主线程用
    byte[] tempBytes;
    //用来通知主线程的参数
    bool isOk = false;

    //注意：处理这个方法是辅线程，不要用Unity的类，否则报错，将收到的字节流拷贝出来，通知主线程来处理
    //接收完成后对应的处理方法
    public void PocessReceiveFrom(SocketAsyncEventArgs e)
    {

        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            //这里会造成内存垃圾以及内存碎片化，如果频繁的长时间的接收，建议做一个Byte池。
            tempBytes = new byte[e.BytesTransferred];     //将数据拷贝出来保证可以复用
            Array.Copy(e.Buffer, e.Offset, tempBytes, 0, tempBytes.Length);
            //通知主线程
            isOk = true;
        }
    }

    /// <summary>
    /// 异步发送消息方法
    /// </summary>
    /// <param name="bytes"></param>
    public void AsyncSend(byte[] bytes)
    {
        //设置缓冲区，缓冲区里是发送的字节流
        socketAsyceArgs.SetBuffer(bytes, 0, bytes.Length);
        //Debug.Log("socket异步参数字节流长度 " + socketAsyceArgs.Buffer.Length);
        bool bo = socket.SendToAsync(socketAsyceArgs);
        if (!bo)
        {
            //在hololens上发现过一段时间scoket就不会发送数据，最后这样处理：判断SentToAsync方法失败后，就重新new一个SAEA，解决socket发送失败的问题
            //注意初始化一个SAEA时，1.IP    2.缓冲区，3.完成后的回调事件  这三个都是必要的，
            socketAsyceArgs = new SocketAsyncEventArgs();
            socketAsyceArgs.RemoteEndPoint = ipEnd;
        }
    }

    //初始化socket并测试一下
    private void Start()
    {
        lMessage = GameObject.Find("Canvas/Text").GetComponent<Text>();
        InitSocket();
        TestSocekt();

        D[0]= GameObject.Find("Canvas/year").GetComponent<Text>();
        D[1] = GameObject.Find("Canvas/month").GetComponent<Text>();
        D[2] = GameObject.Find("Canvas/day").GetComponent<Text>();
        D[0].gameObject.SetActive(false);
        D[1].gameObject.SetActive(false);
        D[2].gameObject.SetActive(false);
    }

    //用来测试socket的方法，发送一个信息
    void TestSocekt()
    {
        //int tempInt = 15;
        byte[] tempBytes = Encoding.UTF8.GetBytes("hello");
        //tempBytes = BitConverter.GetBytes(tempInt);

        AsyncSend(tempBytes);
    }
   
    private void Update()
    {
        if (isOk)
        {
            //对tempBytes进行处理
            string temp = Encoding.UTF8.GetString(tempBytes, 0, tempBytes.Length);
            Debug.Log("接收socket,接收到了字节流，接收到的数字为 " + temp);
            
            lMessage.text = temp;
            manage(temp);

            //每接受完就发一次消息
            //byte[] tempByte = Encoding.UTF8.GetBytes("ok");
            //tempBytes = BitConverter.GetBytes(tempInt);
            //AsyncSend(tempByte);

            isOk = false;
        }
    }

    void manage(string str)//检测电脑传来的指令，进行判断
    {
        if (str.Equals("open"))//开机
        {
            if (StatusManager.Instance.currentstatus == CurrentStatus.Close)
            {
                GameObject g = GameObject.FindGameObjectWithTag("open");
                StatusManager.Instance.currentstatus = CurrentStatus.Ready;
                g.GetComponent<StatusManager>().OpenCamera();
            }
        }
        else if (str.Equals("close"))//关机
        {
            if (StatusManager.Instance.currentstatus != CurrentStatus.Close)
            {
                quad.SetActive(false);//
                ModelManager.Instance.imageObject.SetActive(false);
                GameObject g = GameObject.FindGameObjectWithTag("open");
                StatusManager.Instance.currentstatus = CurrentStatus.Close;
                g.GetComponent<StatusManager>().cameraIsColosed();
            }
        }
        else if (str.Equals("take"))//拍照
        {
            GameObject g = GameObject.FindGameObjectWithTag("shutter");
            StatusManager.Instance.SetStatus(CurrentStatus.Ready);
            quad.SetActive(false);
            ModelManager.Instance.imageObject.SetActive(true);
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            g.GetComponent<PhotoCaptureFinal>().ReadyToPhoto();
        }
        else if (str.Equals("play"))//查看照片
        {
            GameObject g = GameObject.FindGameObjectWithTag("play");
            StatusManager.Instance.SetStatus(CurrentStatus.ReadPhoto);
            ModelManager.Instance.imageObject.SetActive(true);
            quad.SetActive(false);
            openpicname = string.Format(@"CapturedImage{0}.jpg", PhotoCaptureFinal.openpicnum);
            PhotoCaptureFinal.openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
            g.GetComponent<PhotoCaptureFinal>().ShowPhoto(openpicname, PhotoCaptureFinal.openpicpath);
        }
        else if (str.Equals("next"))//下一张
        {
            GameObject g = GameObject.FindGameObjectWithTag("back");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                g.GetComponent<PhotoCaptureFinal>().NextPhoto();
            }
        }
        else if (str.Equals("previous"))//上一张
        {
            GameObject g = GameObject.FindGameObjectWithTag("back");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                g.GetComponent<PhotoCaptureFinal>().BackPhoto();
            }
        }
        else if (str.Equals("remove"))
        {

            Debug.Log("remove");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                openpicname = string.Format(@"CapturedImage{0}.jpg", PhotoCaptureFinal.openpicnum);
                PhotoCaptureFinal.openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                File.Delete(PhotoCaptureFinal.openpicpath);
                if (PhotoCaptureFinal.openVdieonum > 1)
                {
                    PCF.BackPhoto();
                }
                else if (PhotoCaptureFinal.openVdieonum == 1)
                {
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
                    //ModelManager.Instance.SetTipText("Nothing.Please air tap to take photos");
                }
                PhotoCaptureFinal.sumpic--;
            }
            else if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
            {

                string filename = string.Format("TestVideo_{0}.mp4", PhotoCaptureFinal.openVdieonum);
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                File.Delete(filepath);
                if (PhotoCaptureFinal.openVdieonum > 1)
                {
                    PCF.PreviousVideo();
                }
                else if (PhotoCaptureFinal.openVdieonum == 1)
                {
                    quad.GetComponent<Image>().sprite = null;
                    //ModelManager.Instance.SetTipText("Nothing.Please air tap to record videos");
                }
            }
        }
        //以下是时间修改相关
        else if(str.Equals("time"))
        {
            StatusManager.Instance.SetStatus(CurrentStatus.ChangeTime);
            showtime();
                        
        }
        else if (str.Equals("right"))
        {
            timeadd();
          
        }
        else if (str.Equals("left"))
        {
            timedecrease();
           
        }
        else if (str.Equals("up"))
        {
            dateadd();
            
        }
        else if (str.Equals("down"))
        {
            datedecrease();
            
        }
        /*
        else if (str.Equals("remove"))
        {

            Debug.Log("remove");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                openpicname = string.Format(@"CapturedImage{0}.jpg", PhotoCaptureFinal.openpicnum);
                PhotoCaptureFinal.openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                ModelManager.Instance.SetTipText("" + openpicname);
                File.Delete(PhotoCaptureFinal.openpicpath);

                //PhotoCaptureFinal.sumpic--;
                PhotoCaptureFinal.openpicnum++;
                int open = PhotoCaptureFinal.openpicnum;
                if (open > 1)
                {
                    PCF.BackPhoto();
                }
                else
                {
                    PCF.NextPhoto();
                }
                if (PhotoCaptureFinal.sumpic == 0)
                {
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
                    //ModelManager.Instance.SetTipText("Nothing.Please air tap to take photos");
                }

            }
            
        else if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
            {

                string filename = string.Format("TestVideo_{0}.mp4", PhotoCaptureFinal.openVdieonum);
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                File.Delete(filepath);
                ModelManager.Instance.SetTipText("" + PhotoCaptureFinal.openVdieonum.ToString() + "//" + PhotoCaptureFinal.sumVideo.ToString());
                //PhotoCaptureFinal.openVdieonum--;
                //PhotoCaptureFinal.sumVideo--;
                if (PhotoCaptureFinal.openVdieonum > 1)
                {

                    PCF.PreviousVideo();
                }
                else if (PhotoCaptureFinal.openVdieonum == 1)
                {
                    quad.GetComponent<Image>().sprite = null;
                    //ModelManager.Instance.SetTipText("Nothing.Please air tap to record videos");
                }

            }
        }
*/
        else if (str.Equals("end"))//关闭vuforia
        {
            if (Vuforia.CameraDevice.Instance.IsActive())
            {
                GameObject.Find("ARCamera").GetComponent<Vuforia.VuforiaBehaviour>().enabled = false;
            }
        }
        else if (str.Equals("start"))//开启vuforia
        {
            GameObject.Find("ARCamera").GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
        }
        else if (str.Equals("take video"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("record");
            if (StatusManager.Instance.currentstatus != CurrentStatus.CaptureVideo)
            {

                StatusManager.Instance.SetStatus(CurrentStatus.CaptureVideo);
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                g.GetComponent<PhotoCaptureFinal>().TakeVideo();
            }
            else
            {
                PhotoCaptureFinal.m_VideoCapture.StopRecordingAsync(g.GetComponent<PhotoCaptureFinal>().OnStoppedRecordingVideo);
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                StatusManager.Instance.SetStatus(CurrentStatus.Ready);

            }
        }
        else if (str.Equals("close video"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("back");

            PhotoCaptureFinal.m_VideoCapture.StopRecordingAsync(g.GetComponent<PhotoCaptureFinal>().OnStoppedRecordingVideo);
            quad.SetActive(false);
            modelma.Instance.imageObject.SetActive(true);
            modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
            statusma.Instance.SetStatus(CurrentStatus.Ready);

            PhotoCaptureFinal.m_VideoCapture.Dispose();
            PhotoCaptureFinal.m_VideoCapture = null;
            Debug.Log("停止录像模式!");
            StatusManager.Instance.currentstatus = CurrentStatus.Ready;
            g.GetComponent<PhotoCaptureFinal>().StopVideo();
        }
        else if (str.Equals("play video"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("video");
            if (StatusManager.Instance.GetStatus() != CurrentStatus.WatchVideo)
            {
                quad.SetActive(true);
                ModelManager.Instance.imageObject.SetActive(false);
                StatusManager.Instance.currentstatus = CurrentStatus.WatchVideo;
                string filename = string.Format("TestVideo_{0}.mp4", PhotoCaptureFinal.openVdieonum);
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                g.GetComponent<PhotoCaptureFinal>().PlayVideo(filepath);

            }
            else
            {
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                StatusManager.Instance.currentstatus = CurrentStatus.Ready;
                quad.SetActive(false);
            }
        }
        else if (str.Equals("note"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("light");
            g.GetComponent<Notes>().enabled = true;
        }
    }

    public void datedecrease()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            switch (flag)
            {
                case 0:
                    da.AddYears(-1);
                    D[flag].text = da.Year.ToString();
                    break;
                case 1:
                    da.AddMonths(-1);
                    D[flag].text = da.Month.ToString();
                    break;
                case 2:
                    da.AddDays(-1);
                    D[flag].text = da.Day.ToString();
                    break;
            }
        }
    }

    public void dateadd()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            switch (flag)
            {
                case 0:
                    da.AddYears(1);
                    D[flag].text = da.Year.ToString();
                    break;
                case 1:
                    da.AddMonths(1);
                    D[flag].text = da.Month.ToString();
                    break;
                case 2:
                    da.AddDays(1);
                    D[flag].text = da.Day.ToString();
                    break;
            }
        }
    }

    public void timedecrease()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            flag = (flag - 1) % 3;
            D[flag].color = Color.blue;
        }
    }

    public void timeadd()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            flag = (flag + 1) % 3;
            D[flag].color = Color.blue;
        }
    }

    public void showtime()
    {
        D[0].gameObject.SetActive(true);
        D[1].gameObject.SetActive(true);
        D[2].gameObject.SetActive(true);
        D[0].text = DateTime.Now.Year.ToString();
        D[1].text = DateTime.Now.Month.ToString();
        D[2].text = DateTime.Now.Day.ToString();

        D[0].color = Color.blue;
        flag = 0;
    }

    //每隔一段时间就接受一下
    private void FixedUpdate()
    {
        socket.ReceiveFromAsync(reciveArgs);
    }

    
}


/*
public class Community : MonoBehaviour
{
    public GameObject quad;
    public GameObject photobtn;
    public Sprite jujiao;
    
    public string recvStr;
    private string UDPClientIP;
    string str = "我是客户端，发送了消息";
    Socket socket;
    EndPoint serverEnd;
    IPEndPoint ipEnd;

    byte[] recvData = new byte[1024];
    byte[] sendData = new byte[1024];
    int recvLen = 0;
    Thread connectThread;
    

                          
    void Start()
    {
        UDPClientIP = "127.0.0.1";//服务端的IP.更改
        UDPClientIP = UDPClientIP.Trim();
        InitSocket();
    }

    void InitSocket()
    {
        ipEnd = new IPEndPoint(IPAddress.Parse(UDPClientIP), 8781);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        print("等待连接");
        SocketSend(str);
        print("连接");
        //开启一个线程连接
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }
    void SocketSend(string sendStr)
    {
    //清空
        sendData = new byte[1024];
    //数据转换
        sendData = Encoding.UTF8.GetBytes(sendStr);
    //发送给指定服务端
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }

    //接收服务器信息
    void SocketReceive()
    {
        while (true)
        {

            recvData = new byte[1024];
            try
            {
                recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
            }
            catch (Exception e)
            {
            print("error" + e);
            }

            print("信息来自: " + serverEnd.ToString());
            if (recvLen > 0)
            {
                recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
            }

            print(recvStr);
            Debug.Log(recvStr);
            manage(recvStr);
        }
    }

    void manage(string str)
    {
        if (str.Equals("open"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("open");
            StatusManager.Instance.currentstatus = CurrentStatus.Ready;
            g.GetComponent<StatusManager>().OpenCamera();

        }else if (str.Equals("close"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("open");
            StatusManager.Instance.currentstatus = CurrentStatus.Close;
            g.GetComponent<StatusManager>().cameraIsColosed();
        }else if (str.Equals("take"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("shutter");
            StatusManager.Instance.SetStatus(CurrentStatus.Ready);
            quad.SetActive(false);
            ModelManager.Instance.imageObject.SetActive(true);
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            g.GetComponent<PhotoCaptureFinal>().ReadyToPhoto();
        }
    }

    //连接关闭
    void SocketQuit()
    {
    //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
    //最后关闭socket
        if (socket != null)
            socket.Close();
        Debug.LogWarning("断开连接");
    }
                         

    void OnApplicationQuit()
    {
        SocketQuit();
    }
    
    void Update()
    {

    }

}
/*
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

//这个脚本是hololens端的SocketUDP脚本，提供发送方法，初始化并开启接收方法
public class Community : MonoBehaviour
{
    public GameObject quad;
    public GameObject photobtn;
    public Sprite jujiao;
    public Sprite daiji;
    private Text lMessage;
    static string openpicname = null;
    static string openpicpath = null;

private VideoPlayer videoplayer;

    PhotoCaptureFinal PCF = new PhotoCaptureFinal();//直接调用PhotoCaptureFinal的

    Socket socket; //目标socket
    //发送端口
    EndPoint serverEnd;
    IPEndPoint ipEnd;
    //接收端口
    IPEndPoint IPLocalPoint;
    //发送用的socket异步参数
    SocketAsyncEventArgs socketAsyceArgs;
    //接收用的socket异步参数
    SocketAsyncEventArgs reciveArgs;
    //接收SAEA用来接收的缓冲区
    byte[] reciveArgsBuffer;
    //初始化
    void InitSocket()
    {
        //定义连接的服务器ip和端口，可以是本机ip，局域网，互联网
        ipEnd = new IPEndPoint(IPAddress.Parse("192.168.1.21"), 8781);
        //初始化要接收的IP，IPAddress.Any表示接收所有IP地址发来的字节流
        IPLocalPoint = new IPEndPoint(IPAddress.Any, 8002);
        //初始化socket
        socket = new Socket(IPLocalPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

        //定义服务端
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        //初始化发送用的SAEA
        socketAsyceArgs = new SocketAsyncEventArgs();
        //设置发送用的SAEA的IP
        socketAsyceArgs.RemoteEndPoint = ipEnd;
        //初始化接收用的SAEA的缓冲区，此处我设为10K
        reciveArgsBuffer = new byte[1024 * 10];
        //初始化接收SAEA
        reciveArgs = new SocketAsyncEventArgs();
        //设置接收SAEA的接收IP地址
        reciveArgs.RemoteEndPoint = IPLocalPoint;
        //因为SAEA系列API 是异步方法，所以设置好完成方法后的回调
        reciveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(CompletedRecive);
        //设置接收缓冲区
        reciveArgs.SetBuffer(reciveArgsBuffer, 0, reciveArgsBuffer.Length);
    }

    //异步方法完成后的complete时间
    private void CompletedRecive(object sender, SocketAsyncEventArgs e)
    {
        //通过SAEA.LastOperation这个枚举来判断完成的是什么方法，对应不同的操作
        switch (reciveArgs.LastOperation)
        {
            //因为reciveArgs是我专门用来接收的SAEA,所以这里只设置一个完成接收后用的方法
            case SocketAsyncOperation.ReceiveFrom:
                PocessReceiveFrom(e);
                break;
        }
    }



    //中转缓冲区，将数据拷贝出来给主线程用
    byte[] tempBytes;
    //用来通知主线程的参数
    bool isOk = false;

    //注意：处理这个方法是辅线程，不要用Unity的类，否则报错，将收到的字节流拷贝出来，通知主线程来处理
    //接收完成后对应的处理方法
    public void PocessReceiveFrom(SocketAsyncEventArgs e)
    {

        if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
        {
            //这里会造成内存垃圾以及内存碎片化，如果频繁的长时间的接收，建议做一个Byte池。
            tempBytes = new byte[e.BytesTransferred];     //将数据拷贝出来保证可以复用
            Array.Copy(e.Buffer, e.Offset, tempBytes, 0, tempBytes.Length);
            //通知主线程
            isOk = true;
        }
    }

    /// <summary>
    /// 异步发送消息方法
    /// </summary>
    /// <param name="bytes"></param>
    public void AsyncSend(byte[] bytes)
    {
        //设置缓冲区，缓冲区里是发送的字节流
        socketAsyceArgs.SetBuffer(bytes, 0, bytes.Length);
        //Debug.Log("socket异步参数字节流长度 " + socketAsyceArgs.Buffer.Length);
        bool bo = socket.SendToAsync(socketAsyceArgs);
        if (!bo)
        {
            //在hololens上发现过一段时间scoket就不会发送数据，最后这样处理：判断SentToAsync方法失败后，就重新new一个SAEA，解决socket发送失败的问题
            //注意初始化一个SAEA时，1.IP    2.缓冲区，3.完成后的回调事件  这三个都是必要的，
            socketAsyceArgs = new SocketAsyncEventArgs();
            socketAsyceArgs.RemoteEndPoint = ipEnd;
        }
    }

    //初始化socket并测试一下
    private void Start()
    {
        lMessage = GameObject.Find("Canvas/Text").GetComponent<Text>();
        InitSocket();
        TestSocekt();
    }

    //用来测试socket的方法，发送一个信息
    void TestSocekt()
    {
        //int tempInt = 15;
        byte[] tempBytes = Encoding.UTF8.GetBytes("hello");
        //tempBytes = BitConverter.GetBytes(tempInt);

        AsyncSend(tempBytes);
    }

    private void Update()
    {
        if (isOk)
        {
            //对tempBytes进行处理
            string temp = Encoding.UTF8.GetString(tempBytes, 0, tempBytes.Length);
            Debug.Log("接收socket,接收到了字节流，接收到的数字为 " + temp);

            lMessage.text = temp;
            manage(temp);

            //每接受完就发一次消息
            //byte[] tempByte = Encoding.UTF8.GetBytes("ok");
            //tempBytes = BitConverter.GetBytes(tempInt);
            //AsyncSend(tempByte);

            isOk = false;
        }
    }

    void manage(string str)
    {
        if (str.Equals("open"))//开机
        {
            GameObject g = GameObject.FindGameObjectWithTag("open");
            StatusManager.Instance.currentstatus = CurrentStatus.Ready;
            g.GetComponent<StatusManager>().OpenCamera();
        }
        else if (str.Equals("close"))//关机
        {
            ModelManager.Instance.imageObject.SetActive(false);
            GameObject g = GameObject.FindGameObjectWithTag("open");
            StatusManager.Instance.currentstatus = CurrentStatus.Close;
            g.GetComponent<StatusManager>().cameraIsColosed();
        }
        else if (str.Equals("take"))//拍照
        {
            GameObject g = GameObject.FindGameObjectWithTag("shutter");
            StatusManager.Instance.SetStatus(CurrentStatus.Ready);
            quad.SetActive(false);
            ModelManager.Instance.imageObject.SetActive(true);
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            g.GetComponent<PhotoCaptureFinal>().ReadyToPhoto();
        }
        else if (str.Equals("play"))//查看照片
        {
            GameObject g = GameObject.FindGameObjectWithTag("play");
            StatusManager.Instance.SetStatus(CurrentStatus.ReadPhoto);
            ModelManager.Instance.imageObject.SetActive(true);
            quad.SetActive(false);
            if (PhotoCaptureFinal.picList.Count>0){
                openpicname = PhotoCaptureFinal.picList[0].Name + ".jpg";
                openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                g.GetComponent<PhotoCaptureFinal>().ShowPhoto(openpicname, openpicpath, 0);
            }

        }
        else if (str.Equals("next"))//下一张
        {
            GameObject g = GameObject.FindGameObjectWithTag("back");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                g.GetComponent<PhotoCaptureFinal>().NextPhoto();
            }
        }
        else if (str.Equals("previous"))//上一张
        {
            GameObject g = GameObject.FindGameObjectWithTag("back");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                g.GetComponent<PhotoCaptureFinal>().BackPhoto();
            }
        }
        else if (str.Equals("remove"))
        {
            Debug.Log("remove");
            if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
            {
                //openpicname = string.Format(@"CapturedImage{0}.jpg", PhotoCaptureFinal.openpicnum);

                openpicname = PhotoCaptureFinal.picList[PhotoCaptureFinal.openpicnum].Name+".jpg";

                openpicpath = Path.Combine(Application.persistentDataPath, openpicname);

                //ModelManager.Instance.SetTipText("" + openpicname);

                File.Delete(openpicpath);

                PhotoCaptureFinal.sumpic--;
                PhotoCaptureFinal.files = PhotoCaptureFinal.root.GetFiles();
                PhotoCaptureFinal.picList.RemoveAt(PhotoCaptureFinal.openpicnum);

                if (PhotoCaptureFinal.sumpic <= 0)
                {
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
                    //ModelManager.Instance.SetTipText("Nothing.Please air tap to take photos");
                }
                else
                {
                    int open = PhotoCaptureFinal.openpicnum;
                    if (open == PhotoCaptureFinal.sumpic)
                    {
                        PhotoCaptureFinal.openpicnum--;
                    }
                    openpicname = PhotoCaptureFinal.picList[PhotoCaptureFinal.openpicnum].Name + ".jpg";

                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    
                    PCF.ShowPhoto(openpicname, openpicpath, PhotoCaptureFinal.openpicnum);

                }
            }
            else if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
            {

                //string filename = string.Format("TestVideo_{0}.mp4", PhotoCaptureFinal.openVdieonum);
                string filename = PhotoCaptureFinal.videoList[PhotoCaptureFinal.openVdieonum].Name + ".mp4";
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                File.Delete(filepath);
                ModelManager.Instance.SetTipText("" + PhotoCaptureFinal.openVdieonum.ToString() + "//" + PhotoCaptureFinal.sumVideo.ToString());
                PhotoCaptureFinal.sumVideo--;
                PhotoCaptureFinal.files = PhotoCaptureFinal.root.GetFiles();
                PhotoCaptureFinal.videoList.RemoveAt(PhotoCaptureFinal.openVdieonum);

                if (PhotoCaptureFinal.sumVideo <= 0)
                {
                    quad.GetComponent<Image>().sprite = null;
                    //ModelManager.Instance.SetTipText("Nothing.Please air tap to record videos");
                }
                else
                {
                    int open = PhotoCaptureFinal.openVdieonum;
                    if (open == PhotoCaptureFinal.sumVideo)
                    {
                        PhotoCaptureFinal.openVdieonum--;
                    }
                    openpicname = PhotoCaptureFinal.videoList[PhotoCaptureFinal.openVdieonum].Name + ".mp4";;

                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    PCF.PlayVideo(openpicpath, PhotoCaptureFinal.openVdieonum);

                }

            }

        }
        else if (str.Equals("end"))//关闭voforia
        {
            if (Vuforia.CameraDevice.Instance.IsActive())//// 操作相机前检查相机是否在活动（相机Stop后或Vuforia未初始化完成，该函数返回false）
            {
                GameObject.Find("ARCamera").GetComponent<Vuforia.VuforiaBehaviour>().enabled = false;
            }
        }
        else if (str.Equals("start"))//开启voforia
        {
            GameObject.Find("ARCamera").GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
        }
        else if (str.Equals("take video"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("record");
            if (StatusManager.Instance.currentstatus != CurrentStatus.CaptureVideo)
            {

                StatusManager.Instance.SetStatus(CurrentStatus.CaptureVideo);
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                g.GetComponent<PhotoCaptureFinal>().TakeVideo();
            }
            else
            {
                PhotoCaptureFinal.m_VideoCapture.StopRecordingAsync(g.GetComponent<PhotoCaptureFinal>().OnStoppedRecordingVideo);
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                StatusManager.Instance.SetStatus(CurrentStatus.Ready);

            }
        }
        else if (str.Equals("close video"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("back");

            PhotoCaptureFinal.m_VideoCapture.StopRecordingAsync(g.GetComponent<PhotoCaptureFinal>().OnStoppedRecordingVideo);
            quad.SetActive(false);
            modelma.Instance.imageObject.SetActive(true);
            modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
            statusma.Instance.SetStatus(CurrentStatus.Ready);

            PhotoCaptureFinal.m_VideoCapture.Dispose();
            PhotoCaptureFinal.m_VideoCapture = null;
            Debug.Log("停止录像模式!");
            StatusManager.Instance.currentstatus = CurrentStatus.Ready;
            g.GetComponent<PhotoCaptureFinal>().StopVideo();
        }
        else if (str.Equals("play video"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("video");
            if (StatusManager.Instance.GetStatus() != CurrentStatus.WatchVideo)
            {
                quad.SetActive(true);
                ModelManager.Instance.imageObject.SetActive(false);
                StatusManager.Instance.currentstatus = CurrentStatus.WatchVideo;
                //string filename = string.Format("TestVideo_{0}.mp4", PhotoCaptureFinal.openVdieonum);
                string filename = PhotoCaptureFinal.videoList[0].Name+".mp4";
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                g.GetComponent<PhotoCaptureFinal>().PlayVideo(filepath, 0);
            }
            else
            {
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                StatusManager.Instance.currentstatus = CurrentStatus.Ready;
                quad.SetActive(false);
            }
        }
        else if (str.Equals("note"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("light");
            g.GetComponent<Notes>().enabled = true;
        }
    }

    //每隔一段时间就接受一下
    private void FixedUpdate()
    {
        socket.ReceiveFromAsync(reciveArgs);
    }

}



public class Community : MonoBehaviour
{
    public GameObject quad;
    public GameObject photobtn;
    public Sprite jujiao;
    
    public string recvStr;
    private string UDPClientIP;
    string str = "我是客户端，发送了消息";
    Socket socket;
    EndPoint serverEnd;
    IPEndPoint ipEnd;

    byte[] recvData = new byte[1024];
    byte[] sendData = new byte[1024];
    int recvLen = 0;
    Thread connectThread;
    

                          
    void Start()
    {
        UDPClientIP = "127.0.0.1";//服务端的IP.更改
        UDPClientIP = UDPClientIP.Trim();
        InitSocket();
    }

    void InitSocket()
    {
        ipEnd = new IPEndPoint(IPAddress.Parse(UDPClientIP), 8781);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = (EndPoint)sender;
        print("等待连接");
        SocketSend(str);
        print("连接");
        //开启一个线程连接
        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }
    void SocketSend(string sendStr)
    {
    //清空
        sendData = new byte[1024];
    //数据转换
        sendData = Encoding.UTF8.GetBytes(sendStr);
    //发送给指定服务端
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }

    //接收服务器信息
    void SocketReceive()
    {
        while (true)
        {

            recvData = new byte[1024];
            try
            {
                recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
            }
            catch (Exception e)
            {
            print("error" + e);
            }

            print("信息来自: " + serverEnd.ToString());
            if (recvLen > 0)
            {
                recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
            }

            print(recvStr);
            Debug.Log(recvStr);
            manage(recvStr);
        }
    }

    void manage(string str)
    {
        if (str.Equals("open"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("open");
            StatusManager.Instance.currentstatus = CurrentStatus.Ready;
            g.GetComponent<StatusManager>().OpenCamera();

        }else if (str.Equals("close"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("open");
            StatusManager.Instance.currentstatus = CurrentStatus.Close;
            g.GetComponent<StatusManager>().cameraIsColosed();
        }else if (str.Equals("take"))
        {
            GameObject g = GameObject.FindGameObjectWithTag("shutter");
            StatusManager.Instance.SetStatus(CurrentStatus.Ready);
            quad.SetActive(false);
            ModelManager.Instance.imageObject.SetActive(true);
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            g.GetComponent<PhotoCaptureFinal>().ReadyToPhoto();
        }
    }

    //连接关闭
    void SocketQuit()
    {
    //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
    //最后关闭socket
        if (socket != null)
            socket.Close();
        Debug.LogWarning("断开连接");
    }
                         

    void OnApplicationQuit()
    {
        SocketQuit();
    }
    
    void Update()
    {

    }

}
*/
