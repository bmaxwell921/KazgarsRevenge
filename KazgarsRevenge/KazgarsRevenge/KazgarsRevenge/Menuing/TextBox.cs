using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using KazgarsRevenge;

/// <summary>
/// Taken from http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game
/// </summary>
public class TextBox : IKeyboardSubscriber
{
    Texture2D _textBoxTexture;
    Texture2D _caretTexture;

    SpriteFont _font;

    private string test = "Enter Name";

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; private set; }
    private Vector2 _guiScale;
    public Vector2 guiScale
    {
        get
        {
            return _guiScale;
        }
        set
        {
            average = (value.X + value.Y) / 2;
            _guiScale = value;
        }
    }
    private float average;

    public bool Highlighted { get; set; }

    string _text = "";
    public String Text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
            if (_text == null)
                _text = "";

            if (_text != "")
            {
                //if you attempt to display a character that is not in your font
                //you will get an exception, so we filter the characters
                String filtered = "";
                foreach (char c in value)
                {
                    if (_font.Characters.Contains(c))
                        filtered += c;
                }

                _text = filtered;

                if (_text.Length > NewAccountMenu.NAME_LENGTH_LIMIT)
                {
                    //recursion to ensure that text cannot be larger than the box
                    Text = _text.Substring(0, _text.Length - 1);
                }
            }
        }
    }

    public TextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font)
    {
        _textBoxTexture = textBoxTexture;
        _caretTexture = caretTexture;
        _font = font;

        _previousMouse = Mouse.GetState();
    }

    MouseState _previousMouse;
    public void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        Point mousePoint = new Point(mouse.X, mouse.Y);

        Rectangle position = new Rectangle(X, Y, Width, Height);
        Highlighted = position.Contains(mousePoint);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        bool caretVisible = true;

        if (Text.Length == 0 || (gameTime.TotalGameTime.TotalMilliseconds % 1000) < 500)
            caretVisible = false;
        else
            caretVisible = true;

        String toDraw = Text.Length == 0 ? test : Text;
        Height = (int) (_font.MeasureString(toDraw).Y * guiScale.Y);

        spriteBatch.Draw(_textBoxTexture, new Rectangle(X, Y, (int) (Width), (int) (Height)), Color.White);

        Vector2 size = _font.MeasureString(toDraw);
        size.X = size.X * guiScale.X;
        size.Y = size.Y * guiScale.Y;

        if (caretVisible && Selected)
        {
            spriteBatch.Draw(_caretTexture, new Rectangle((int) (X + size.X + 2), (int) ((Y + 2)), 2, (int)size.Y - 2), Color.Yellow);
        }

        //shadow first, then the actual text
        spriteBatch.DrawString(_font, toDraw, new Vector2(X, Y) + Vector2.One, Color.Black, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
        spriteBatch.DrawString(_font, toDraw, new Vector2(X, Y) + Vector2.One, Color.White, 0, Vector2.Zero, guiScale, SpriteEffects.None, 0);
    }

    public void ClearText()
    {
        _text = "";
    }

    public bool ReceiveTextInput(char inputChar)
    {
        Text = Text + inputChar;
        return true;
    }

    public bool ReceiveTextInput(string text)
    {
        Text = Text + text;
        return true;
    }

    public bool ReceiveCommandInput(char command)
    {
        if (command == '\b')
        {
            if (Text.Length > 0)
            {
                Text = Text.Substring(0, Text.Length - 1);
            }
            return true;
        }
        return false;
    }

    public bool ReceiveSpecialInput(Keys key)
    {
        return false;
    }

    public bool Selected
    {
        get;
        set;
    }
}