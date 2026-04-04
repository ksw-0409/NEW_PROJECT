using UnityEngine;
using System.Collections.Generic;

public class Pachinko : MonoBehaviour
{
    //데이터 저장용 
    public Pachinko_data items;
    //멈출 숫자 저장용
    private int[] vlaues=new int[3];

    public GameObject first;
    public GameObject second;
    public GameObject third;

    //랜덤 당첨
    int GetRandomValue()
    {
        int value = 1;
        for (int i = 0; i < 3; i++)
        {
            int RandomIndex = UnityEngine.Random.Range(1, items.GetTotalProbability()+1);
            int p = 0;
            for (int j = 0; j < items.items.Length; j++) {
                p += items.items[j].probability;
                if (RandomIndex <= p)
                {
                    value *= items.items[j].itemValue;
                    vlaues[i] = items.items[j].itemValue;
                    break;
                } 
            }
        }
        Debug.Log(value);
        return value;
    }    
    void RollingObject()
    {

    }
}
