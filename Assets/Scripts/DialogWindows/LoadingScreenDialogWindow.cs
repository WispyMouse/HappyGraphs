using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenDialogWindow : DialogWindow
{
    public Text LoadingText;

    float loadingTicker { get; set; } = 0;
    int loadingTickerLevel { get; set; } = 0;

    private void Update()
    {
        if (IsOpen)
        {
            loadingTicker += Time.deltaTime;

            if (loadingTicker >= .4f)
            {
                loadingTickerLevel = (loadingTickerLevel + 1) % 4;
                loadingTicker -= .4f;

                StringBuilder loadingTicks = new StringBuilder();

                for (int ii = 0; ii < loadingTickerLevel; ii++)
                {
                    if (loadingTicks.Length > 0)
                    {
                        loadingTicks.Append(" ");
                    }

                    loadingTicks.Append(".");
                }

                LoadingText.text = $"Loading\r\n{loadingTicks.ToString()}";
            }
        }
    }
}
