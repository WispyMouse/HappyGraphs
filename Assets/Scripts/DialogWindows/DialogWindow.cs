using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DialogWindow : MonoBehaviour
{
    public void OpenDialog()
    {
        this.gameObject.SetActive(true);
        DialogOpened();
    }

    protected virtual void DialogOpened()
    {

    }

    public void CloseDialog()
    {
        this.gameObject.SetActive(false);
    }

    protected virtual void DialogClosed()
    {

    }

    public bool IsOpen
    {
        get
        {
            return this.gameObject.activeSelf;
        }
    }

    public void ToggleDialog()
    {
        if (IsOpen)
        {
            CloseDialog();
        }
        else
        {
            OpenDialog();
        }
    }
}
