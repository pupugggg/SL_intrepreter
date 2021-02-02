using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using Common;

public class UserInterface : MonoBehaviour {
    public bool Pause;
    public GameObject pauseMenu;
    public GameObject inputField;
    public Material Mat1, Mat2;
    public string OpenProject()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = "txt (*.txt)\0 *.txt";
        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir =UnityEngine.Application.dataPath;  // default path  
        pth.title = "打開項目";
        pth.defExt = "txt";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (Common.OpenFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file;//选择的文件路径;  
            return filepath;
        }
        return null;
    }
    // Use this for initialization
    void Start () {
        Pause = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            Pause = !Pause;
            pauseMenu.SetActive(Pause);
        }
        if (Pause)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
	}
    public void AppQuit()
    {
        UnityEngine.Application.Quit();
    }

    public void ReadFile()
    {

        string path = OpenProject();
        if (!string.IsNullOrEmpty(path))
        {
            inputField.GetComponent<Intrepreter>().ClearScrollViewContent();
            CustomVariablesAndFunctions.Clean();
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            string line = file.ReadToEnd();
            Debug.Log(line);
            //移除空格
            line = line.Replace(" ", "");
            //把newline換成空格
            line = Regex.Replace(line, @"\r\n?|\n", " ");
            line = line.Replace(System.Environment.NewLine, " ");
            //移除註解行(第一行)
            line = line.Remove(0, line.IndexOf(" "));
            Debug.Log(line);
            string[] tmp = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> separated = new List<string>();
            for (int i = 0; i < tmp.Length; i++)
                separated.Add(tmp[i]);
            for (int i = 0; i < separated.Count; i++)
                if (separated[i].IndexOf("//") == 0)
                    separated[i] = separated[i].Remove(0);
            int leftPos = 0, rightPos = 0;
            bool flag = false;
            int head = 0;
            for (int i = 0; i < separated.Count; i++)
            {

                if ((leftPos = separated[i].IndexOf("/*")) != -1)
                {

                    if ((rightPos = separated[i].IndexOf("*/")) != -1)
                    {
                        separated[i] = separated[i].Remove(leftPos, (rightPos + 1) - leftPos + 1);

                    }
                    else
                    {
                        separated[i] = separated[i].Remove(leftPos);
                        flag = true;
                        head = i + 1;
                    }
                }
                if (flag)
                {
                    if ((rightPos = separated[i].IndexOf("*/")) != -1)
                    {
                        separated[i] = separated[i].Substring(rightPos + 2);
                        for (int j = 0; j < i - head; j++)
                            separated.RemoveAt(head);
                        flag = false;
                    }
                }
            }

            separated.RemoveAll(string.IsNullOrEmpty);
            foreach (string i in separated)
            {
                inputField.GetComponent<InputField>().text = i;
                inputField.GetComponent<Intrepreter>().Parse();
            }
            Pause = false;
            pauseMenu.SetActive(false);
            file.Close();

            while(inputField.GetComponent<Intrepreter>().previousRec.Count > 0)
                inputField.GetComponent<Intrepreter>().previousRec.Pop();
            inputField.GetComponent<Intrepreter>().previousRec.Push(separated[separated.Count - 1]);
        }
    }

    public void changeSkyBox(int idx)
    {
        switch (idx)
        {
            case 0:
                RenderSettings.skybox=Mat2;
                break;
            case 1:
                RenderSettings.skybox = Mat2;
                break;
        }
    }
}
