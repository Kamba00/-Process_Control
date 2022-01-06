using Sharp7;
using UnityEngine;
using UnityEngine.UI;

public class startbtn : MonoBehaviour
{
    [SerializeField] string machineName;
    ConnectToPLC machineComms;
    [SerializeField] string signal_name;
    [SerializeField] short iiot_access_mode = 1;
    Button button;
    [SerializeField] public bool boolValue;
    bool valueChanged;
   // Image image;
    //Color bgCol;
    string value;
    void Start()
    {
        button = GetComponent<Button>();
        machineComms = GameObject.Find(machineName).GetComponent<ConnectToPLC>();
        button.onClick.AddListener(IIoTWrite);
       // image = GetComponent<Image>();
        //bgCol = image.color;
    }

    void Update()
    {
        /*if (boolValue)
        {
            var temp = image.color;
            temp.a = 1f;
            image.color = temp;
        }
        else
        {
            var temp = image.color;
            temp.a = 0.6f;
            image.color = temp;
        }*/
    }

    void IIoTWrite()
    {

        boolValue = !boolValue;
        machineComms.PLCWrite(signal_name, boolValue.ToString());
    }

    public void SetToFalse()
    {
        boolValue = false;
    }
}
