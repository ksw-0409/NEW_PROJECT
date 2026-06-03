using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BettingManager : MonoBehaviour
{
    [SerializeField] private GameObject bettingPanel; 

    private bool isPaused = false;
    private bool isPlusMode = true; 

    [Header("UI ����")]
    public TMP_Text currentBetText;
    public Button goButton;
    public Button PButton;
    public Button MButton;

    [Header("��������Ʈ �迭 (0: ��Ȱ��/�⺻, 1: Ȱ��/����)")]
    public Sprite[] Pbs;
    public Sprite[] Mbs;

    // ���� ����
    private int playerTotalGold = 0;
    private int currentBetAmount = 0;

    [Header("��Ī�� �ý��� ����")]
    public GameObject Pachinko;
    public Pachinko pachinko;

    // ��Ī�� ���� â�� ���� ��
    void OnEnable()
    {
        BGMManager.Instance.SetMusic(3);
        setUp();
        GameDataManager.Instance.isPachinkoActive = true;

        // ���� ��� �ֽ�ȭ
        playerTotalGold = GameDataManager.Instance.Gold;

        // �ʱ�ȭ
        ResetBet();
    }

    // +100, -100(�ν����Ϳ��� ��� 100, 1000 �Է� ����) ��ư Ŭ�� �� ȣ��
    public void ChangeBetAmount(int amount)
    {
        // �׻� �Ű������� ����� �ް�, ���̳ʽ� ����� ���� ������ ��ȯ�ϴ� ���� ������
        int finalAmount = Mathf.Abs(amount);

        if (!isPlusMode)
        {
            finalAmount = -finalAmount;
        }

        currentBetAmount += finalAmount;

        // ���� �ݾ� �Ѱ� ���� (0 ~ ���� ���)
        currentBetAmount = Mathf.Clamp(currentBetAmount, 0, playerTotalGold);

        UpdateUI();
    }

    // ���� (All-In)
    public void SetAllIn()
    {
        currentBetAmount = playerTotalGold;
        UpdateUI();
    }

    // ���� (Reset)
    public void ResetBet()
    {
        currentBetAmount = 0;
        UpdateUI();
    }

    // �÷��� ��� ����
    public void setUp()
    {
        isPlusMode = true;
        if (PButton != null && Pbs.Length > 1) PButton.image.sprite = Pbs[1];
        if (MButton != null && Mbs.Length > 0) MButton.image.sprite = Mbs[0];
    }

    // ���̳ʽ� ��� ����
    public void setM()
    {
        isPlusMode = false;
        if (PButton != null && Pbs.Length > 0) PButton.image.sprite = Pbs[0];
        if (MButton != null && Mbs.Length > 1) MButton.image.sprite = Mbs[1];
    }

    // ���� ���� (Go) ��ư
    public void StartPachinko()
    {
        // [����] ���� �˻縦 ���� �� �ڿ� â�� ���� ���װ� �� ����ϴ�.
        if (currentBetAmount > 0 && currentBetAmount <= playerTotalGold)
        {
            Debug.Log($"��Ī�� ����! ���õ� �ݾ�: {currentBetAmount}");

            this.gameObject.SetActive(false); // �������� ���� ����â ���
            Pachinko.SetActive(true);
            pachinko.SetupBetAmount(currentBetAmount);
        }
        else
        {
            Debug.LogError("���� �ݾ��� 0���̰ų� ���� ��尡 �����մϴ�.");
        }
    }

    public void exitButtn()
    {
        GameDataManager.Instance.isPachinkoActive = false;
        this.gameObject.SetActive(false);
    }

    // UI ����
    private void UpdateUI()
    {
        if (currentBetText != null)
            currentBetText.text = $"{currentBetAmount}";

        if (goButton != null)
        {
            // 0���� ���� ��ư Ŭ�� �Ұ����ϰ� ��� �ڵ�
            goButton.interactable = currentBetAmount > 0;
        }
    }

    // ����â ��� �Ѱ� ���
    public void Gobetting()
    {
        isPaused = !isPaused;
        if (bettingPanel != null) bettingPanel.SetActive(isPaused);
    }
}