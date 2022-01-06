using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sharp7;
using System;

public class toggleswitch : MonoBehaviour
{
    [SerializeField] string machineName;
    ConnectToPLC machineComms;
    [SerializeField] string signal_name;
    [SerializeField] short iiot_access_mode = 1;
    Toggle toggle;
    [SerializeField] public bool boolValue;
    [SerializeField] RectTransform uihandleARectTransform;
    Vector2 handleposition;
    bool valueChanged;
    void Awake()
    {
        toggle = GetComponent<Toggle>();
        handleposition = uihandleARectTransform.anchoredPosition;
        toggle.onValueChanged.AddListener (IIoTWrite);
    }

    private void IIoTWrite(bool on)
    {
        if (on)
        {
            boolValue = !boolValue;
            uihandleARectTransform.anchoredPosition = handleposition * 13;
            machineComms.PLCWrite(signal_name, boolValue.ToString());
        }
        else
        {
            boolValue = !boolValue;
            uihandleARectTransform.anchoredPosition = handleposition;
            machineComms.PLCWrite(signal_name, boolValue.ToString());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        machineComms = GameObject.Find(machineName).GetComponent<ConnectToPLC>();
    }

    // Update is called once per frame
   
}
