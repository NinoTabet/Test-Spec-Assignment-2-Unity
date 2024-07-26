using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Text messageText;
    public Button[] buttons;
    public Sprite xSprite;
    public Sprite oSprite;
    private string currentPlayer;
    private string[,] board;
    private bool gameActive;
    private bool isPlayerTurn;

    void Start()
    {
        // initialize the game board 
        board = new string[3, 3];

        // set the game as active
        gameActive = true; 
        foreach (Button button in buttons)
        {
            // sets initial opacity of buttons to 0 (transparent)
            SetButtonOpacity(button, 0f); 

            // adds an OnClick listener to each button
            button.onClick.AddListener(() => OnButtonClick(button)); 
        }

        // starts the game
        StartGame(); 
    }

    void StartGame()
    {
        // randomly decide who starts
        isPlayerTurn = Random.value > 0.5f; 

        // sets the current player based on who starts
        currentPlayer = isPlayerTurn ? "X" : "O"; 

        // updates the message text to show the current player's turn
        messageText.text = isPlayerTurn ? "Player X's Turn" : "Computer O's Turn"; 

        if (!isPlayerTurn)
        {
            // starts the computer move coroutine if it's the computer's turn
            StartCoroutine(ComputerMove()); 
        }
    }

    void OnButtonClick(Button button)
    {
        Debug.Log("Button clicked: " + button.name);
        if (button.GetComponent<Image>().sprite == null && gameActive && isPlayerTurn)
        {
            Debug.Log("Making move for player: " + currentPlayer);
            
            // makes the move for the current player
            MakeMove(button, currentPlayer); 
            
            // sets button opacity to fully visible
            SetButtonOpacity(button, 1f); 

            // checks if the current player wins
            if (CheckWin()) 
            {
                messageText.text = "Player " + currentPlayer + " Wins!";
                
                // sets the game as inactive
                gameActive = false; 
                Debug.Log("Attempting to send points to API.");
                var apiHandler = FindObjectOfType<ApiHandler>();
                if (apiHandler != null)
                {
                    // sends points to the API
                    apiHandler.SendPoints(); 
                }
                else
                {
                    Debug.LogError("ApiHandler not found in the scene!");
                }
            }
            // checks if the board is full
            else if (IsBoardFull()) 
            {
                messageText.text = "It's a Draw!";

                // sets the game as inactive
                gameActive = false; 
            }
            else
            {
                // switches to the computer's turn
                isPlayerTurn = false; 
                currentPlayer = "O";
                messageText.text = "Computer O's Turn";

                // starts the computer move coroutine
                StartCoroutine(ComputerMove()); 
            }
        }
    }

    void MakeMove(Button button, string player)
    {
        // sets the button sprite to the current player's sprite
        button.GetComponent<Image>().sprite = player == "X" ? xSprite : oSprite; 
        if (TryGetButtonIndices(button.name, out int row, out int col))
        {
            // updates the board with the current player's move
            board[row, col] = player; 
        }
        else
        {
            Debug.LogError("Failed to parse button name: " + button.name);
        }
    }

    IEnumerator ComputerMove()
    {
        // delay for computer move to simulate thinking time
        yield return new WaitForSeconds(1f); 
       
        // gets all empty buttons
        Button[] emptyButtons = GetEmptyButtons(); 
        if (emptyButtons.Length > 0)
        {
            // chooses a random empty button
            var randomButton = emptyButtons[Random.Range(0, emptyButtons.Length)]; 
            
            // makes the move for the computer
            MakeMove(randomButton, currentPlayer); 
            
            // sets button opacity to fully visible
            SetButtonOpacity(randomButton, 1f); 

            // checks if the computer wins
            if (CheckWin()) 
            {
                messageText.text = "Player " + currentPlayer + " Wins!";

                // sets the game as inactive
                gameActive = false; 
            }
             // checks if the board is full
            else if (IsBoardFull())
            {
                messageText.text = "It's a Draw!";
                
                // sets the game as inactive
                gameActive = false; 
            }
            else
            {
                // switches to the player's turn
                isPlayerTurn = true; 
                currentPlayer = "X";
                messageText.text = "Player X's Turn";
            }
        }
    }

    bool CheckWin()
    {
        // checks rows and columns for a win
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] == currentPlayer && board[i, 1] == currentPlayer && board[i, 2] == currentPlayer)
                return true;
            if (board[0, i] == currentPlayer && board[1, i] == currentPlayer && board[2, i] == currentPlayer)
                return true;
        }
        // checks diagonals for a win
        if (board[0, 0] == currentPlayer && board[1, 1] == currentPlayer && board[2, 2] == currentPlayer)
            return true;
        if (board[0, 2] == currentPlayer && board[1, 1] == currentPlayer && board[2, 0] == currentPlayer)
            return true;
        return false;
    }

    bool IsBoardFull()
    {
        foreach (string cell in board)
        {
            if (string.IsNullOrEmpty(cell))
                // returns false if there is any empty cell
                return false; 
        }
        // returns true if all cells are filled
        return true; 
    }

    bool TryGetButtonIndices(string buttonName, out int row, out int col)
    {
        // splits the button name to get indices
        string[] parts = buttonName.Split(' '); 
        if (parts.Length == 2)
        {
            string[] indices = parts[1].Split('_');
            if (indices.Length == 2 &&
                int.TryParse(indices[0], out row) &&
                int.TryParse(indices[1], out col) &&
                row >= 0 && row < 3 &&
                col >= 0 && col < 3)
            {
                // successfully parsed indices
                return true; 
            }
        }
        row = -1;
        col = -1;

        // failed to parse indices
        return false; 
    }

    Button[] GetEmptyButtons()
    {
        // creates an array to hold empty buttons
        Button[] emptyButtons = new Button[buttons.Length]; 
        int count = 0;
        foreach (Button button in buttons)
        {
            if (button.GetComponent<Image>().sprite == null)
            {
                // adds empty button to the array
                emptyButtons[count] = button; 
                count++;
            }
        }

        // creates a result array of the correct size
        Button[] result = new Button[count]; 
        for (int i = 0; i < count; i++)
        {
            // copies empty buttons to the result array
            result[i] = emptyButtons[i]; 
        }
        // returns the array of empty buttons
        return result; 
    }

    void SetButtonOpacity(Button button, float opacity)
    {
        // gets the image component of the button
        Image image = button.GetComponent<Image>(); 
        if (image != null)
        {
            Color color = image.color;
            
            // sets the alpha of the button
            color.a = opacity; 
            image.color = color;
        }
    }

    public void RestartGame()
    {
        // resets the current player to "X"
        currentPlayer = "X"; 

        // reinitializes the game board
        board = new string[3, 3]; 
        
        // sets the game as active
        gameActive = true; 
        foreach (Button button in buttons)
        {
            // resets button sprites
            button.GetComponent<Image>().sprite = null; 

            // resets button opacity to 0
            SetButtonOpacity(button, 0f); 
        }

        // starts a new game
        StartGame(); 
    }
}
