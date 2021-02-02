using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityScript;
public class Intrepreter : MonoBehaviour {
    public GameObject ScrollViewContent;
    public List<GameObject> ScrollViewTexts;
    public GameObject TextObject;
    public GameObject inputField;
    public GameObject pair;
    public GameObject nullBlock;
    public GameObject stateSphere;
    public GameObject Cam;
    public Canvas canvas;
    public Button btnPrefab;
    public Text stateName;
    public Material cyan_mat;
    public int TotalPairs;
    public GameObject MaximunAmountInputField;
    GameObject currentModel;
    GameObject highlightedState;
    Vector3 currentPosition;
    Quaternion currentRotation;
    bool isStart;
    string Content;
    int I;
    Stack<GameObject> blockStack;
    Stack<bool> isFromInputField;
    public Stack<string> previousRec; // 上一步用的紀錄
    List<GrammarSymobl> symobls;
    List<Button> stateBtn;
    List<GameObject> stateList;
    Dictionary<string, int> stateCount;
    string stringForInputField;

    // Use this for initialization
    private void Awake()
    {
        TotalPairs = 1;
        CustomVariablesAndFunctions.CustomVariableSet = new List<CustomVariablesAndFunctions>();
        CustomVariablesAndFunctions.CustomFunctionSet = new List<CustomVariablesAndFunctions>();
        blockStack = new Stack<GameObject>();
        isFromInputField = new Stack<bool>();
        
        ScrollViewTexts = new List<GameObject>();
        CustomVariablesAndFunctions.lastFunc = 0;
        CustomVariablesAndFunctions.lastVar = 0;

        currentPosition = new Vector3(0f, 0f, 0f);
        currentRotation = new Quaternion(0f, 0f, 0f, 0f);

        symobls = new List<GrammarSymobl>();
        stateBtn = new List<Button>();
        stateList = new List<GameObject>();
        stateCount = new Dictionary<string, int>();
        previousRec = new Stack<string>();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;           
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                GameObject st = hit.collider.gameObject;

                int index = -1;
                // Find index (which state).
                for (int i = 0; i < symobls.Count; i++)
                    if (symobls[i].name == st.name.Substring(0, st.name.IndexOf(')') + 1))
                    {
                        index = i;
                        break;
                    }

                if (index != -1) // if found
                {
                    // Show name
                    stateName.text = st.gameObject.name;
                    if (highlightedState)
                        highlightedState.GetComponent<Renderer>().material = cyan_mat;
                    st.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    highlightedState = st;

                    // delete previous buttons
                    for (int i = 0; i < stateBtn.Count; i++)
                        Destroy(stateBtn[i].gameObject);
                    stateBtn.Clear();

                    // Create buttons.
                    for (int i = 0; i < symobls[index].symbol.Count; i++)
                    {
                        Button btn = Instantiate(btnPrefab);
                        btn.transform.SetParent(canvas.transform, false);
                        btn.gameObject.name = symobls[index].symbol[i];
                        btn.GetComponentInChildren<Text>().text = symobls[index].symbol[i];
                        float y = (i < 6) ? -175f : -230f;
                        print("i = " + i);
                        print(i % 6 - symobls[index].symbol.Count / 2);
                        btn.GetComponent<RectTransform>().localPosition = new Vector3((i % 6 - symobls[index].symbol.Count / 2) * 200 + 100f, y, 0f);

                        stateBtn.Add(btn);
                        
                        int n = int.Parse(st.name.Substring(st.name.IndexOf(')') + 1, 1));
                        int pos = -1, endPos;
                        string content = Content;
                        string stateName = st.name.Substring(0, st.name.IndexOf(')') + 1);
                        for (int j = 0; j < n; j++)
                            pos = content.IndexOf(stateName, pos + 1);                   
                        for (endPos = pos; content[endPos] != ')'; endPos++) ;

                        string c = content.Substring(0, pos) + symobls[index].symbol[i] + content.Substring(endPos + 1);
                        print(c);

                        btn.onClick.AddListener(delegate () { btnClick(c); });
                    }
                }
                else // not found
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        stateName.text = "";
                        if(highlightedState)
                            highlightedState.GetComponent<Renderer>().material = cyan_mat;
                        for (int i = 0; i < stateBtn.Count; i++)
                            Destroy(stateBtn[i].gameObject);
                        stateBtn.Clear();
                    }                       
                }
            }
            else
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    stateName.text = "";
                    if (highlightedState)
                        highlightedState.GetComponent<Renderer>().material = cyan_mat;
                    for (int i = 0; i < stateBtn.Count; i++)
                        Destroy(stateBtn[i].gameObject);
                    stateBtn.Clear();
                }
            }

        }
    }

    public void ScrollViewDisplay()
    {
        #region
        /* if (CustomVariablesAndFunctions.CustomFunctionSet.Count != 0)
          {
              ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
              ScrollViewTexts[0].transform.SetParent(ScrollViewContent.transform);
              ScrollViewTexts[0].GetComponent<Text>().text = "Functions:";
              for (int i=0;i< CustomVariablesAndFunctions.CustomFunctionSet.Count;i++)
              {
                  ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
                  ScrollViewTexts[i + 1].name = i.ToString();
                  ScrollViewTexts[1+i].transform.SetParent(ScrollViewContent.transform);
                  string para = "(";
                  for(int j=0;j< CustomVariablesAndFunctions.CustomFunctionSet[i].ReplacementList.Count;j++)
                  {
                      para += CustomVariablesAndFunctions.CustomFunctionSet[i].ReplacementList[j];
                      if (j != CustomVariablesAndFunctions.CustomFunctionSet[i].ReplacementList.Count - 1)
                          para += ",";
                  }
                  para += ")=";
                  ScrollViewTexts[1+i].GetComponent<Text>().text = CustomVariablesAndFunctions.CustomFunctionSet[i].Name+para+CustomVariablesAndFunctions.CustomFunctionSet[i].Representation;
              }
          }
          if (CustomVariablesAndFunctions.CustomVariableSet.Count != 0)
          {
              ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
              if (CustomVariablesAndFunctions.CustomFunctionSet.Count != 0)
              {
                  ScrollViewTexts[CustomVariablesAndFunctions.CustomFunctionSet.Count + 1].transform.SetParent(ScrollViewContent.transform);
                  ScrollViewTexts[CustomVariablesAndFunctions.CustomFunctionSet.Count + 1].GetComponent<Text>().text = "Variables:";
                  for (int i = 0; i < CustomVariablesAndFunctions.CustomVariableSet.Count; i++)
                  {
                      int offset = CustomVariablesAndFunctions.CustomFunctionSet.Count + 2 + i;
                      ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
                      ScrollViewTexts[offset].name = offset.ToString();
                      ScrollViewTexts[offset].transform.SetParent(ScrollViewContent.transform);
                      ScrollViewTexts[offset].GetComponent<Text>().text = CustomVariablesAndFunctions.CustomVariableSet[i].Name + " = " + CustomVariablesAndFunctions.CustomVariableSet[i].Representation;
                  }
              }
              else
              {
                  ScrollViewTexts[0].transform.SetParent(ScrollViewContent.transform);
                  ScrollViewTexts[0].GetComponent<Text>().text = "Variables:";
                  for (int i = 0; i < CustomVariablesAndFunctions.CustomVariableSet.Count; i++)
                  {
                      int offset = 1 + i;
                      ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
                      ScrollViewTexts[offset].name = offset.ToString();
                      ScrollViewTexts[offset].transform.SetParent(ScrollViewContent.transform);
                      ScrollViewTexts[offset].GetComponent<Text>().text = CustomVariablesAndFunctions.CustomVariableSet[i].Name + " = " + CustomVariablesAndFunctions.CustomVariableSet[i].Representation;
                  }
              }

          }
          if(symobls.Count!=0)
          {

          }*/
        #endregion
        for (int i = 0; i < CustomVariablesAndFunctions.CustomFunctionSet.Count; i++)
        {
            ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
            int last = ScrollViewTexts.Count - 1;
            ScrollViewTexts[last].name = "Func"+i.ToString();
            string para = "(";
            for (int j = 0; j < CustomVariablesAndFunctions.CustomFunctionSet[i].ReplacementList.Count; j++)
            {
                para += CustomVariablesAndFunctions.CustomFunctionSet[i].ReplacementList[j];
                if (j != CustomVariablesAndFunctions.CustomFunctionSet[i].ReplacementList.Count - 1)
                    para += ",";
            }
            para += ")=";
            ScrollViewTexts[last].GetComponent<Text>().text = CustomVariablesAndFunctions.CustomFunctionSet[i].Name + para + CustomVariablesAndFunctions.CustomFunctionSet[i].Representation;
            ScrollViewTexts[last].transform.SetParent(ScrollViewContent.transform);
        }
        for (int i = 0; i < CustomVariablesAndFunctions.CustomVariableSet.Count; i++)
        {
            ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
            int last = ScrollViewTexts.Count - 1;
            ScrollViewTexts[last].name = "Var"+last.ToString();         
            ScrollViewTexts[last].GetComponent<Text>().text = CustomVariablesAndFunctions.CustomVariableSet[i].Name + " = " + CustomVariablesAndFunctions.CustomVariableSet[i].Representation;
            ScrollViewTexts[last].transform.SetParent(ScrollViewContent.transform);
        }
        for(int i=0;i<symobls.Count;i++)
        {
            ScrollViewTexts.Add((Instantiate(TextObject) as GameObject));
            int last = ScrollViewTexts.Count - 1;
            ScrollViewTexts[last].name = "Grammar" + last.ToString();
            string symbol="";
            for (int j = 0; j < symobls[i].symbol.Count; j++)
                symbol += symobls[i].symbol[j] + " | ";
            symbol = symbol.Remove(symbol.Length - 3);
            ScrollViewTexts[last].GetComponent<Text>().text = symobls[i].name + " -> " + symbol;
            ScrollViewTexts[last].transform.SetParent(ScrollViewContent.transform);
        }
        float size = 0f;
        foreach (GameObject i in ScrollViewTexts)
        {
            if (i.GetComponent<Text>().text.Length > size)
            {
                size = i.GetComponent<Text>().text.Length;
                ScrollViewContent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(size*10, 40);
            }
        }
    }

    public void ClearScrollViewContent()
    {
        
        foreach(GameObject i in ScrollViewTexts)
        {
            Destroy(i);
        }
        ScrollViewTexts.Clear();
    }

    // 常數帶入函數，sigma rotation translation 合併custom 修改checking使其可on value chage display 
    //不能用的Variale:跟function同名 基本engagement
    //不能用的function名稱:sigma(in, 1 to 10) R(x,y,z) T(x,y,z)
    public void Parse()
    {
        Debug.Log("Parsing");
        string content = inputField.GetComponent<InputField>().text;
        if (!string.IsNullOrEmpty(content)&&content[content.Length - 1] == '\n')
            content = content.Remove(content.Length - 1);
        string[] tmp = content.Split('\n');
        foreach (string i in tmp)
        {
            Debug.Log(i);
            content = i;
            if (Check(content))//檢查文法
            {
                if (!Customizing(content))//判斷是否是宣告
                {
                    clearStatePrefab();
                    content = content.Replace(" ", "");
                    content = CustomFunctionAndVariale(content);//先取代函數 再來是變數
                    Debug.Log(content);
                    content = TransformationCalculation(content);
                    Debug.Log(content);
                    content = Exponential(content);//指數
                    Debug.Log(content);                   
                    content = RemoveRepeatedParenthesis(content);//把重複的括號去除
                    Debug.Log(content);
                    //content = doTransform(content);//把Transform乘開
                    //Debug.Log(content);
                   // content = RemoveParenthesis(content);//把括號去除'
                   // Debug.Log(content);
                    content = content.Replace("*", "");
                    Processing(content);//開始判斷接法
                    Debug.Log(content);

                }
                else
                {
                    Debug.Log("Customized");
                }
            }
            else
            {
                Debug.Log("syntax error");
            }
        }
        Content = content;
        previousRec.Push(content);
        inputField.GetComponent<InputField>().text= content;       
    }

    string RemoveParenthesis(string content)
    {
        content = content.Replace("(", "");
        content = content.Replace(")", "");
        return content;
    }

    public string RemoveRepeatedParenthesis(string content)
    {       
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '(')
            {
                int count = 0;
                for (int j = i + 1; j < content.Length; j++)
                {                   
                    if (content[j] == ')' && count == 0)
                    {
                        if(i - 1 >= 0 && j + 1 < content.Length && content[i - 1] == '(' && content[j + 1] == ')')
                        {
                            content = content.Remove(j, 1);
                            content = content.Remove(i, 1);
                            i = 0;                          
                        }
                        break;
                    }
                    else if (content[j] == '(') count++;
                    else if (content[j] == ')') count--;
                }
            }
        }
        return content;
    }

    public string ArithmeticConstant(string content)
    {
        Stack<int> valueStack = new Stack<int>();
        Stack<char> operatorStack = new Stack<char>();
        for (int i = 0; i < content.Length; i++)
        {

            if (content[i] >= '0' && content[i] <= '9')
            {
                string times = "";
                for (; i<content.Length&&(content[i] >= '0' && content[i] <= '9'); i++)
                    times += content[i];
                valueStack.Push(int.Parse(times));


            }
            if (i < content.Length && !(content[i] >= '0' && content[i] <= '9'))
            {
                if (operatorStack.Count == 0)
                {
                    operatorStack.Push(content[i]);
                }
                else
                {
                    int weight = Encode(content[i]);

                    while (operatorStack.Count != 0 && Encode(operatorStack.Peek()) > weight&& content[i] != '(')
                    {
                        calc(ref valueStack, ref operatorStack);
                    }
                    if (content[i] == ')' && operatorStack.Peek() == '(')
                    {
                        operatorStack.Pop();
                    }
                    if (content[i]!=')')
                    operatorStack.Push(content[i]);
                }
            }
        }
        content =valueStack.Pop().ToString();
        return content;
    }


    public void calc(ref Stack<int> num, ref Stack<char> oper)
    {
        int n1 = 0, n2 = 0;
        if (num.Count - 2 >= 0)
        {
            n2 = num.Pop();
            n1 = num.Pop();
        }
        switch(oper.Peek())
        {
            case '*':
                num.Push(n1 * n2);
                oper.Pop();
               // Debug.Log(n1+"*"+n2);
                break;
            case '+':
                num.Push(n1+ n2);
                oper.Pop();
                //Debug.Log(n1 + "+" + n2);
                break;
            case '/':
                num.Push(n1 / n2);
                oper.Pop();
               // Debug.Log(n1 + "/" + n2);
                break;
            case '^':
                num.Push((int)Mathf.Pow(n1, n2));
                oper.Pop();
                //Debug.Log(n1 + "^" + n2);
                break;
            case '-':
                num.Push(n1 - n2);
                oper.Pop();
               // Debug.Log(n1 + "-" + n2);
                break;
               
        }

    }
    public int Encode(char input)
    {
        switch(input)
        {
            case '(':
                return -1;
            case ')':
                return -1;
            case '*':
                return 1;
            case '+':
                return 0;
            case '/':
                return 1;
            case '^':
                return 2;
            case '-':
                return 0;
            default: return -2;
        }
    }
    public string ConstantExp(string content)
    {
        for(int i=0;i<content.Length;i++)
        {
            if(content[i]=='^')
            {
                if(content[i+1]=='(')
                {
                    int leftPos = i + 1, rightPos = content.IndexOf(')',i);
                    string replace = ArithmeticConstant(content.Substring(leftPos + 1, rightPos - leftPos - 1));
                    content = content.Replace(content.Substring(leftPos,rightPos-leftPos+1),replace);
                }
                else
                {
                    string times = "";//獲取指數
                    for (int j = 1; i + j < content.Length; j++)
                    {
                        if ((content[i + j] >= '0' && content[i + j] <= '9'))
                            times += content[i + j];
                        else
                            break;
                    }
                    content = content.Remove(i, times.Length + 1);//移除^n
                    string tmp = content.Substring(i - 1, 1) + "*";
                    for (int j = 0; j < int.Parse(times) - 1; i++)
                    {
                        content = content.Insert(i - 1, tmp);
                    }
                }
            }
        }
        return content;
    }


    public void Processing(string content)
    {
        clear(); // 每次輸入先刪掉所有的積木
        Cam.GetComponent<MoveCamera>().centroid = Vector3.zero;
        Cam.GetComponent<MoveCamera>().ObjCentroid = Vector3.zero;
        
        Content = content;
        for (int i = 0; i < content.Length; i++)
        {
            if (isStart)
            {
                isStart = false;
                currentPosition = new Vector3(0f, 0f, 0f);
                currentRotation = new Quaternion(0f, 0f, 0f, 0f);
            }
            else if (currentModel != null)
            {
                currentPosition = currentModel.transform.position;
                currentRotation = currentModel.transform.rotation;
            }
            /*
            print("position: " + currentPosition);
            print("rotation: " + currentRotation.eulerAngles);
            print(content[i]);*/

            bool isState = false;

            switch (content[i])
            {
                case '(':
                    isState = true;                 
                    currentModel = Instantiate(stateSphere, currentPosition, currentRotation);
                    string n = content.Substring(i, content.Substring(i).IndexOf(')', 0) + 1);
                    if (stateCount.ContainsKey(n))
                        stateCount[n]++;                   
                    else
                        stateCount.Add(n, 1);
                    currentModel.name = n + stateCount[n];
                    currentModel.transform.Translate(64f, 0f, 0f);
                    currentModel.transform.rotation *= Quaternion.Euler(180f, 0f, 0f);
                    stateList.Add(currentModel);
                    print("n = " + n);
                    i += n.Length - 1;
                    break;
                case 'h':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    currentModel.transform.Translate(64f, 0f, 0f);
                    currentModel.transform.rotation *= Quaternion.Euler(180f, 0f, 0f);
                    break;
                case 's':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    currentModel.transform.Translate(32f, -32f, 32f);
                    currentModel.transform.rotation *= Quaternion.Euler(180f, -90f, 0f);
                    break;
                case 't':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    currentModel.transform.Translate(32f, 32f, -32f);
                    currentModel.transform.rotation *= Quaternion.Euler(180f, 90f, 0f);
                    break;
                case 'd':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    currentModel.transform.Translate(64f, -32f, 0f);
                    break;
                case 'a':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    currentModel.transform.Translate(32f, 0f, -32f);
                    currentModel.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
                    break;
                case 'y':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    currentModel.transform.Translate(32f, -64f, 32f);
                    currentModel.transform.rotation *= Quaternion.Euler(0f, -90f, 0f);
                    break;
                case 'i':
                    currentModel = Instantiate(pair, currentPosition, currentRotation);
                    break;
                case 'T':
                    float tx = getNum(content, ref i) * 32f;
                    float ty = getNum(content, ref i) * 32f;
                    float tz = getNum(content, ref i) * 32f;
                    currentModel = Instantiate(nullBlock, currentPosition, currentRotation);
                    currentModel.transform.Translate(tx, ty, tz);
                    i--;
                    TotalPairs--;
                    break;
                case 'R':
                    float rx = 0f;
                    float ry = 0f;
                    float rz = 0f;
                    i++;
                    switch (content[i])
                    {
                        case 'x':
                        case 'X':
                            rx = getNum(content, ref i);
                            break;
                        case 'y':
                        case 'Y':
                            ry = getNum(content, ref i);
                            break;
                        case 'z':
                        case 'Z':
                            rz = getNum(content, ref i);
                            break;
                    }
                    currentModel = Instantiate(nullBlock, currentPosition, currentRotation);
                    currentModel.transform.rotation *= Quaternion.Euler(rx, rz, ry);
                    TotalPairs--;
                    i--;
                    break;
                case '+':
                    isStart = true;
                    TotalPairs--;
                    break;
                default:
                    TotalPairs--;
                    break;
            }
            if (currentModel != null && !isState)
                currentModel.tag = "Pair";
            /*
            print("position: " + currentPosition);
            print("rotation: " + currentRotation.eulerAngles);
            */
            Cam.GetComponent<MoveCamera>().ObjCentroid += transform.TransformVector(currentModel.transform.position);
            TotalPairs++;           
        }
        Cam.GetComponent<MoveCamera>().ObjCentroid /= TotalPairs;
        Cam.GetComponent<MoveCamera>().centroid = Cam.GetComponent<MoveCamera>().ObjCentroid;
        if (currentModel != null)
        {
            currentPosition = currentModel.transform.position;
            currentRotation = currentModel.transform.rotation;
            /*print(currentPosition);
            print(currentRotation.eulerAngles);*/
        }
        stateCount.Clear();
    }

    public void clear()
    {
        GameObject[] allPairs = GameObject.FindGameObjectsWithTag("Pair");
        foreach (GameObject pair in allPairs) Destroy(pair);
        isStart = true;

        currentPosition = new Vector3(0f, 0f, 0f);
        currentRotation = new Quaternion(0f, 0f, 0f, 0f);

        I = 0;
        stringForInputField = "";
        inputField.GetComponent<InputField>().text = stringForInputField;
        blockStack.Clear();
        isFromInputField.Clear();
    }



    //Y=ah^2 f(x)=ah^x
    public bool Customizing(string content)
    {
        Debug.Log("F:"+CustomVariablesAndFunctions.lastFunc);
        Debug.Log("V:"+CustomVariablesAndFunctions.lastVar);
        bool customize = false;
        /* for (int i = 0; i < content.Length; i++)
         {
             if (content[i] == '=')
             {
                 customize = true;
                 break;
             }
         }*/
        int type=0;
        if (content.IndexOf('=') != -1 || content.IndexOf("->") != -1)
        {
            if (content.IndexOf("->") != -1)
                type = 1;
            customize = true;
        }
        if (customize)
        {
            if (type == 0)
            {
                bool isFunc = false;
                for (int i = 0; i != content.IndexOf('='); i++)
                {
                    if (content[i] == '(')
                    {
                        content = content.Replace(content[i + 1], 'x');
                        isFunc = true;
                        break;
                    }
                }
                if (isFunc)
                {
                    int leftPos = content.IndexOf('('), rightPos = content.IndexOf(')', leftPos);
                    string funcName = content.Substring(0, content.IndexOf('('));
                    string funcRep = content.Substring(content.IndexOf('=') + 1);
                    string[] tmp = content.Substring(leftPos + 1, rightPos - leftPos - 1).Split(',');
                    CustomVariablesAndFunctions.AddFunc(funcName, funcRep, ref tmp);

                }
                else
                {
                    string varName = content.Substring(0, content.IndexOf('='));
                    string varRep = content.Substring(content.IndexOf('=') + 1);
                    CustomVariablesAndFunctions.AddVar(varName, varRep);

                }
            }
            else
            {
                GrammarSymobl temp = new GrammarSymobl();
                temp.symbol = new List<string>();
                temp.posibility = new List<float>();
                string posIfo="";
                content = content.Replace(" ", "");
                //是否設定機率
                if (content.IndexOf("#") != -1) 
                {
                    posIfo = content.Substring(content.IndexOf("#") + 1);
                    if (posIfo != "")
                    {
                        content = content.Substring(0, content.IndexOf("#"));
                        string[] posi = posIfo.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                        foreach (string i in posi)
                            temp.posibility.Add(float.Parse(i));
                    }
                }
                //設定grammar symbol
                temp.name = content.Substring(0, content.IndexOf("->"));
                string []sym = content.Substring(content.IndexOf("->") + 2).Split(new char[] {'|'},System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string i in sym)
                    temp.symbol.Add(i);
                for(int i=0;i<symobls.Count;i++)
                {
                    if(symobls[i].name==temp.name)
                    {
                        symobls.Remove(symobls[i]);
                        break;
                    }
                }
                symobls.Add(temp);
            }
            ClearScrollViewContent();
            ScrollViewDisplay();
            
        }
        return customize;
    }

    //Check value after ^  is whether constant, parentheses is balance.
    //return true if operation succeed.
    public static bool Check(string content)
    {
        Debug.Log("Entered Check");
        if (string.IsNullOrEmpty(content))
            return false;
        string sample = content;
        bool flag = true;
            for (int j=0;j<sample.Length;j++)
            {
             
                if(sample[j]==')')
                {
                    flag = false;
                    for(int k=j-1;k>=0;k--)
                    {
                        if(sample[k]=='(')
                        {
                            flag = true;
                            sample=sample.Remove(k, j - k+1);
                        j = 0;
                        break;
                        }
                    }
                }
          
            }
        
        return flag;
    }
    public string CustomFunctionAndVariale(string final)
    {
        Debug.Log("Entered CustomFunction");
        int rightPos = 0, leftPos = 0;
        //deal with custom function
        for (int i = 0; i < CustomVariablesAndFunctions.CustomFunctionSet.Count; i++)
        {
            int pos = final.IndexOf(CustomVariablesAndFunctions.CustomFunctionSet[i].Name); //先找FUNCTION名稱
            while (pos != -1)
            {
                rightPos = final.IndexOf(')', pos); //找右括弧
                leftPos = pos + CustomVariablesAndFunctions.CustomFunctionSet[i].Name.Length; //左括弧=function開頭+name長度。
                string replace = CustomVariablesAndFunctions.CustomFunctionSet[i].FuncRepresentation(final.Substring(leftPos + 1, rightPos - leftPos - 1));//將function遞歸拆解成representation
                replace = "(" + replace + ")";
                replace = CustomFunctionAndVariale(replace);//檢查representation是否有其他內容;
                final=final.Replace(final.Substring(pos, rightPos - pos + 1), replace);//進行替換
                pos = final.IndexOf(CustomVariablesAndFunctions.CustomFunctionSet[i].Name);//找尋下一個使用到的function所在位置
            }
        }
        Debug.Log("Entered CustomVariale");
        for (int i = 0; i < CustomVariablesAndFunctions.CustomVariableSet.Count; i++)//尋找變數名稱
        {
            int pos = final.IndexOf(CustomVariablesAndFunctions.CustomVariableSet[i].Name);
            if (pos != -1)
            {
                string res = CustomFunctionAndVariale(CustomVariablesAndFunctions.CustomVariableSet[i].Representation);//如果representation內含其他變數則遞歸拆解
                res = "(" + res + ")";
                final = final.Replace(CustomVariablesAndFunctions.CustomVariableSet[i].Name, res);//變數替換
            }
        }

        return final;
    }

    private void OnDestroy()
    {
        CustomVariablesAndFunctions.CustomVariableSet.Clear();
        CustomVariablesAndFunctions.CustomFunctionSet.Clear();
        ClearScrollViewContent();
    }

    public string TransformationCalculation(string content)
    {
        //Debug.Log("Enter TransformationCalculation");
        for (int i = 0; i < content.Length; i++)
        {
            //Debug.Log(content+" "+content.Length.ToString());
            if ((i + 1 < content.Length && (content[i] == 'R') && (content[i + 1] == 'x' || content[i + 1] == 'X' || content[i + 1] == 'y' || content[i + 1] == 'Y' || content[i + 1] == 'z' || content[i + 1] == 'Z')))
            {
                //Debug.Log("Entered Rotation");
                int t = 1;
                int rightPos = -1;
                int leftPos = i + 2;
                for (int j = i + 3; j < content.Length; j++)
                {
                    if (content[j] == '(')
                        t++;
                    else if (content[j] == ')')
                        t--;
                    if (t == 0)
                    {
                        rightPos = j;
                        break;
                    }
                }
                content = content.Replace(content.Substring(leftPos, rightPos - leftPos + 1), ArithmeticConstant(content.Substring(leftPos, rightPos - leftPos + 1)));
                
            }
            else if ((i==0 && content[i] == 'T' && i+1<content.Length &&content[i + 1] == '('))
            {
                
                //Debug.Log("Entered Translation");
                int t = 1;
                int rightPos = -1;
                int leftPos = i +1 ;
                for (int j = i + 2; j < content.Length; j++)
                {
                    if (content[j] == '(')
                        t++;
                    else if (content[j] == ')')
                        t--;
                    if (t == 0)
                    {
                        rightPos = j;
                        break;
                    }
                }
                if(rightPos!=-1)
                {
                    string []tmp = content.Substring(leftPos+1,rightPos-leftPos-1).Split(',');
                    /*foreach (string x in tmp)
                        Debug.Log("P:"+x);*/
                    for(int j=0;j<tmp.Length;j++)
                    {
                        content=content.Replace(tmp[j], ArithmeticConstant("(" + tmp[j] + ")"));
                        Debug.Log(content);
                    }
                }
            }
            else if(i - 1 >= 0 && i + 1 < content.Length && content[i] == 'T' && content[i + 1] == '(' && (content[i - 1] == '*' || content[i - 1] == '('))
            {
                int t = 1;
                int rightPos = -1;
                int leftPos = i + 1;
                for (int j = i + 2; j < content.Length; j++)
                {
                    if (content[j] == '(')
                        t++;
                    else if (content[j] == ')')
                        t--;
                    if (t == 0)
                    {
                        rightPos = j;
                        break;
                    }
                }
                if (rightPos != -1)
                {
                    string[] tmp = content.Substring(leftPos + 1, rightPos - leftPos - 1).Split(',');
                    /*foreach (string x in tmp)
                        Debug.Log("P:" + x);*/
                    for (int j = 0; j < tmp.Length; j++)
                    {
                        content = content.Replace(tmp[j], ArithmeticConstant("(" + tmp[j] + ")"));
                        //Debug.Log(content);
                    }
                }
            }
        }
        return content;
    }

    public string Exponential(string final)
    {
        Debug.Log("Entered Exponential");
        int rightPos = 0, leftPos = 0;
        int expPos;
        expPos = final.IndexOf('^');
        Debug.Log(expPos);
        while (expPos != -1)
        {
          
            string times = "";//獲取指數
            if (final[expPos+1]=='(')
            {
                int totalL = 1,expR=-1;
                for(int i=expPos+2;i<final.Length;i++)
                {
                    if (final[i] == '(')
                        totalL++;
                    else if (final[i] == ')')
                        totalL--;
                    if (totalL == 0)
                    {
                        expR = i;
                        break;
                    }
                }
                string replace = final.Substring(expPos + 1, expR - expPos);
                final=final.Replace(replace, ArithmeticConstant(replace));
                Debug.Log(final);
            }
            for (int i = 1; expPos+i<final.Length; i++)
            {
                if ((final[expPos + i] >= '0' && final[expPos + i] <= '9'))
                    times += final[expPos + i];
                else
                    break;
            }
            final=final.Remove(expPos, times.Length + 1);//移除^n
            if (final[expPos - 1] == ')')//若為括號
            {
                //取得左右括號位置
                int t = 1;
                rightPos = expPos - 1;
                for (int i = rightPos - 1; i >= 0; i--)
                {
                    if (final[i] == '(')
                        t--;
                    else if (final[i] == ')')
                        t++;
                    if (t == 0)
                    {
                        leftPos = i;
                        break;
                    }
                }
                string tmp;
                if (leftPos-1>=0&&(final[leftPos - 1] == 'T' || final[leftPos - 1] == 'R'))
                {
                    tmp = final.Substring(leftPos - 1, rightPos - leftPos + 2);//獲取內容
                    tmp += "*";
                    for (int i = 0; i < int.Parse(times) - 1; i++)
                    {
                        final = final.Insert(leftPos - 1, tmp);
                    }
                }
                else
                {
                    tmp = final.Substring(leftPos + 1, rightPos - leftPos - 1);//獲取內容
                   
                    //final=final.Remove(rightPos, 1);
                    tmp = Exponential(tmp);
                    tmp += "*";
                    for (int i = 0; i < int.Parse(times) - 1; i++)
                    {
                        final = final.Insert(leftPos + 1, tmp);
                    }
                    //final=final.Remove(leftPos, 1);
                }
            }
            else//若非括號
            {
                string tmp = final.Substring(expPos - 1, 1) + "*";
                for (int i = 0; i < int.Parse(times) - 1; i++)
                {
                    final=final.Insert(expPos - 1, tmp);
                }
            }
            expPos = final.IndexOf('^');//尋找指數符號
        }

        return final;
    }

    float getNum(string str, ref int i)
    {
        float num = 0f;
        bool neg = false;
        for (; str[i] > '9' || str[i] < '0'; i++) 
        if (str[i - 1] == '-') neg = true;
        for (; str[i] <= '9' && str[i] >= '0'; i++)
            num = num * 10f + str[i] - '0';
        if (neg) return -num;
        else return num;
    }

    string doTransform(string str)
    {
        string trans = null;
        int start = 0, end = 0;
        for (; bigTransformExist(str, ref start, ref end, ref trans); str = Trans(str, start, end, trans)) ;
        return str;
    }

    string Trans(string iStr, int start, int end, string trans)
    {
        string oStr = null;
        int count = 0;
        for (int i = 0; i < start; oStr += iStr[i], i++) ;
        oStr += trans;
        for (; end < iStr.Length; end++)
        {
            oStr += iStr[end];
            if (iStr[end] == '(') count++;
            else if (iStr[end] == ')')
            {
                if (count == 0) break;
                else count--;
            }
            else if (iStr[end] == '+' && count == 0)
                oStr += trans;
        }
        oStr = oStr.Remove(oStr.Length - 1);
        for (end += 1; end < iStr.Length; oStr += iStr[end], end++) ;
        return oStr;
    }

    bool bigTransformExist(string str, ref int start, ref int end, ref string trans)
    {
        trans = null;
        bool isFirstTime = true;
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == 'T' || str[i] == 'R')
            {
                if (isFirstTime)
                {
                    start = i;
                    isFirstTime = false;
                }
                trans += str[i];
                for (; str[i] != '*'; i++, trans += str[i]) ;
                if (str[i + 1] == '(')
                {
                    end = i + 2;
                    return true;
                }
            }
            else
            {
                isFirstTime = true;
                trans = null;
            }
        }
        return false;
    }

    public void previousStep()
    {
        if(previousRec.Count > 1)
        {
            for (int i = 0; i < stateBtn.Count; i++)
                Destroy(stateBtn[i].gameObject);
            stateBtn.Clear();
            for (int i = 0; i < stateList.Count; i++)
                Destroy(stateList[i].gameObject);
            stateList.Clear();

            previousRec.Pop();
            inputField.GetComponent<InputField>().text = previousRec.Peek();
            Parse();
            previousRec.Pop();
        }
        /*
        if (blockStack.Count != 0)
        {
            Destroy(blockStack.Pop());
            if (isFromInputField.Pop())
                for (I -= 1; Content[I] != 'h' && Content[I] != 's' && Content[I] != 't' && Content[I] != 'd' && Content[I] != 'a' && Content[I] != 'y'; I--) ;            
        }
            
        
        if(blockStack.Count != 0)
        {
            currentPosition = blockStack.Peek().transform.position;
            currentRotation = blockStack.Peek().transform.rotation;
        }
        else
        {
            currentPosition = new Vector3(0f, 0f, 0f);
            currentRotation = Quaternion.identity;
        }
        if(stringForInputField.Length > 1)
            stringForInputField = stringForInputField.Remove(stringForInputField.Length - 2);
        else
            stringForInputField = "";
        inputField.GetComponent<InputField>().text = stringForInputField;*/
    }

    public void stepByStep()
    {
        if(I == 0)  clear();

        for (; I < Content.Length; I++)
        {
            if (isStart)
            {
                isStart = false;
                currentPosition = new Vector3(0f, 0f, 0f);
                currentRotation = new Quaternion(0f, 0f, 0f, 0f);
            }
            else if (currentModel != null)
            {
                currentPosition = currentModel.transform.position;
                currentRotation = currentModel.transform.rotation;
            }

            switch (Content[I])
            {
                case 'h':
                    engageH();
                    isFromInputField.Push(true);
                    break;
                case 's':
                    engageS();
                    isFromInputField.Push(true);
                    break;
                case 't':
                    engageT();
                    isFromInputField.Push(true);
                    break;
                case 'd':
                    engageD();
                    isFromInputField.Push(true);
                    break;
                case 'a':
                    engageA();
                    isFromInputField.Push(true);
                    break;
                case 'y':
                    engageY();
                    isFromInputField.Push(true);
                    break;
                case 'i':
                    engageI();
                    isFromInputField.Push(true);
                    break;
                case 'T':
                    float tx = getNum(Content, ref I) * 32f;
                    float ty = getNum(Content, ref I) * 32f;
                    float tz = getNum(Content, ref I) * 32f;
                    currentModel = Instantiate(nullBlock, currentPosition, currentRotation);
                    currentModel.transform.Translate(tx, ty, tz);
                    I--;
                    break;
                case 'R':
                    float rx = 0f;
                    float ry = 0f;
                    float rz = 0f;
                    I++;
                    switch (Content[I])
                    {
                        case 'x':
                        case 'X':
                            rx = getNum(Content, ref I);
                            break;
                        case 'y':
                        case 'Y':
                            ry = getNum(Content, ref I);
                            break;
                        case 'z':
                        case 'Z':
                            rz = getNum(Content, ref I);
                            break;
                    }
                    currentModel = Instantiate(nullBlock, currentPosition, currentRotation);
                    currentModel.transform.rotation *= Quaternion.Euler(rx, rz, ry);
                    I--;    
                    break;
                case '+':
                    isStart = true;
                    toWhite();
                    break;
                default:
                    break;
            }
            if (currentModel != null)
                currentModel.tag = "Pair";
            if(Content[I] == 'h' || Content[I] == 's' || Content[I] == 't' || Content[I] == 'd' || Content[I] == 'a' || Content[I] == 'y')
            {
                I++;
                break;
            }
        }
    }

    void afterEngage()
    {
        currentModel.tag = "Pair";
        currentPosition = currentModel.transform.position;
        currentRotation = currentModel.transform.rotation;
        foreach (Transform block in currentModel.transform)
            block.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        blockStack.Push(currentModel);
        Cam.GetComponent<MoveCamera>().ObjCentroid = Cam.GetComponent<MoveCamera>().ObjCentroid * TotalPairs;
        Cam.GetComponent<MoveCamera>().ObjCentroid += transform.TransformVector(currentModel.transform.position);
        Cam.GetComponent<MoveCamera>().ObjCentroid /= (++TotalPairs);
        
        Cam.GetComponent<MoveCamera>().centroid = Cam.GetComponent<MoveCamera>().ObjCentroid;
        if (stringForInputField[0] == '*')
            stringForInputField = stringForInputField.Remove(0, 1);
        inputField.GetComponent<InputField>().text = stringForInputField;
    }
    void toWhite()
    {
        if (currentModel != null)
            foreach (Transform block in currentModel.transform)
                block.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.796f, 0.827f, 0.851f));
    }

    public void engageI()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation) as GameObject;
        stringForInputField += "*i";
        afterEngage();
    }

    public void engageH()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation) as GameObject;      
        currentModel.transform.Translate(64f, 0f, 0f);
        currentModel.transform.rotation *= Quaternion.Euler(180f, 0f, 0f);
        stringForInputField += "*h";
        afterEngage();
    }
    public void engageS()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation);
        currentModel.transform.Translate(32f, -32f, 32f);
        currentModel.transform.rotation *= Quaternion.Euler(180f, -90f, 0f);
        stringForInputField += "*s";
        afterEngage();
    }
    public void engageT()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation);
        currentModel.transform.Translate(32f, 32f, -32f);
        currentModel.transform.rotation *= Quaternion.Euler(180f, 90f, 0f);
        stringForInputField += "*t";
        afterEngage();
    }
    public void engageD()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation);
        currentModel.transform.Translate(64f, -32f, 0f);
        stringForInputField += "*d";
        afterEngage();
    }
    public void engageA()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation);
        currentModel.transform.Translate(32f, 0f, -32f);
        currentModel.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);
        stringForInputField += "*a";
        afterEngage();
    }
    public void engageY()
    {
        toWhite();
        currentModel = Instantiate(pair, currentPosition, currentRotation);
        currentModel.transform.Translate(32f, -64f, 32f);
        currentModel.transform.rotation *= Quaternion.Euler(0f, -90f, 0f);
        stringForInputField += "*y";
        afterEngage();       
    }

    public void isFromBtn()
    {
        isFromInputField.Push(false);
    }
    //operator + * () ^

    void btnClick(string content)
    {
        stateName.text = "";
        inputField.GetComponent<InputField>().text = content;

        for (int i = 0; i < stateBtn.Count; i++)
            Destroy(stateBtn[i].gameObject);
        stateBtn.Clear();
        for (int i = 0; i < stateList.Count; i++)
            Destroy(stateList[i].gameObject);
        stateList.Clear();
        Parse();
        inputField.GetComponent<InputField>().text = content;
    }

    public void posibilityGeneration(string text)
    {
        int maximunAmount = int.Parse(MaximunAmountInputField.GetComponent<InputField>().text);
        int roof = 0;
        GrammarSymobl target= symobls.Find(delegate (GrammarSymobl g) { return g.name == text; });
        for(int i=0;i<target.posibility.Count;i++)
        {

        }
    }

    public void random()
    {
        /*
        for (int i = 0; i < symobls.Count; i++)
        {
            symobls[i].probability = new List<int>();
            if (symobls[i].name == "(Tree)")
            {               
                symobls[i].probability.Add(100);
            }
            else if (symobls[i].name == "(L)")
            {
                symobls[i].probability.Add(40);
                symobls[i].probability.Add(60);
            }
            else if (symobls[i].name == "(S)")
            {
                symobls[i].probability.Add(25);
                symobls[i].probability.Add(15);
                symobls[i].probability.Add(15);
                symobls[i].probability.Add(15);
                symobls[i].probability.Add(15);
                symobls[i].probability.Add(15);
            }
        }
        */
        inputField.GetComponent<InputField>().text = updateState(inputField.GetComponent<InputField>().text, 0, "", "");

        clearStatePrefab();
        Parse();
    }

    string updateState(string str, int depth, string front, string back)
    {
        Debug.Log("Depth: " + depth + ", " + front + ' ' + str + ' ' + back);

        if (depth == int.Parse(MaximunAmountInputField.GetComponent<InputField>().text))
            return "";

        while (str.IndexOf('(') != -1)
        {
            // sta = state name, ex: (S)
            string sta = str.Substring(str.IndexOf('('), str.IndexOf(')') - str.IndexOf('(') + 1);

            // Find index (which state).
            int index = -1;
            for (int i = 0; i < symobls.Count; i++)
                if (symobls[i].name == sta)
                {
                    index = i;
                    break;
                }
            if (index == -1) return "";

            string newState = "";
            if(symobls[index].posibility.Count == 0)
                newState = symobls[index].symbol[Random.Range(0, symobls[index].symbol.Count)];
            else
            {
                float max = 0f;
                for (int i = 0; i < symobls[index].posibility.Count; i++)
                    max += symobls[index].posibility[i];
                float rnd = Random.Range(0f, max);
                float count = 0;

                for (int i = 0; i < symobls[index].symbol.Count; i++)
                {
                    if (rnd >= count && rnd < count + symobls[index].posibility[i])
                    {
                        newState = symobls[index].symbol[i];
                        break;
                    }
                    count += symobls[index].posibility[i];
                }
            }
            

            string f = front + str.Substring(0, str.IndexOf('('));
            string b = str.Substring(str.IndexOf(')') + 1) + back;
            str = str.Substring(0, str.IndexOf('(')) + updateState(newState, depth + 1, f, b) + str.Substring(str.IndexOf(')') + 1);
        }

        return str;
    }

    void clearStatePrefab()
    {
        for (int i = 0; i < stateBtn.Count; i++)
            Destroy(stateBtn[i].gameObject);
        stateBtn.Clear();
        for (int i = 0; i < stateList.Count; i++)
            Destroy(stateList[i].gameObject);
        stateList.Clear();
    }
}
