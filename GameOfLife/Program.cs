/*
 *
 *  Игра "Жизнь" Джона Конвея.
 *  Copyleft, Anton Petrov, 2017.
 *
 */

using System;
using System.Text;
using System.Threading;

namespace GameOfLife
{
    class Life
    {
        private const char DeadCell = '\u2591';
        private const char LiveCell = '\u2588';
        private readonly bool[,] _lifeBoard;
        private readonly bool[,] _nextBoard;
        private readonly int _boardWidth;
        private readonly int _boardHeight;
        public int Generations { get; set; }
        public int Size { get; private set; }

        public Life(int lifeSize, bool generateRandomLife = false)
        {
            _boardHeight = _boardWidth = lifeSize;
            _nextBoard = new bool[_boardHeight, _boardWidth];
            _lifeBoard = new bool[_boardHeight, _boardWidth];
            Size = lifeSize;
            if (generateRandomLife)
                InitRandommLife();
        }

        private bool IsIndexesInBounds(int i, int j)
        {
            return i < _boardHeight && j < _boardWidth && i >= 0 && j >= 0;
        }

        public void ToggleCell(int i, int j)
        {
            if (IsIndexesInBounds(i, j))
            {
                _lifeBoard[i, j] = !_lifeBoard[i, j];
            }
        }

        public bool this[int i, int j]
        {
            get
            {
                if (IsIndexesInBounds(i, j))
                {
                    return _lifeBoard[i, j];
                }
                throw new ArgumentOutOfRangeException();
            }
            set
            {
                if (IsIndexesInBounds(i, j))
                {
                    _lifeBoard[i, j] = value;
                }
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InitRandommLife()
        {
            var randGen = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < _boardHeight; i++)
            {
                for (int j = 0; j < _boardWidth; j++)
                {
                    _lifeBoard[i, j] = (randGen.Next(2) == 0);
                }
            }
        }

        /// <summary>
        /// Конвертирует игровое поле в строку, пригодную для отображения в консоли.
        /// Используются символы псевдографики.
        /// </summary>
        /// <returns>Игровое поле в виде строки (матрица).</returns>
        public string GetBoard()
        {
            var boardString = new StringBuilder();
            for (var y = 0; y < _boardHeight; y++)
            {
                for (var x = 0; x < _boardWidth; x++)
                {
                    char c = _lifeBoard[y, x] ? LiveCell : DeadCell;
                    // Рисуем символ клетки дважды, чтобы она получилась квадратной.
                    boardString.Append(c);
                    boardString.Append(c);
                }
                boardString.Append('\n');
            }
            return boardString.ToString();
        }

        /// <summary>
        /// Генерация нового поколения, проверка на предмет окончания игры.
        /// </summary>
        /// <returns>Возвращает true, если было сгенерировано новое поколение, и оно отличается от старого.
        /// В случае зацикливания возвращается false.</returns>
        public bool NextGeneration()
        {
            for (int row = 0; row < _boardHeight; row++)
            {
                for (int column = 0; column < _boardWidth; column++)
                {
                    var n = NumberOfAliveNeighbors(row, column);
                    if ((!_lifeBoard[row, column] && n == 3) || (_lifeBoard[row, column] && (n == 2 || n == 3)))
                    {
                        // зарождается жизнь или продолжается жизнь
                        _nextBoard[row, column] = true;
                    }
                    else
                    {
                        // здесь жизни нет :)
                        _nextBoard[row, column] = false;
                    }
                }
            }

            // проверка на окончание игры - поколение перестает изменяться
            bool haveNewGeneration = false;
            for (int row = 0; row < _boardHeight; row++)
            {
                for (int column = 0; column < _boardWidth; column++)
                {
                    if (_lifeBoard[row, column] != _nextBoard[row, column])
                    {
                        haveNewGeneration = true;
                        break;
                    }
                }
                if (haveNewGeneration)
                    break;
            }
            Array.Copy(_nextBoard, _lifeBoard, _boardHeight * _boardWidth);
            Generations++;
            return haveNewGeneration;
        }

        // Вычисление количества живых соседей. Игровая плоскость как-бы замкнута в тор, т.е. можно выйти за пределы.
        // Например, использовать отрицательные индексы, индексы любой размерности. Получается бесконечная поверхность.
        //
        private int NumberOfAliveNeighbors(int y, int x)
        {
            int result = 0;
            for (int j = -1; j <= 1; j++)
            {
                int row = (y + j + _boardHeight) % _boardHeight;
                for (int i = -1; i <= 1; i++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    int column = (x + i + _boardWidth) % _boardWidth;
                    result += _lifeBoard[row, column] ? 1 : 0;
                }
            }
            return result;
        }
    }

    class Program
    {
        private const int StepDelay = 50;
        private const ConsoleColor DeadColor = ConsoleColor.Black;
        private const ConsoleColor LiveColor = ConsoleColor.Cyan;
        private const int GameSize = 45;

        static void Main(string[] args)
        {
            InitConsole(GameSize, GameSize);
            Life life = new Life(GameSize, generateRandomLife: true);
//            life[0, 1] = true;
//            life[1, 2] = true;
//            life[2, 0] = life[2, 1] = life[2, 2] = true;
            do
            {
                Console.SetCursorPosition(0, 0);
                Console.Write(life.GetBoard());
                Thread.Sleep(StepDelay);
                Console.Title = $"Game of Life: {life.Size}x{life.Size}; Number of generations: {life.Generations}\n";
            } while (life.NextGeneration());

            
            Console.ReadKey();
        }

        private static void InitConsole(int width, int height)
        {
            Console.Clear();
            Console.Title = "Game of Life " + width +"x" + height;
            Console.CursorVisible = false;
            Console.BackgroundColor = DeadColor;
            Console.ForegroundColor = LiveColor;
            int consoleWidth = width * 2 + 1;
            int consoleHeight = height + 1;
            Console.SetWindowSize(consoleWidth, consoleHeight);
            Console.SetBufferSize(consoleWidth, consoleHeight);
        }
    }
}
