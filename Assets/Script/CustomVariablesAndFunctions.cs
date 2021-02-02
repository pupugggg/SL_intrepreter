using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public class CustomVariablesAndFunctions
{
    public static List<CustomVariablesAndFunctions> CustomVariableSet;
    public static List<CustomVariablesAndFunctions> CustomFunctionSet;
    public List<string> ReplacementList;
    public static int lastVar;
    public static int lastFunc;
    private string CName ;
    private string representation;
    public string Name { get { return CName; } set { CName = value; } }
    public string Representation { get { return representation; } set { representation = value; } }
    public CustomVariablesAndFunctions(string nam,string rep)
    {
        this.CName = nam;
        this.representation = rep;
        ReplacementList = new List<string>();
    }
    public static void AddVariable(string nam, string rep)
    {
        CustomVariableSet.Add(new CustomVariablesAndFunctions(nam, rep));
    }

    public static void AddFunction(string nam, string rep)
    {
        CustomFunctionSet.Add(new CustomVariablesAndFunctions(nam, rep));
    }

    public static void AddFunc(string Nam,string rep,ref string[] replacement)
    {
        
        for(int i=0;i<CustomFunctionSet.Count;i++)
        {
            if(CustomFunctionSet[i].CName==Nam)
            {
                CustomFunctionSet[i].ReplacementList.Clear();
                for (int j = 0; j < replacement.Length; j++)
                    CustomFunctionSet[i].ReplacementList.Add(replacement[j]);
                CustomFunctionSet[i].representation = rep;
                Debug.Log(CustomFunctionSet[i].CName + " " + CustomFunctionSet[i].representation);
                return;
            }
        }
        CustomFunctionSet.Add(new CustomVariablesAndFunctions(Nam, rep));
        for (int j = 0; j < replacement.Length; j++)
            CustomFunctionSet[lastFunc].ReplacementList.Add(replacement[j]);
        Debug.Log(CustomFunctionSet[lastFunc].CName + " " + CustomFunctionSet[lastFunc].representation);
        lastFunc++;
    }

    public static void AddVar(string Nam, string rep)
    {
        
        for (int i = 0; i < CustomVariableSet.Count; i++)
        {
            if (CustomVariableSet[i].CName == Nam)
            {

                CustomVariableSet[i].representation = rep;
                Debug.Log(CustomVariableSet[i].CName + " " + CustomVariableSet[i].representation);
                return;
            }
        }
        CustomVariableSet.Add(new CustomVariablesAndFunctions(Nam, rep));
        Debug.Log(CustomVariableSet[lastVar].CName+" "+ CustomVariableSet[lastVar].representation);
        lastVar++;
    }

    public string FuncRepresentation(string content)
    {
        string[] tmp = content.Split(',');
        string ret = representation;
        for (int i = 0; i < tmp.Length; i++)
        {
            for(int j=0;j<ret.Length;j++)
            {

              if(ret[j].ToString()==ReplacementList[i]&&(j==0||(ret[j-1].ToString()+ret[j].ToString())!="Rx"&& (ret[j - 1].ToString() + ret[j].ToString()) != "RX" && (ret[j - 1].ToString() + ret[j].ToString()) != "Ry" && (ret[j - 1].ToString() + ret[j].ToString()) != "RY" && (ret[j - 1].ToString() + ret[j].ToString()) != "Rz" && (ret[j - 1].ToString() + ret[j].ToString()) != "RZ"))
              {
                    ret = ret.Remove(j, 1);
                    ret = ret.Insert(j, "(" + tmp[i] + ")");
              }
            }
            //ret = ret.Replace(ReplacementList[i], "(" + tmp[i] + ")");
        }
        return ret;
    }
    public static void Clean()
    {
        CustomFunctionSet.Clear();
        CustomVariableSet.Clear();
        lastFunc = 0;
        lastVar = 0;
    }
}