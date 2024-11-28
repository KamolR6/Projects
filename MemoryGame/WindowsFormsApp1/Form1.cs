using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public class GameState
    {
        public List<int> GridNumbers { get; set; } 
        public List<int> RevealedPairs { get; set; } 
        public int ElapsedTime { get; set; } 
    }

    public partial class Form1 : Form
    {
        private Panel gamePanel;
        private List<int> numbers;
        private Button firstButton, secondButton;
        private Timer timer, gameTimer;
        private Label timerLabel;
        private int elapsedTime;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        /*sumary
         * Creation of dynamic timer (why not lol)
         * Creation of panel where buttons are placed
         * creation of two timers => for number checking and for main timer
         * summary*/
        private void InitializeGame()
        {
            // Timer Label
            timerLabel = new Label
            {
                Location = new Point(50, 50),
                Size = new Size(100, 30),
                Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold),
                Text = "Time: 0",
                BackColor = Color.Transparent
            };
            Controls.Add(timerLabel);

            gamePanel = new Panel
            {
                Location = new Point(this.Width / 3, 50),
                Size = new Size(400, 400),
                BackColor = Color.LightGray,
                Visible = false
            };
            Controls.Add(gamePanel);

            // Timer for hiding numbers
            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += Timer_Tick;

            // Timer for tracking game time
            gameTimer = new Timer
            {
                Interval = 1000
            };
            gameTimer.Tick += GameTimer_Tick;
        }

        /*sumary
         Start button function which start game
         summary*/
        private void StartButton_Click(object sender, EventArgs e)
        {
            Button startButton = sender as Button;
            if (startButton != null)
            {
                startButton.Enabled = false;
            }

            // Reset and start the game timer
            elapsedTime = 0;
            timerLabel.Text = "Time: 0";
            gameTimer.Start();

            gamePanel.Visible = true;
            InitializeGrid();

            // Enable the Reset button
            Button resetButton = Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Reset");
            if (resetButton != null)
            {
                resetButton.Enabled = true;
            }
        }

        /*sumary
         Initialization of number list (js in case so its not crashing kek)
         Spawning random buttons
         */
        private void InitializeGrid()
        {
            if (numbers == null)
            {
                numbers = Enumerable.Range(1, 32).SelectMany(i => new[] { i, i }).OrderBy(_ => Guid.NewGuid()).ToList();
            }

            gamePanel.Controls.Clear();
            int buttonSize = 50;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = row * 8 + col;
                    Button button = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(col * buttonSize, row * buttonSize),
                        Tag = numbers[index], 
                        Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold),
                        BackColor = Color.White,
                        Text = ""
                    };
                    button.Click += Button_Click;
                    gamePanel.Controls.Add(button);
                }
            }
        }


        /*sumary
         Activating function to check if button a and button b are having same value
         */
        private void Button_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            // Jeśli przycisk już ma tekst, zignoruj kliknięcie
            if (clickedButton == null || clickedButton.Text != "" || timer.Enabled)
                return;

            // Ustaw pierwszy przycisk
            clickedButton.Text = clickedButton.Tag.ToString();

            if (firstButton == null)
            {
                firstButton = clickedButton;
            }
            else
            {
                secondButton = clickedButton;
                CheckMatch();
            }
        }

        /*sumary
         Function to check if button a and button b are having same value
         Game end function
         */
         
        private void CheckMatch()
        {
            if (firstButton.Tag.ToString() == secondButton.Tag.ToString())
            {
                firstButton = null;
                secondButton = null;

                if (gamePanel.Controls.OfType<Button>().All(b => b.Text != ""))
                {
                    gameTimer.Stop(); // Stop the game timer
                    MessageBox.Show($"Congratulations! You've won! Time: {elapsedTime} seconds");
                    ResetGame();
                }
            }
            else
            {
                timer.Start();
            }
        }
        /*sumary
        timer to check values
         */
        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            // Hide numbers again
            firstButton.Text = "";
            secondButton.Text = "";

            firstButton = null;
            secondButton = null;
        }
        /*sumary
         timer to make you miserable when you check it (how much you spent)
        */
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            elapsedTime++;
            timerLabel.Text = $"Time: {elapsedTime}";
        }
        /*sumary
            saving data as arrays (didnt want to download json extenstion)
            only txt are egible as saving opt as well as read opt
            when read properly message box with info
        */
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var revealedIndexes = gamePanel.Controls.OfType<Button>()
                                    .Where(b => b.Text != "") 
                                    .Select(b => gamePanel.Controls.GetChildIndex(b))
                                    .ToList();

            string gridData = string.Join(",", numbers);
            string revealedData = string.Join(",", revealedIndexes); 
            string timeData = elapsedTime.ToString();

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
                saveFileDialog.DefaultExt = "txt";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {

                        writer.WriteLine(gridData);
                        writer.WriteLine(revealedData);
                        writer.WriteLine(timeData);
                    }

                    MessageBox.Show("Game saved successfully!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text Files (*.txt)|*.txt";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                    {
                        string gridData = reader.ReadLine(); 
                        string revealedData = reader.ReadLine(); 
                        string timeData = reader.ReadLine();


                        numbers = gridData.Split(',').Select(int.Parse).ToList(); 
                        var revealedIndexes = revealedData.Split(',').Select(int.Parse).ToList(); 
                        elapsedTime = int.Parse(timeData); 

                        InitializeGrid();


                        foreach (var index in revealedIndexes)
                        {
                            var button = gamePanel.Controls[index] as Button;
                            if (button != null)
                            {
                                button.Text = button.Tag.ToString();
                            }
                        }


                        timerLabel.Text = $"Time: {elapsedTime}";

                        MessageBox.Show("Game loaded successfully!", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        /*sumary
         Reset game button functionality
        */
        private void ResetButton_Click_1(object sender, EventArgs e)
        {
            firstButton = null;
            secondButton = null;

            foreach (Button btn in gamePanel.Controls.OfType<Button>())
            {
                btn.Text = ""; 
            }

            numbers = Enumerable.Range(1, 32).SelectMany(i => new[] { i, i }).OrderBy(_ => Guid.NewGuid()).ToList();

            InitializeGrid();
        }

        private void ResetGame()
        {

            if (firstButton != null)
            {
                firstButton.Text = ""; 
            }

            if (secondButton != null)
            {
                secondButton.Text = ""; 
            }

            firstButton = null;
            secondButton = null;

            gamePanel.Controls.Clear();
            InitializeGrid();
        }
    }
}
