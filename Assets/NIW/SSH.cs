using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Renci.SshNet;
using UnityEngine.UI;

public class SSH : MonoBehaviour {

    private Text text = null;

    private string _host = "192.168.5.81"; //host.h
    private string _username = "NIW"; // sre // apocalypse
    private string _password = "n7w5re";

    void Start()
    {
        Invoke("SSHmethod", 30);

    }

    void SSHmethod() { 
        text = GameObject.Find("SSH").GetComponent<Text>();

        try
        {
            var connectionInfo = new PasswordConnectionInfo(_host, 22, _username, _password);
            text.text += "connection infos : ok\n";

            using (var client = new SshClient(connectionInfo))
            {
                text.text += "Connecting...\n";
                client.Connect();
                text.text += "OK\n";

                var command = client.RunCommand("open ./Desktop/NIW/scripts/launchscripts/textures/MaxModular.command"); //pwd
                text.text += command.Result;

                text.text += "Disconnecting...\n";
                client.Disconnect();
                text.text += "OK\n";

                Debug.Log(text.text);
            }
        }
        catch (System.Exception e)
        {
            text.text = "Error\n" + e;
            Debug.Log(text.text + e);

        }
    }
    //IEnumerator Wait()
   // {
     //   print(Time.time);
     //   yield return new WaitForSeconds(30);
     //   print(Time.time);
   // }
}
