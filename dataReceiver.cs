using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System;
using System.Text;
using UnityEngine.UI;


public class dataReceiver : MonoBehaviour {

    const int port = 8888;
    private Thread mainThread = null;
    private string dataToWrite;
    const int BUFFER_LEN = 1000;
    private bool newgesture = false;
    private int count = 0;
    public MovieTexture movie_front;
    public MovieTexture movie_side;
    private FileInfo[] videos_front;
    private FileInfo[] videos_side;
    private int numVideo = 0;
    private string debuging = "";
    private bool modifying = false;
    Text name;
    InputField gestureNow;
    string gestureName;
    HttpListener httpListener;
    private int gestureResult = 0;
    int lastgesture = 0;
    bool wait = false;
    // Use this for initialization
    void Start () {
        //gestureNow = GameObject.Find("GestureNow").GetComponent<InputField>();
        name = GameObject.Find("Name").GetComponent<Text>();
        DirectoryInfo dirname_front = new DirectoryInfo("Assets/Resources/front");
        videos_front = dirname_front.GetFiles();
        DirectoryInfo dirname_side = new DirectoryInfo("Assets/Resources/side");
        videos_side = dirname_side.GetFiles();
        System.Random rnd = new System.Random();
        /*int n = videos_front.Length;
        while (n > 1)
        {
            int k = (rnd.Next(0, n) % n);
            n--;
            FileInfo value = videos_front[k];
            videos_front[k] = videos_front[n];
            videos_front[n] = value;
        }*/
        string first = videos_front[numVideo].Name;

        while (first[first.Length - 5] == '.')
        {
            //Debug.Log(first);
            //Debug.Log(first[first.Length - 5]);
            numVideo++;
            first = videos_front[numVideo].Name;
        }
        first = first.Remove(first.Length - 4);
        name.text = first;
        movie_front = Resources.Load("front/" +first) as MovieTexture;
        GameObject display = GameObject.Find("Display Front");
        //display.transform.Translate(Vector3.forward * 1000, Space.World);
        display.GetComponent<RawImage>().texture = movie_front as MovieTexture;
        movie_front.Play();
        //side display
        first = videos_side[numVideo].Name;

        while (first[first.Length - 5] == '.')
        {
            //Debug.Log(first);
            //Debug.Log(first[first.Length - 5]);
            numVideo++;
            first = videos_side[numVideo].Name;
        }
        first = first.Remove(first.Length - 4);
        //name.text = first;
        movie_side = Resources.Load("side/" + first) as MovieTexture;
        GameObject display_side = GameObject.Find("Display Side");
        display_side.GetComponent<RawImage>().texture = movie_side as MovieTexture;
        movie_side.Play();
        httpListener = new HttpListener();

        httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        httpListener.Prefixes.Add("http://127.0.0.1:8456/");
        
        httpListener.Start();
        new Thread(new ThreadStart(delegate
        {
            while (true)
            {
                //mytext.text = count.ToString();
                //count++;
                HttpListenerContext httpListenerContext = httpListener.GetContext();
                string query = httpListenerContext.Request.Url.ToString();
                //objectName.text = httpListenerContext.Request.QueryString.ToString();
                string type = query.Split('/')[3];

                httpListenerContext.Response.StatusCode = 200;
                //query = httpListenerContext.Request.QueryString[0];
                //Debug.Log(type);
                if (type[0] == 't')
                {
                    //Debug.Log(query);
                    using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                    {
                        //need to be changed
                        writer.WriteLine(dataToWrite);
                        //writer.WriteLine("0.007750986 0.08418342 0.07138674 356.9888 359.1182 357.67362");
                    }
                }
                else
                {
                    gestureResult = Int32.Parse(type);
                    using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                    {
                        //need to be changed
                        writer.WriteLine("-1");

                    }
                }
                /*else
                {
                    //Debug.Log(query);
                    result = new int[topnum];
                    string[] strresult = type.Split('?');
                    for (int i = 0; i < topnum; i++)
                        result[i] = int.Parse(strresult[i]);
                    using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                    {
                        writer.WriteLine(senddata);

                    }
                }*/
            }
        })).Start();
    }
	
	// Update is called once per frame
	void Update () {
        //gestureName = gestureNow.text;
        /*if (newgesture)
        {
            Debug.Log("Press");
            if (!movie.isPlaying)
                movie.Stop();
            movie.Play();
        }*/
        //Debug.Log(BitConverter.GetBytes(numVideo)[0]);
        if(Input.GetKeyDown(KeyCode.I))
        {
            httpListener = new HttpListener();

            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add("http://127.0.0.1:8120/");
            httpListener.Start();
            new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    //mytext.text = count.ToString();
                    //count++;
                    HttpListenerContext httpListenerContext = httpListener.GetContext();
                    string query = httpListenerContext.Request.Url.ToString();
                    //objectName.text = httpListenerContext.Request.QueryString.ToString();
                    string type = query.Split('/')[3];

                    httpListenerContext.Response.StatusCode = 200;
                    //query = httpListenerContext.Request.QueryString[0];
                    if (type[0] == 't')
                    {
                        //Debug.Log(query);
                        using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                        {
                            //need to be changed
                            writer.WriteLine(dataToWrite);

                        }
                    }
                    /*else
                    {
                        //Debug.Log(query);
                        result = new int[topnum];
                        string[] strresult = type.Split('?');
                        for (int i = 0; i < topnum; i++)
                            result[i] = int.Parse(strresult[i]);
                        using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                        {
                            writer.WriteLine(senddata);

                        }
                    }*/
                }
            })).Start();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("Press");
            if (!movie_front.isPlaying || !movie_side.isPlaying)
            {
                movie_front.Stop();
                movie_side.Stop();
            }
            movie_front.Play();
            movie_side.Play();
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            numVideo = 0;
            string next = videos_front[numVideo].Name;
            while (next[next.Length - 5] == '.' && numVideo < videos_front.Length)
            {
                //Debug.Log(next);
                //Debug.Log(next[next.Length - 5]);
                numVideo++;
                next = videos_front[numVideo].Name;
            }
            next = next.Remove(next.Length - 4);
            name.text = next;
            movie_front = Resources.Load("front/" + next) as MovieTexture;
            GameObject display = GameObject.Find("Display Front");
            display.GetComponent<RawImage>().texture = movie_front as MovieTexture;
            movie_front.Play();

            next = videos_side[numVideo].Name;
            while (next[next.Length - 5] == '.' && numVideo < videos_front.Length)
            {
                //Debug.Log(next);
                //Debug.Log(next[next.Length - 5]);
                numVideo++;
                next = videos_side[numVideo].Name;
            }
            next = next.Remove(next.Length - 4);
            movie_side = Resources.Load("side/" + next) as MovieTexture;
            GameObject display_side = GameObject.Find("Display Side");
            display_side.GetComponent<RawImage>().texture = movie_side as MovieTexture;
            movie_side.Play();
        }
        //Move(new Vector3(-1.5f, 0, 0), 15);
        if (numVideo == videos_front.Length)
            name.text = "Done! Thank you!";
        if (Input.GetKeyDown(KeyCode.RightArrow) && numVideo < videos_front.Length)
        {
            numVideo++;
            string next = videos_front[numVideo].Name;
            while (next[next.Length - 5] == '.' && numVideo < videos_front.Length)
            {
                //Debug.Log(next);
                //Debug.Log(next[next.Length - 5]);
                numVideo++;
                next = videos_front[numVideo].Name;
            }
            next = next.Remove(next.Length - 4);
            name.text = next;
            Debug.Log(next);
            movie_front = Resources.Load("front/" + next) as MovieTexture;
            GameObject display_front = GameObject.Find("Display Front");
            display_front.GetComponent<RawImage>().texture = movie_front as MovieTexture;
            movie_front.Play();
            next = videos_side[numVideo].Name;
            while (next[next.Length - 5] == '.' && numVideo < videos_front.Length)
            {
                //Debug.Log(next);
                //Debug.Log(next[next.Length - 5]);
                numVideo++;
                next = videos_side[numVideo].Name;
            }
            next = next.Remove(next.Length - 4);
            
            movie_side = Resources.Load("side/" + next) as MovieTexture;
            GameObject display_side = GameObject.Find("Display Side");
            display_side.GetComponent<RawImage>().texture = movie_side as MovieTexture;
            movie_side.Play();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && numVideo > 0)
        {
            //Debug.Log("here");
            numVideo--;
            string next = videos_front[numVideo].Name;
            while (next[next.Length - 5] == '.' && numVideo > 0)
            {
                //Debug.Log(next);
                //Debug.Log(next[next.Length - 5]);
                numVideo--;
                next = videos_front[numVideo].Name;
            }
            next = next.Remove(next.Length - 4);
            name.text = next;
            movie_front = Resources.Load("front/" + next) as MovieTexture;
            GameObject display_front = GameObject.Find("Display Front");
            display_front.GetComponent<RawImage>().texture = movie_front as MovieTexture;
            movie_front.Play();

            next = videos_side[numVideo].Name;
            while (next[next.Length - 5] == '.' && numVideo < videos_front.Length)
            {
                Debug.Log(next);
                Debug.Log(next[next.Length - 5]);
                numVideo++;
                next = videos_side[numVideo].Name;
            }
            next = next.Remove(next.Length - 4);
            Debug.Log(next);
            movie_side = Resources.Load("side/" + next) as MovieTexture;
            GameObject display_side = GameObject.Find("Display Side");
            display_side.GetComponent<RawImage>().texture = movie_side as MovieTexture;
            movie_side.Play();
        }
        //name.text = debuging;
    }

    private void OnGUI()
    {
        if (mainThread == null)
        {
            if (GUI.Button(new Rect(200, 50, 200, 50), "start capture server"))
            {
                Debug.Log("start");
                startServer();
            }
        }
        else
        {
            if (GUI.Button(new Rect(200, 50, 200, 50), "end capture server"))
            {
                endServer();
            }
        }
    }

    private void startServer()
    {
        string ipaddress = Network.player.ipAddress;
        mainThread = new Thread(() => hostServer(ipaddress));
        mainThread.Start();
    }
    private void hostServer(string ipadress)
    {
        IPAddress serverIP = IPAddress.Parse(ipadress);
        //IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        TcpListener listener = new TcpListener(serverIP, port);
        Debug.Log(ipadress);
        debuging = ipadress;
        listener.Start();
        while(mainThread != null)
        {
            if(listener.Pending())
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread thread = new Thread(() => receiveThread(client));
                thread.Start();
            }
            Thread.Sleep(10);

        }
        listener.Stop();
    }
    private void receiveThread(TcpClient client)
    {
        Debug.Log("connected");
        Stream sr = new StreamReader(client.GetStream()).BaseStream;
        Stream sw = new StreamWriter(client.GetStream()).BaseStream;
        Debug.Log("connected");
        FileStream fileToWrite = new FileStream("new_natural.txt", FileMode.Create);
        StreamWriter outputFile = new StreamWriter(fileToWrite);
        Debug.Log("connected");
        byte[] buffer = new byte[400];
        while(mainThread != null)
        {
            try
            {

                sr.Read(buffer, 0, 100);
                modifying = true;
                dataToWrite = Encoding.Default.GetString(buffer);
                debuging = dataToWrite;
                //Debug.Log(dataToWrite);
                if (dataToWrite[0] == '*')
                {
                    Debug.Log(dataToWrite);
                    newgesture = true;
                    dataToWrite.Remove(0, 1);
                    dataToWrite = name.text + " " + dataToWrite;
                    

                }
                else
                {
                    newgesture = false;
                    //dataToWrite = "";
                }
                //Debug.Log(dataToWrite);
                outputFile.WriteLine(dataToWrite);
                outputFile.Flush();
                //Debug.Log(BitConverter.GetBytes(numVideo));
                //Debug.Log(BitConverter.GetBytes(numVideo).Length);

                //Debug.Log(dataToWrite);


                //Debug.Log(gestureResult);
                //sw.WriteByte(0);
                if (gestureResult != 0)
                {
                    Debug.Log(gestureResult);
                    lastgesture = gestureResult;
                    sw.Write(BitConverter.GetBytes(gestureResult), 0, 4);
                    //gestureResult = -1;
                    //sw.Write(BitConverter.GetBytes(numVideo), 0, 4);
                    sw.Flush();
                    sr.Read(buffer, 0, 4);
                    if (buffer[0] == 0)
                        wait = true;
                    else
                        wait = false;
                }
                else if (wait == false)
                {
                    sw.Write(BitConverter.GetBytes(gestureResult), 0, 4);
                    //gestureResult = -1;
                    //sw.Write(BitConverter.GetBytes(numVideo), 0, 4);
                    sw.Flush();
                    sr.Read(buffer, 0, 4);
                }
                else
                {
                    sw.Write(BitConverter.GetBytes(lastgesture), 0, 4);
                    //gestureResult = -1;
                    //sw.Write(BitConverter.GetBytes(numVideo), 0, 4);
                    sw.Flush();
                    sr.Read(buffer, 0, 4);
                    if (buffer[0] == 0)
                        wait = true;
                    else
                        wait = false;
                }
               modifying = false;


            }
            catch
            {
                break;
            }

            Thread.Sleep(10);
        }
        client.Close();
        outputFile.Close();
    }

    private void endServer()
    {
        mainThread = null;
    }
}
