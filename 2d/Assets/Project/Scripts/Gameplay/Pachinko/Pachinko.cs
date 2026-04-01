using UnityEngine;
using System.Collections.Generic;

public class Pachinko : MonoBehaviour
{
    public Pachinko_data items;
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
                    // 그 밖에 함수 호출해야지 무? 슬롯 보이게 하는 함수 
                    break;
                } 
            }
        }
        Debug.Log(value);
        return value;
    }
    

}
