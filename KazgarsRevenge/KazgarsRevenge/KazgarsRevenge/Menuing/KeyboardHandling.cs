using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Threading;
using System;
using System.Windows;
using System.Collections.Generic;

/*
 * Code taken from http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game
 */

public interface IKeyboardSubscriber
{
    // all of these return whether they consumed the event or not
    bool ReceiveTextInput(char inputChar);
    bool ReceiveTextInput(string text);
    bool ReceiveCommandInput(char command);
    bool ReceiveSpecialInput(Keys key);

    bool Selected { get; set; }
}

public class KeyboardDispatcher
{
    // Everyone subscribing to this dispatcher
    private ISet<IKeyboardSubscriber> subscribers;

    public KeyboardDispatcher(GameWindow window)
    {
        subscribers = new HashSet<IKeyboardSubscriber>();

        EventInput.EventInput.Initialize(window);
        EventInput.EventInput.CharEntered += new EventInput.CharEnteredHandler(EventInput_CharEntered);
        EventInput.EventInput.KeyDown += new EventInput.KeyEventHandler(EventInput_KeyDown);
    }

    public void Register(IKeyboardSubscriber sub)
    {
        if (sub == null)
        {
            return;
        }
        subscribers.Add(sub);
    }

    public void Unregister(IKeyboardSubscriber sub)
    {
        if (sub == null)
        {
            return;
        }
        subscribers.Remove(sub);
    }

    void EventInput_KeyDown(object sender, EventInput.KeyEventArgs e)
    {
        if (subscribers.Count == 0)
            return;
        SendAllSpecialInput(e.KeyCode);
    }

    void EventInput_CharEntered(object sender, EventInput.CharacterEventArgs e)
    {
        if (subscribers.Count == 0)
            return;
        if (char.IsControl(e.Character))
        {
            //ctrl-v
            if (e.Character == 0x16)
            {
                //XNA runs in Multiple Thread Apartment state, which cannot Receive clipboard
                Thread thread = new Thread(PasteThread);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                SendAllTextInput(_pasteResult);
            }
            else
            {
                SendAllCommandInput(e.Character);
            }
        }
        else
        {
            SendAllTextInput(e.Character);
        }
    }

    private void SendAllSpecialInput(Keys key)
    {
        foreach (IKeyboardSubscriber sub in subscribers)
        {
            if (sub.ReceiveSpecialInput(key))
            {
                break;
            }
        }
    }

    // Sends text input to all subscribers
    private void SendAllTextInput(string text)
    {
        foreach (IKeyboardSubscriber sub in subscribers)
        {
            if (sub.ReceiveTextInput(text))
            {
                break;
            }
        }
    }

    // Sends char input to all subscribers
    private void SendAllTextInput(char c)
    {
        foreach (IKeyboardSubscriber sub in subscribers)
        {
            if (sub.ReceiveTextInput(c))
            {
                break;
            }
        }
    }

    private void SendAllCommandInput(char c)
    {
        foreach (IKeyboardSubscriber sub in subscribers)
        {
            if (sub.ReceiveCommandInput(c))
            {
                break;
            }
        }
    }

    //Thread has to be in Single Thread Apartment state in order to receive clipboard
    string _pasteResult = "";
    [STAThread]
    void PasteThread()
    {
        if (Clipboard.ContainsText())
        {
            _pasteResult = Clipboard.GetText();
        }
        else
        {
            _pasteResult = "";
        }
    }
}