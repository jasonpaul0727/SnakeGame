///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8
///
///This Class represents the View of our game. Being resposible of calling the drawing methods and showing messages and interactions with the user.
namespace SnakeGame;

public partial class MainPage : ContentPage
{
    Controller gc = new Controller();
    public MainPage()
    {
        //initialize component, and set the worldpanel world to the controller version of world, also check for update after server communication
        InitializeComponent();
        graphicsView.Invalidate();
        worldPanel.SetWorld(gc.GetWorld());
        gc.UpdateArrived += OnFrame;
    
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    /// <summary>
    /// Method that will communicate the text changes to the controller, using the MessageEntered method
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            gc.MessageEntered("{\"moving\":\"up\"}");
        }
        else if (text == "a")
        {
            gc.MessageEntered("{\"moving\":\"left\"}");
        }
        else if (text == "s")
        {
            gc.MessageEntered("{\"moving\":\"down\"}");
        }
        else if (text == "d")
        {
            gc.MessageEntered("{\"moving\":\"right\"}");
        }
        entry.Text = "";
    }

    private void NetworkErrorHandler()
    {
        DisplayAlert("Error", "Disconnected from server", "OK");
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (serverText.Text != "localhost")
        {
            DisplayAlert("Error", "Please enter a valid server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }
        //if connection is successfull, the button will be disabled and the controller method for connct will be called.
        connectButton.IsEnabled = false;
        serverText.IsEnabled = false;
        gc.Connect(serverText.Text, nameText.Text);

        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        //onFrame method that updates the graphics drawing.
        lock (gc)
        {
            Dispatcher.Dispatch(() => graphicsView.Invalidate());
        }
    }
    private void ShowError(string err)
    {
        // Show the error
        Dispatcher.Dispatch(() => DisplayAlert("Error", err, "OK"));

        // Then re-enable the controlls so the user can reconnect
        Dispatcher.Dispatch(
          () =>
          {
              connectButton.IsEnabled = true;
              serverText.IsEnabled = true;
          });
    }
    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by ...\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }
}