using UnityEngine;


namespace NimbleGames.BackgroundAssets
{
    public class ButtonManager : MonoBehaviour
    {
        public Button[] buttons; // an array of all the buttons we want to manage
        private int enabledButtonIndex = -1; // the index of the currently enabled button (-1 means none are enabled)

        void Start()
        {
            // add event listeners for each button so we can detect when they are clicked
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i; // we need to create a new variable here to avoid the "closure problem"
                buttons[i].OnButtonClicked += () => { OnButtonClicked(index); };
            }
        }

        void OnButtonClicked(int index)
        {
            // perform the action associated with the clicked button
            buttons[index].SwapTiles();
        }

        public int GetEnabledButtonIndex()
        {
            return enabledButtonIndex;
        }
    }
}