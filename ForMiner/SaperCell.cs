using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForMiner
{
    class SaperCell
    {
        public int value;
        public int X;
        public int Y;
        //количество неоткрытых клеток рядом с данной ячейкой
        public int numberOf9TypeNeighbours = 0;

        //количество флагов, сигнализирующих о бомбе рядом с данной ячейкой
        public int numberOfFlags = 0;

        //соседние клетки, начинается отсчет с клетки над данной ячейкой, далее по часовой стрелке, заканчивается левой-верхней клеткой
        private SaperCell[] neighbours = new SaperCell[8];

        public SaperCell[] getNeighbours()
        {
            return neighbours;
        }

        public SaperCell[] get9TypeNeighbours()
        {
            SaperCell[] notOpenedCells = new SaperCell[numberOf9TypeNeighbours];
            int counter = 0;
            for (int i = 0; i < 8; ++i)
            {
                if (neighbours[i] != null && neighbours[i].value == 9)
                {
                    notOpenedCells[counter++] = neighbours[i];
                }
            }
            return notOpenedCells;
        }

        //задает соседа для данной ячейки, position - положение cell относительно данной ячейки
        //0 - она сверху данной ячейки, 4 - снизу, 6 - слева и т.д.
        public void setNeighbour(SaperCell cell, int position)
        {
            neighbours[position] = cell;
            if (cell.value == 9)
                numberOf9TypeNeighbours++;
            if (cell.value == -1)
                numberOfFlags++;
        }

        public SaperCell(int setValue, int setX, int setY)
        {
            value = setValue;
            X = setX;
            Y = setY;
            for (int i = 0; i < 8; ++i)
            {
                neighbours[i] = null;
            }
        }

        public float Probability()
        {
            if (numberOf9TypeNeighbours != 0)
            {
                return (float)(value - numberOfFlags) / (float)numberOf9TypeNeighbours;
            }
            return 2;
        }
    }
}
