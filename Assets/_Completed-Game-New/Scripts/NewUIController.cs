using UnityEngine;
using UnityEngine.UI;
using Dsu.Framework;

public class NewUIController : UIControllerBase
{
    public Text countText;
    public Text winText;

    protected override void UpdateData(int groupId)
    {
        // TODO: Implement your UI update logic here
        // read data from your RuntimeGameDataManager
        SetCountText();
    }

    private void Start()
    {
        // Run the SetCountText function to update the UI (see below)
        SetCountText();

        // Set the text property of our Win Text UI to an empty string, making the 'You Win' (game over message) blank
        winText.text = "";
    }

    // Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
    void SetCountText()
    {
        int count = NewRuntimeGameDataManager.instance.Count;
        // Update the text field of our 'countText' variable
        countText.text = "Count: " + count.ToString();

        // Check if our 'count' is equal to or exceeded 12
        if (count >= 12) {
            // Set the text value of our 'winText'
            winText.text = "You Win!";
        }
    }
}
