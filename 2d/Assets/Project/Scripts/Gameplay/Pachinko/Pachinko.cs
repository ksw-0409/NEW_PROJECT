using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class Pachinko : MonoBehaviour
{
    //데이터 저장용 
    public Pachinko_data items;

    //멈출 숫자 저장용
    private int[] values = new int[3];

    private int value=0;

    //릴 조작 
    public GameObject[] Reals;
    
    void OnEnable()
    {
        value = 0;
        for (int i = 0; i < 3; i++) values[i] = 0;
    }

    public void StartPachinko()
    {
        StartReal();
        value=GetRandomValue();
        StopAllReels();
    }

    //랜덤 당첨
    private int GetRandomValue()
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
                    values[i] = items.items[j].itemValue;
                    Debug.Log(value);
                    break;
                } 
            }
        }
        Debug.Log(value);
        return value;
    }

    private void StartReal()
    {
        for (int i = 0; i < Reals.Length; i++)
        {
            Reals[i].GetComponent<Pachinko_Real>().StartSpin();
        }
    }
    private void StopAllReels()
    {
        for (int i = 0; i < Reals.Length; i++)
        {
            Reals[i].GetComponent<Pachinko_Real>().RequestStop(values[i], 2.0f+ (float)i);
        }
    }

}
